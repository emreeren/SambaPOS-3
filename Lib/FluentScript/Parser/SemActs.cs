using System;
using System.Collections.Generic;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.Helpers;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser
{
    public enum SemanticCheckResult
    {
        /// <summary>
        /// Valid node
        /// </summary>
        Valid, 


        /// <summary>
        /// Error on node but can still continue processing other parts of the node.
        /// </summary>
        ErrorContinue,


        /// <summary>
        /// Error on node and stop processing other parts of node.
        /// </summary>
        ErrorStop,
    }



    /// <summary>
    /// Semantic Analyser to validat the AST tree.
    /// </summary>
    public class SemActs
    {
        private RunResult _results = null;
        private List<ScriptError> _errors;
        private Dictionary<string, List<Func<SemActs, AstNode, SemanticCheckResult>>> _validators;
        private ISymbols _currentSymScope;
        private ParseStackManager _parseStk;
        private Context _ctx;

        /// <summary>
        /// Initialize
        /// </summary>
        public SemActs()
        {
            _validators = new Dictionary<string, List<Func<SemActs, AstNode, SemanticCheckResult>>>();         
            AddCheck(NodeTypes.SysBinary,          (semacts, node) => CheckDivisionByZero(semacts, (BinaryExpr)node));
            AddCheck(NodeTypes.SysNew,             (semacts, node) => CheckNewExpression(semacts, (NewExpr)node));
            AddCheck(NodeTypes.SysVariable,        (semacts, node) => CheckVariable(semacts, (VariableExpr)node));
            AddCheck(NodeTypes.SysFunctionCall,    (semacts, node) => CheckFunctionCall(semacts, (FunctionCallExpr)node));
            AddCheck(NodeTypes.SysIf,              (semacts, node) => CheckIfFalse(semacts, node));
            AddCheck(NodeTypes.SysFunctionDeclare, (semacts, node) => CheckFunctionDeclaration(semacts, (FunctionDeclareExpr)node));
        }


        /// <summary>
        /// The results
        /// </summary>
        public RunResult Results
        {
            get { return _results; }
        }


        /// <summary>
        /// Validate the statements for errors.
        /// </summary>
        /// <param name="stmts">Statements to validate</param>
        /// <returns></returns>
        public RunResult Validate(List<Expr> stmts)
        {
            var start = DateTime.Now;
            if (stmts == null || stmts.Count == 0)
                return new RunResult(start, start, true, "No nodes to validate");

            _ctx = stmts[0].Ctx;

            // Reset the errors.
            _errors = new List<ScriptError>();
            _parseStk = new ParseStackManager();

            // Use the visitor to walk the AST tree of statements/expressions
            var visitor = new AstVisitor((astnode1) => Validate(astnode1), (astnode2) =>OnNodeEnd(astnode2));
            visitor.Visit(stmts);

            var end = DateTime.Now;
            // Now check for success.
            bool success = _errors.Count == 0;
            _results = new RunResult(start, end, success, _errors);
            _results.Errors = _errors;
            return _results;
        }


        /// <summary>
        /// Validates the AST node
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool Validate(AstNode node)
        {
            if (node is Expr)
            {
                _currentSymScope = ((Expr)node).SymScope;
            }
            OnNodeStart(node);

            int initialErrorCount = _errors.Count;
            string name = node.Nodetype;
            if(!_validators.ContainsKey(name))
                return true;

            var rules = _validators[name];
            foreach (var rule in rules)
            {
                rule(this, node);
            }
            return initialErrorCount == _errors.Count;
        }


        public void OnNodeStart(AstNode node)
        {
            if (node.IsNodeType(NodeTypes.SysFunctionDeclare))
            {
                _parseStk.PushNamed("function_declares", NodeTypes.SysFunctionDeclare);
                var count = _parseStk.CountOf("function_declares");
                var name = node.ToQualifiedName();
                if (count > 1)
                    AddErrorCode(ErrorCodes.Func1001, node, name);
            }    
        }


        public void OnNodeEnd(AstNode node)
        {
            if(node.IsNodeType(NodeTypes.SysFunctionDeclare))
                _parseStk.PopNamed("function_declares", NodeTypes.SysFunctionDeclare);
        }


        #region Add warnings / errors
        /// <summary>
        /// Add a rule to the validation process
        /// </summary>
        /// <param name="rule"></param>
        private void AddCheck(string nodeType, Func<SemActs,AstNode, SemanticCheckResult> rule)
        {
            List<Func<SemActs, AstNode, SemanticCheckResult>> rules = null;
            if (_validators.ContainsKey(nodeType))
                rules = _validators[nodeType];
            else
            {
                rules = new List<Func<SemActs, AstNode, SemanticCheckResult>>();
                _validators[nodeType] = rules;
            }
            rules.Add(rule);
        }
        
                
        /// <summary>
        /// Add the specified error and source position of the expression
        /// </summary>
        /// <param name="errorText">Error text</param>
        /// <param name="node">The expression associated with the error</param>
        private SemanticCheckResult AddError(string errorText, AstNode node)
        {
            AddSemanticError("", errorText, node, ScriptErrorType.Error);
            return SemanticCheckResult.ErrorContinue;
        }


        /// <summary>
        /// Add the specified error and source position of the expression
        /// </summary>
        /// <param name="errorText">Error text</param>
        /// <param name="node">The expression associated with the error</param>
        private SemanticCheckResult NodeError(string errorText, AstNode node)
        {
            AddSemanticError("", errorText, node, ScriptErrorType.Error);
            return SemanticCheckResult.ErrorStop;
        }


        /// <summary>
        /// Add the specified error and source position of the expression
        /// </summary>
        /// <param name="code">The error code from ErrorCodes class</param>
        /// <param name="node">The expression associated with the error</param>
        private SemanticCheckResult AddErrorCode(string code, AstNode node, string arg1 = null, string arg2 = null)
        {
            var error = ErrorCodes.GetError(code);
            var errorText = error.SupportsParams ? error.Format(arg1) : error.Message;
            AddSemanticError(code, errorText, node, ScriptErrorType.Error);
            return SemanticCheckResult.ErrorContinue;
        }


        /// <summary>
        /// Add the specified error and source position of the expression
        /// </summary>
        /// <param name="warningText">Error text</param>
        /// <param name="node">The expression associated with the warning</param>
        private void AddWarning(string warningText, AstNode node)
        {
            AddSemanticError("", warningText, node, ScriptErrorType.Warning);
        }


        /// <summary>
        /// Add the specified error and source position of the expression
        /// </summary>
        /// <param name="errorCode">The distinct error code.</param>
        /// <param name="errorText">Error text</param>
        /// <param name="node">The expression associated with the warning</param>
        /// <param name="errorType">Type of error.</param>
        private void AddSemanticError(string errorCode, string errorText, AstNode node, ScriptErrorType errorType)
        {
            string errormsg = errorText + " at line : " + node.Ref.Line + ", pos : " + node.Ref.CharPos;
            var error = new ScriptError();
            error.Line = node.Ref.Line;
            error.File = node.Ref.ScriptName;
            error.Column = node.Ref.CharPos;
            error.ErrorType = errorType.ToString();
            error.ErrorCode = errorCode;
            error.Message = errormsg;
            _errors.Add(error);
        }
        #endregion


        #region Validation Rules
        /// <summary>
        /// Checks for division by zero.
        /// </summary>
        /// <param name="semActs"></param>
        /// <param name="exp"></param>
        private SemanticCheckResult CheckDivisionByZero(SemActs semActs, BinaryExpr exp)
        {
            if(exp.Op != Operator.Divide) return SemanticCheckResult.Valid;
            if (!(exp.Right.IsNodeType(NodeTypes.SysConstant))) return SemanticCheckResult.Valid;

            var val = (LObject)((ConstantExpr)exp.Right).Value;
            if (val.Type == LTypes.Number)
            {
                var d = ((LNumber)val).Value;
                if (d == 0)
                    AddError("Division by zero", exp.Right);
            }
            return SemanticCheckResult.Valid;
        }


        /// <summary>
        /// Checks if there is an if condition that always evaluates to false.
        /// </summary>
        /// <param name="semActs"></param>
        /// <param name="node"></param>
        private SemanticCheckResult CheckIfFalse(SemActs semActs, AstNode node)
        {
            var stmt = node as IfExpr;
            if (!(stmt.Condition.IsNodeType(NodeTypes.SysConstant))) 
                return SemanticCheckResult.Valid;

            var exp = stmt.Condition as ConstantExpr;
            if (!(exp.Value is bool)) return SemanticCheckResult.Valid;
            bool val = (bool)exp.Value;
            if (val == false)
                return AddError("If statement condition is always false", stmt);

            return SemanticCheckResult.Valid;
        }


        private SemanticCheckResult CheckFunctionDeclaration(SemActs semActs, FunctionDeclareExpr exp)
        {
            // 1. Number of params
            var func = exp.Function;
            var initialErrorCount = _errors.Count;

            if (func.Meta.Arguments.Count > 12)
                AddErrorCode(ErrorCodes.Func1004, exp);

            // 2. Too many aliases on function
            if (func.Meta.Aliases != null && func.Meta.Aliases.Count > 5)
                AddError(ErrorCodes.Func1005, exp);

            // 3. Parameter named arguments?
            if (func.Meta.ArgumentsLookup.ContainsKey("arguments"))
                AddErrorCode(ErrorCodes.Func1003, exp);

            return initialErrorCount == _errors.Count
                       ? SemanticCheckResult.Valid
                       : SemanticCheckResult.ErrorContinue;
        }



        /// <summary>
        /// Checks function call expressions for correct number of parameters.
        /// </summary>
        /// <param name="semActs">The semantic analyser</param>
        /// <param name="exp">The functioncallexpression</param>
        private SemanticCheckResult CheckFunctionCall(SemActs semActs, FunctionCallExpr exp)
        {
            var functionName = exp.ToQualifiedName();
            var exists = exp.SymScope.IsFunction(functionName);
            
            // 1. Function does not exist.
            if (!exists)
            {
                return AddErrorCode(ErrorCodes.Func1000, exp, functionName);
            }
            var sym = exp.SymScope.GetSymbol(functionName) as SymbolFunction;
            var func = sym.FuncExpr as FunctionExpr;

            // 5. Check that named parameters exist.
            foreach(var argExpr in exp.ParamListExpressions)
            {
                if(argExpr.IsNodeType(NodeTypes.SysNamedParameter))
                {
                    var argName = ((NamedParameterExpr) argExpr).Name;
                    if (!func.Meta.ArgumentsLookup.ContainsKey(argName))
                        AddErrorCode(ErrorCodes.Func1002, exp, argName);
                }
            }
            return SemanticCheckResult.Valid;
        }


        /// <summary>
        /// Check new expressions.
        /// </summary>
        /// <param name="semActs">The semantic analyser</param>
        /// <param name="exp">The newexpression</param>
        private SemanticCheckResult CheckNewExpression(SemActs semActs, NewExpr exp)
        {
            var typeName = exp.TypeName;

            // 1. Check # params to Date 
            if (string.Compare(typeName, "Date", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                if (!DateTimeTypeHelper.CanCreateDateFrom(exp.ParamListExpressions.Count))
                    return AddError("Unexpected number of inputs when creating date", exp);
            }
            // 2. Check # params to Time
            else if (string.Compare(typeName, "Time", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                if (!DateTimeTypeHelper.CanCreateTimeFrom(exp.ParamListExpressions.Count))
                    return AddError("Unexpected number of inputs when creating time", exp);
            }
            return SemanticCheckResult.Valid;
        }


        /// <summary>
        /// Check that variable exists and that it is not used befor it is declared.
        /// </summary>
        /// <param name="semActs">The semantic analyser</param>
        /// <param name="exp">The variable expression</param>
        private SemanticCheckResult CheckVariable(SemActs semActs, VariableExpr exp)
        {   
            if(!_currentSymScope.Contains(exp.Name))
                return NodeError("Variable " + exp.Name + " does not exist", exp);

            return SemanticCheckResult.Valid;
        }
        #endregion
    }
}
