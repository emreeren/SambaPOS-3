using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;
// <lang:using>

// </lang:using>

namespace Fluentscript.Lib.Parser.Core
{
    internal class Limits
    {
        private Context _ctx;
        private bool _hasSettings;


        public Limits(Context ctx)
        {
            _ctx = ctx;
        }


        /// <summary>
        /// Reset the limit.
        /// </summary>
        public void Init()
        {
            _hasSettings = _ctx.Settings != null;
        }


        /// <summary>
        /// Checks the length of the script.
        /// </summary>
        /// <param name="script"></param>
        internal void CheckScriptLength(string script)
        {
            if (string.IsNullOrEmpty(script)) return;
            if (_ctx.Settings.MaxScriptLength > 0 && script.Length > _ctx.Settings.MaxScriptLength)
                throw BuildLimitException(null, "script length", _ctx.Settings.MaxScriptLength);
        }


        /// <summary>
        /// Check the lenght of the string.
        /// </summary>
        /// <param name="node">The ast node associated with the operation.</param>
        /// <param name="val"></param>
        internal void CheckStringLength(AstNode node, object val)
        {
            if (val == null || val == LObjects.Null) return;
            if (!(val is LString)) return;

            // Check limit
            if (_hasSettings && _ctx.Settings.MaxScopeStringVariablesLength > 0)
            {
                var strval = ((LString) val).Value;
                if (strval.Length > _ctx.Settings.MaxScopeStringVariablesLength)
                    throw BuildLimitException(node, "string lenth", _ctx.Settings.MaxScopeStringVariablesLength);
            }
        }


        /// <summary>
        /// Checks that the string is with in limits.
        /// </summary>
        /// <param name="node">The ast node associated with the operation.</param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        internal void CheckStringLength(AstNode node, string left, string right)
        {
            // Check limit
            if (_hasSettings && _ctx.Settings.MaxStringLength > 0)
            {
                if (left.Length + right.Length > _ctx.Settings.MaxStringLength)
                    throw BuildLimitException(node, "string lenth", _ctx.Settings.MaxStringLength);
            }
        }


        /// <summary>
        /// Check the maximum number of exceptions that can occurr
        /// </summary>
        /// <param name="node">The ast node associated with the string operation.</param>
        /// <param name="incrementFirst"></param>
        internal void CheckExceptions(AstNode node, bool incrementFirst = true)
        {
            if (incrementFirst) _ctx.State.ExceptionCount++;

            // Check limit
            if (_hasSettings && _ctx.Settings.MaxExceptions > 0)
            {
                if (_ctx.State.ExceptionCount > _ctx.Settings.MaxExceptions)
                    throw BuildLimitException(node, "# Exceptions", _ctx.Settings.MaxExceptions);
            }
        }


        /// <summary>
        /// Check the call stack limit.
        /// </summary>
        /// <param name="node">The ast node associated with the operation.</param>
        /// <param name="lastIndex">The current call stack index</param>
        internal void CheckCallStack(AstNode node, int lastIndex)
        {
            if (_hasSettings && _ctx.Settings.MaxCallStack > 0)
            {
                // Check for call limit (includes recursion).
                if (lastIndex + 1 > _ctx.Settings.MaxCallStack)
                {
                    // Throw.
                    throw BuildLimitException(node, "Function call stack", _ctx.Settings.MaxCallStack);
                }
            }
        }


        /// <summary>
        /// Checks the loop limit.
        /// </summary>        
        /// <param name="node">The ast node associated with the operation.</param>
        internal void CheckLoop(AstNode node)
        {
            if (!_ctx.Settings.HasMaxLoopLimit) return;

            // Increase loop limit.
            _ctx.State.LoopCount++;

            // Check if max loop limit
            if (_ctx.State.LoopCount > _ctx.Settings.MaxLoopLimit)
                throw BuildLimitException(node, "loops", _ctx.Settings.MaxLoopLimit);
        }


        /// <summary>
        /// Checks the limit of number of scope variables
        /// </summary>        
        /// <param name="node">The ast node associated with the operation.</param>
        internal void CheckScopeCount(AstNode node)
        {
            if (_hasSettings && _ctx.Settings.MaxScopeVariables > 0)
            {                
                // Check if max loop limit
                if (_ctx.Memory.Total > _ctx.Settings.MaxScopeVariables)
                    throw BuildLimitException(node, "variables", _ctx.Settings.MaxScopeVariables);
            }
        }


        /// <summary>
        /// Checks the limit of the total length of all string variables.
        /// </summary>
        /// <param name="node"></param>
        internal void CheckScopeStringLength(AstNode node)
        {
            if (_hasSettings && _ctx.Settings.MaxScopeStringVariablesLength > 0)
            {
                // Check if max loop limit
                if (_ctx.Memory.TotalStringLength > _ctx.Settings.MaxScopeStringVariablesLength)
                    throw BuildLimitException(node, "total string variables length", _ctx.Settings.MaxScopeStringVariablesLength);
            }
        }


        #region Parser limits
        /// <summary>
        /// Checks the number of statements.
        /// </summary>
        /// <param name="node"></param>
        internal void CheckParserStatement(AstNode node)
        {
            _ctx.State.StatementCount++;
            if (_hasSettings && _ctx.Settings.MaxStatements > 0 && _ctx.State.StatementCount >= _ctx.Settings.MaxStatements)
                throw BuildLimitException(node, "statements : " + _ctx.Settings.MaxStatements);
        }


        /// <summary>
        /// Checks the nested statement depth.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="statementNestedCount">The number of nested statements</param>
        internal void CheckParserStatementNested(TokenData token, int statementNestedCount)
        {
            if (_hasSettings && _ctx.Settings.MaxStatementsNested > 0 && statementNestedCount > _ctx.Settings.MaxStatementsNested)
                throw BuildLimitExceptionFromToken(token, "nested statements", _ctx.Settings.MaxStatementsNested);
        }


        /// <summary>
        /// Checks the nested statement depth.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="functionCallCount">The number of nested statements</param>
        internal void CheckParserFuncCallNested(TokenData token, int functionCallCount)
        {
            if (_hasSettings && _ctx.Settings.MaxFuncCallNested > 0 && functionCallCount > _ctx.Settings.MaxFuncCallNested)
                throw BuildLimitExceptionFromToken(token, "nested function call", _ctx.Settings.MaxFuncCallNested);
        }


        /// <summary>
        /// Check the number of continous expressions.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="expressionCount"></param>
        internal void CheckParserExpression(AstNode node, int expressionCount)
        {
            if (_hasSettings && _ctx.Settings.MaxConsequetiveExpressions > 0 && expressionCount > _ctx.Settings.MaxConsequetiveExpressions)
                throw BuildLimitException(node, "consequetive expressions (" + expressionCount + ")", _ctx.Settings.MaxConsequetiveExpressions);

        }


        /// <summary>
        /// Check the number of member access expressions.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="memberAccessCount"></param>
        internal void CheckParserMemberAccess(AstNode node, int memberAccessCount)
        {
            if (_hasSettings && _ctx.Settings.MaxConsequetiveMemberAccess > 0 && memberAccessCount > _ctx.Settings.MaxConsequetiveMemberAccess)
                throw BuildLimitException(node, "consequetive member access ( '(),[],.' )", _ctx.Settings.MaxConsequetiveMemberAccess);
        }


        /// <summary>
        /// Checks the number of function parameters.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="paramCount"></param>
        internal void CheckParserFunctionParams(AstNode node, int paramCount)
        {
            if (_hasSettings && _ctx.Settings.MaxFuncParams > 0 && paramCount > _ctx.Settings.MaxFuncParams)
                throw BuildLimitException(node, "function parameters", _ctx.Settings.MaxFuncParams);
        }
        #endregion


        private LangLimitException BuildLimitException(AstNode node, string error, int limit = -1)
        {
            if (limit != -1)
                error = "Limit for : " + error + " reached at " + limit;

            string script = "";
            int lineNumber = 0;
            int charPos = 0;
            if (node != null && node.Ref != null)
            {
                script = node.Ref.ScriptName;
                lineNumber = node.Ref.Line;
                charPos = node.Ref.CharPos;
            }
            var ex = new LangLimitException(error, script, lineNumber);
            ex.Error.Column = charPos;
            return ex;
        }


        private LangLimitException BuildLimitExceptionFromToken(TokenData token, string error, int limit = -1)
        {
            if (limit != -1)
                error = "Limit for : " + error + " reached at " + limit;

            string script = "";
            int lineNumber = 0;
            int charPos = 0;
            if (token != null)
            {
                lineNumber = token.Line;
                charPos = token.LineCharPos;
            }
            var ex = new LangLimitException(error, script, lineNumber);
            ex.Error.Column = charPos;
            return ex;
        }
    }
}
