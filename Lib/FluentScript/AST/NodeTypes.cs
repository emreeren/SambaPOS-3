
namespace ComLib.Lang.AST
{
    /// ------------------------------------------------------------------------------------------------
    /// remarks: This file is auto-generated from the FSGrammar specification and should not be modified.
    /// summary: This file contains all the AST for expressions at the system level.
    ///			features like control-flow e..g if, while, for, try, break, continue, return etc.
    /// version: 0.9.8.10
    /// author:  kishore reddy
    /// date:	01/11/13 04:01:53 PM
    /// ------------------------------------------------------------------------------------------------

    public class NodeTypes
    {
        public const string SysArray = "SysArray"; // 1
        public const string SysAssign = "SysAssign"; // 2
        public const string SysAssignMulti = "SysAssignMulti"; // 3
        public const string SysBinary = "SysBinary"; // 4
        public const string SysCompare = "SysCompare"; // 5
        public const string SysCondition = "SysCondition"; // 6
        public const string SysConstant = "SysConstant"; // 7
        public const string SysFunctionCall = "SysFunctionCall"; // 8
        public const string SysFunction = "SysFunction"; // 9
        public const string SysIndex = "SysIndex"; // 10
        public const string SysInterpolated = "SysInterpolated"; // 11
        public const string SysMap = "SysMap"; // 12
        public const string SysMemberAccess = "SysMemberAccess"; // 13
        public const string SysNamedParameter = "SysNamedParameter"; // 14
        public const string SysNegate = "SysNegate"; // 15
        public const string SysNew = "SysNew"; // 16
        public const string SysParameter = "SysParameter"; // 17
        public const string SysUnary = "SysUnary"; // 18
        public const string SysBreak = "SysBreak"; // 19
        public const string SysContinue = "SysContinue"; // 20
        public const string SysForEach = "SysForEach"; // 21
        public const string SysFor = "SysFor"; // 22
        public const string SysFunctionDeclare = "SysFunctionDeclare"; // 23
        public const string SysIf = "SysIf"; // 24
        public const string SysLambda = "SysLambda"; // 25
        public const string SysReturn = "SysReturn"; // 26
        public const string SysThrow = "SysThrow"; // 27
        public const string SysTryCatch = "SysTryCatch"; // 28
        public const string SysWhile = "SysWhile"; // 29
        public const string SysVariable = "SysVariable"; // 30

        // Other
        public const string SysBlock = "SysBlock";
        public const string SysIndexable = "SysIndexable";
        public const string SysDataType = "SysDataType";

    }
}