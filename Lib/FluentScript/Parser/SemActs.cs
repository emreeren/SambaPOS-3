using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ComLib.Lang.Helpers;

namespace ComLib.Lang
{
    /// <summary>
    /// Language validation result type. info, warn, compile error.
    /// </summary>
    public enum SemActsValidationType
    {
        /// <summary>
        /// Information
        /// </summary>
        Info,


        /// <summary>
        /// Warning
        /// </summary>
        Warning,


        /// <summary>
        /// Error
        /// </summary>
        Error
    }



    /// <summary>
    /// Stores both the error and the type ( e.g. warning, error etc )
    /// </summary>
    public class SemActsResult
    {
        /// <summary>
        /// The error text
        /// </summary>
        public string Error;


        /// <summary>
        /// Validation type
        /// </summary>
        public SemActsValidationType Type;
    }



    /// <summary>
    /// Stores both the error and the type ( e.g. warning, error etc )
    /// </summary>
    public class SemActsResults
    {
        /// <summary>
        /// Intiailzie with the success flag.
        /// </summary>
        /// <param name="success"></param>
        public SemActsResults(bool success)
        {
            Success = success;
        }


        /// <summary>
        /// Whether or not the semantic actions / analysis was successful ( did not have any errors )
        /// </summary>
        public readonly bool Success;


        /// <summary>
        /// Any results( validation errors, warnings from the analyser )
        /// </summary>
        public List<SemActsResult> Results;


        /// <summary>
        /// Whether or not there were any results.
        /// </summary>
        public bool HasResults { get { return Results != null && Results.Count > 0; } }
    }



    /// <summary>
    /// Semantic Analyser to validat the AST tree.
    /// </summary>
    public class SemActs
    {
        private SemActsResults _results = new SemActsResults(false);
        private List<SemActsResult> _errors;
        private Dictionary<string, List<Action<SemActs, AstNode>>> _validators;
        private ISymbols _currentSymScope;
        private Context _ctx;

        /// <summary>
        /// Initialize
        /// </summary>
        public SemActs()
        {
            _errors = new List<SemActsResult>();
            _validators = new Dictionary<string, List<Action<SemActs, AstNode>>>();         
            AddCheck<BinaryExpr>((semacts, node) => CheckDivisionByZero(semacts, (BinaryExpr)node));
            AddCheck<NewExpr>((semacts, node) => CheckNewExpression(semacts, (NewExpr)node));
            AddCheck<VariableExpr>((semacts, node) => CheckVariable(semacts, (VariableExpr)node));
            AddCheck<FunctionCallExpr>((semacts, node) => CheckFunctionCall(semacts, (FunctionCallExpr)node));
            AddCheck<IfExpr>((semacts, node) => CheckIfFalse(semacts, node));
        }


        /// <summary>
        /// The results
        /// </summary>
        public SemActsResults Results
        {
            get { return _results; }
        }


        /// <summary>
        /// Validate the statements for errors.
        /// </summary>
        /// <param name="stmts">Statements to validate</param>
        /// <returns></returns>
        public bool Validate(List<Expr> stmts)
        {
            if (stmts == null || stmts.Count == 0)
                return true;

            _ctx = stmts[0].Ctx;

            // Reset the errors.
            _errors.Clear();
            _errors = new List<SemActsResult>();

            // Use the visitor to walk the AST tree of statements/expressions
            var visitor = new AstVisitor((astnode) => Validate(astnode));
            visitor.Visit(stmts);

            // Now check for success.
            bool success = _errors.Count == 0;
            _results = new SemActsResults(success);
            _results.Results = _errors;
            return success;
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

            int initialErrorCount = _errors.Count;
            string name = node.GetType().Name;
            if(!_validators.ContainsKey(name))
                return true;

            var rules = _validators[name];
            foreach (var rule in rules)
            {
                rule(this, node);
            }
            return initialErrorCount == _errors.Count;
        }
        

        #region Add warnings / errors
        /// <summary>
        /// Add a rule to the validation process
        /// </summary>
        /// <param name="rule"></param>
        private void AddCheck<T>(Action<SemActs,AstNode> rule)
        {
            string nodeName = typeof(T).Name;
            List<Action<SemActs, AstNode>> rules = null;
            if (_validators.ContainsKey(nodeName))
                rules = _validators[nodeName];
            else
            {
                rules = new List<Action<SemActs, AstNode>>();
                _validators[nodeName] = rules;
            }
            rules.Add(rule);
        }
        
                
        /// <summary>
        /// Add the specified error and source position of the expression
        /// </summary>
        /// <param name="errorText">Error text</param>
        /// <param name="node">The expression associated with the error</param>
        private void AddError(string errorText, AstNode node)
        {
            AddValidationResult(errorText, node, SemActsValidationType.Error);
        }


        /// <summary>
        /// Add the specified error and source position of the expression
        /// </summary>
        /// <param name="warningText">Error text</param>
        /// <param name="node">The expression associated with the warning</param>
        private void AddWarning(string warningText, AstNode node)
        {
            AddValidationResult(warningText, node, SemActsValidationType.Warning);
        }


        /// <summary>
        /// Add the specified error and source position of the expression
        /// </summary>
        /// <param name="errorText">Error text</param>
        /// <param name="node">The expression associated with the warning</param>
        /// <param name="type">Type of validation result error or warning.</param>
        private void AddValidationResult(string errorText, AstNode node, SemActsValidationType type)
        {
            string error = errorText + " at line : " + node.Ref.Line + ", pos : " + node.Ref.CharPos;
            _errors.Add(new SemActsResult() { Error = error, Type = type });
        }
        #endregion


        #region Validation Rules
        /// <summary>
        /// Checks for division by zero.
        /// </summary>
        /// <param name="semActs"></param>
        /// <param name="exp"></param>
        private void CheckDivisionByZero(SemActs semActs, BinaryExpr exp)
        {
            if(exp.Op != Operator.Divide) return;
            if (!(exp.Right is ConstantExpr)) return;

            object val = ((ConstantExpr)exp.Right).Value;
            if (val is int || val is double)
            {
                var d = Convert.ToDouble(val);
                if (d == 0)
                    AddError("Division by zero", exp.Right);
            }
        }


        /// <summary>
        /// Checks if there is an if condition that always evaluates to false.
        /// </summary>
        /// <param name="semActs"></param>
        /// <param name="node"></param>
        private void CheckIfFalse(SemActs semActs, AstNode node)
        {
            var stmt = node as IfExpr;
            if (!(stmt.Condition is ConstantExpr)) return;

            var exp = stmt.Condition as ConstantExpr;
            if (!(exp.Value is bool)) return;
            bool val = (bool)exp.Value;
            if (val == false)
                AddError("If statement condition is always false", stmt);
        }


        /// <summary>
        /// Checks function call expressions for correct number of parameters.
        /// </summary>
        /// <param name="semActs">The semantic analyser</param>
        /// <param name="exp">The functioncallexpression</param>
        private void CheckFunctionCall(SemActs semActs, FunctionCallExpr exp)
        {
            var func = _ctx.Functions.GetByName(exp.Name);

            // 1. Number of params
            if(func.Meta.Arguments.Count < exp.ParamListExpressions.Count)
                AddError("Function parameters do not match", exp);
        }


        /// <summary>
        /// Check new expressions.
        /// </summary>
        /// <param name="semActs">The semantic analyser</param>
        /// <param name="exp">The newexpression</param>
        private void CheckNewExpression(SemActs semActs, NewExpr exp)
        {
            var typeName = exp.TypeName;

            // 1. Check # params to Date 
            if (string.Compare(typeName, "Date", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                if (!LDate.CanCreateFrom(exp.ParamListExpressions.Count))
                    AddError("Unexpected number of inputs when creating date", exp);
            }
            // 2. Check # params to Time
            else if (string.Compare(typeName, "Time", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                if (!TimeTypeHelper.CanCreateTimeFrom(exp.ParamListExpressions.Count))
                    AddError("Unexpected number of inputs when creating time", exp);
            }
        }


        /// <summary>
        /// Check that variable exists and that it is not used befor it is declared.
        /// </summary>
        /// <param name="semActs">The semantic analyser</param>
        /// <param name="exp">The variable expression</param>
        private void CheckVariable(SemActs semActs, VariableExpr exp)
        {   
            if(!_currentSymScope.Contains(exp.Name))
                AddError("Variable " + exp.Name + " does not exist", exp);
        }
        #endregion
    }
}
