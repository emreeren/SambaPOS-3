
namespace Fluentscript.Lib.AST
{
    /// ------------------------------------------------------------------------------------------------
    /// remarks: This file is auto-generated from the FSGrammar specification and should not be modified.
    /// summary: This file contains all the AST for expressions at the system level.
    ///			features like control-flow e..g if, while, for, try, break, continue, return etc.
    /// version: 0.9.8.10
    /// author:  kishore reddy
    /// date:	02/25/13 04:37:02 PM
    /// ------------------------------------------------------------------------------------------------

    public class NodeTypes
    {
        public const string SysArray = "SysArray"; // 1
        public const string SysAnyOf = "SysAnyOf"; // 2
        public const string SysAssign = "SysAssign"; // 3
        public const string SysAssignMulti = "SysAssignMulti"; // 4
        public const string SysBinary = "SysBinary"; // 5
        public const string SysCompare = "SysCompare"; // 6
        public const string SysCondition = "SysCondition"; // 7
        public const string SysConstant = "SysConstant"; // 8
        public const string SysDay = "SysDay"; // 9
        public const string SysDuration = "SysDuration"; // 10
        public const string SysDate = "SysDate"; // 11
        public const string SysDateRelative = "SysDateRelative"; // 12
        public const string SysFunctionCall = "SysFunctionCall"; // 13
        public const string SysFunction = "SysFunction"; // 14
        public const string SysIndex = "SysIndex"; // 15
        public const string SysInterpolated = "SysInterpolated"; // 16
        public const string SysListCheck = "SysListCheck"; // 17
        public const string SysMap = "SysMap"; // 18
        public const string SysMemberAccess = "SysMemberAccess"; // 19
        public const string SysNamedParameter = "SysNamedParameter"; // 20
        public const string SysNegate = "SysNegate"; // 21
        public const string SysNew = "SysNew"; // 22
        public const string SysParameter = "SysParameter"; // 23
        public const string SysRun = "SysRun"; // 24
        public const string SysTable = "SysTable"; // 25
        public const string SysUnary = "SysUnary"; // 26
        public const string SysBreak = "SysBreak"; // 27
        public const string SysContinue = "SysContinue"; // 28
        public const string SysForEach = "SysForEach"; // 29
        public const string SysFor = "SysFor"; // 30
        public const string SysFunctionDeclare = "SysFunctionDeclare"; // 31
        public const string SysIf = "SysIf"; // 32
        public const string SysLambda = "SysLambda"; // 33
        public const string SysReturn = "SysReturn"; // 34
        public const string SysThrow = "SysThrow"; // 35
        public const string SysTryCatch = "SysTryCatch"; // 36
        public const string SysWhile = "SysWhile"; // 37
        public const string SysVariable = "SysVariable"; // 38

        // Other
        public const string SysBlock = "SysBlock";
        public const string SysIndexable = "SysIndexable";
        public const string SysDataType = "SysDataType";

    }
}