using System;
using System.Reflection;
using Fluentscript.Lib.AST;
using Fluentscript.Lib.AST.Core;
using Fluentscript.Lib.AST.Interfaces;
using Fluentscript.Lib.Parser.Core;
using Fluentscript.Lib.Parser.Integration;
using Fluentscript.Lib.Types;
using Fluentscript.Lib._Core;

namespace Fluentscript.Lib.Helpers
{
    /// <summary>
    /// Helper class for member access on internal fluentscript types and external c# types.
    /// </summary>
    public class MemberHelper
    {
        /// <summary>
        /// Gets a member access object representing the a member access.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="ctx"></param>
        /// <param name="varExp"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public static MemberAccess GetMemberAccess(AstNode node, Context ctx, Expr varExp, string memberName, IAstVisitor visitor)
        {
            var isVariableExp = varExp.IsNodeType(NodeTypes.SysVariable);
            var variableName = isVariableExp ? ((VariableExpr) varExp).Name : string.Empty;

            // CASE 1: External function call "user.create"
            if (isVariableExp && FunctionHelper.IsExternalFunction(ctx.ExternalFunctions, variableName, memberName))
                return new MemberAccess(MemberMode.FunctionExternal) {Name = variableName, MemberName = memberName};

            // CASE 2. Static method call: "Person.Create" 
            if (isVariableExp)
            {
                var result = MemberHelper.IsExternalTypeName(ctx.Memory, ctx.Types, variableName);
                if (result.Success)
                    return MemberHelper.GetExternalTypeMember(node, (Type) result.Item, variableName, null, memberName, true);
            }
            
            // CASE 3: Module
            if (varExp.IsNodeType(NodeTypes.SysVariable))
            {
                var name = varExp.ToQualifiedName();
                if (!ctx.Memory.Contains(name))
                {
                    var modresult = ResolveSymbol(varExp.SymScope, name);
                    if (modresult != null) return modresult;
                }
            }

            // CASE 4: Nested member.
            var res = varExp.Visit(visitor);
            if (res is MemberAccess )
            {
                return res as MemberAccess;
            }

            var obj = res as LObject;
            // Check for empty objects.
            ExceptionHelper.NotNull(node,  obj, "member access");

            var type = obj.Type;

            // Case 3: Method / Property on FluentScript type
            bool isCoreType = obj.Type.IsBuiltInType();
            if (isCoreType)
            {
                var result = MemberHelper.GetLangBasicTypeMember(node, ctx.Methods, obj, memberName);
                return result;
            }

            // CASE 4: Method / Property on External/Host language type (C#)
            var lclass = obj as LClass;
            var lclassType = lclass.Type as LClassType;
            var member = MemberHelper.GetExternalTypeMember(node, lclassType.DataType, variableName, lclass.Value, memberName, false);
            return member;
        }


        /// <summary>
        /// Resolve a symbol that is either a module or function.
        /// </summary>
        /// <param name="symscope"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MemberAccess ResolveSymbol(ISymbols symscope, string name)
        {
            var symbol = symscope.GetSymbol(name);

            // Case 2: Module.
            if (symbol.Category == SymbolCategory.Module)
            {
                var msym = symbol as SymbolModule;
                var mem = new MemberAccess(MemberMode.Module);
                mem.Type = msym.DataType;
                mem.Scope = msym.Scope;
                return mem;
            }

            // Case 3: Function.
            if (symbol.Category == SymbolCategory.Func)
            {
                var sfunc = symbol as SymbolFunction;
                var mem = new MemberAccess(MemberMode.FunctionScript);
                mem.MemberName = name;
                mem.Type = sfunc.DataType;
                mem.Expr = sfunc.FuncExpr as Expr;
                return mem;
            }
            return null;
        }


        /// <summary>
        /// Gets a see MemberAccess object that represents the instance, member name and other information on the member access expression.
        /// </summary>
        /// <param name="node">The Ast node associated with the member access operation</param>
        /// <param name="methods">The collection of registered methods on various types</param>
        /// <param name="obj">The object on which the member access is being performed.</param>
        /// <param name="memberName">The name of the member to get.</param>
        /// <returns></returns>
        public static MemberAccess GetLangBasicTypeMember(AstNode node, RegisteredMethods methods, LObject obj, string memberName)
        {
            var type = obj.Type;
            
            // Get the methods implementation LTypeMethods for this basic type 
            // e.g. string,  date,  time,  array , map
            // e.g. LStringType  LDateType, LTimeType, LArrayType, LMapType
            var typeMethods = methods.Get(type);
            var memberExists = typeMethods.HasMember(obj, memberName);

            // 1. Can set non-existing map properties
            if (type == LTypes.Map && !memberExists)
            {
                var nonExistMAccess = new MemberAccess(MemberMode.PropertyMember);
                nonExistMAccess.Name = type.Name;
                nonExistMAccess.Instance = obj;
                nonExistMAccess.MemberName = memberName;
                nonExistMAccess.Type = type;
                nonExistMAccess.MemberMissing = true;
                return nonExistMAccess;
            }

            // 2. Check that the member exists.
            if (!memberExists)
                throw ExceptionHelper.BuildRunTimeException(node, "Property or Member : " + memberName + " does not exist");

            // 3. It's either a Property or method
            var isProp = typeMethods.HasProperty(obj, memberName);
            var mode = isProp ? MemberMode.PropertyMember : MemberMode.MethodMember;
            var maccess = new MemberAccess(mode);
            maccess.Name = type.Name;
            maccess.Instance = obj;
            maccess.MemberName = memberName;
            maccess.Type = type;
            return maccess;
        }


        /// <summary>
        /// Gets a see MemberAccess object that represents the instance, member name and other information on the member access expression.
        /// </summary>
        /// <param name="node">The Ast node associated with the member access operation</param>
        /// <param name="type">The data type of the external object c#.</param>
        /// <param name="varName">The name associated with the object. e.g. user.FirstName, "user".</param>
        /// <param name="obj">The object on which the member access is being performed.</param>
        /// <param name="memberName">The name of the member to get.</param>
        /// <param name="isStatic">Whether or not this is a static access.</param>
        /// <returns></returns>
        public static MemberAccess GetExternalTypeMember(AstNode node, Type type, string varName, object obj, string memberName, bool isStatic)
        {
            // 1. Get all the members on the type that match the name.
            var members = type.GetMember(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase);

            // 2. Check that there were members with matching name.
            if (members == null || members.Length == 0)
                throw ExceptionHelper.BuildRunTimeException(node, "Property does not exist : '" + memberName + "' ");

            // 3. Get the first member that matches the memberName.
            var matchingMember = members[0];
            var memberNameCaseIgnorant = matchingMember.Name;
            var mode = isStatic ? MemberMode.CustObjMethodStatic : MemberMode.CustObjMethodInstance;

            // 4. Store information about the member. instance, type, membername.
            var member = new MemberAccess(mode);
            member.Name = isStatic ? type.Name : varName;
            member.DataType = type;
            member.Instance = obj;
            member.MemberName = matchingMember.Name;

            // 1. Property.
            if (matchingMember.MemberType == MemberTypes.Property)
                member.Property = type.GetProperty(memberNameCaseIgnorant);

            // 2. Method
            else if (matchingMember.MemberType == MemberTypes.Method)
                member.Method = type.GetMethod(matchingMember.Name);

            else if (matchingMember.MemberType == MemberTypes.Field)
                member.Field = type.GetField(matchingMember.Name);

            return member;
        }


        /// <summary>
        /// Whether or not the variable name provided is an external host language( C# ) type name.
        /// </summary>
        /// <param name="memory">The memory space of the runtime.</param>
        /// <param name="types">The collection of registered external types.</param>
        /// <param name="varName">The name associated with the member access.</param>
        /// <returns></returns>
        public static BoolMsgObj IsExternalTypeName(Memory memory, RegisteredTypes types, string varName)
        {
            Type type = null;
            var isStatic = false;
            
            // 1. Class name : "Person" as in "Person.Create" -> so definitely a static method call on custom object.
            if (types.Contains(varName))
            {
                type = types.Get(varName);
            }
            // 2. Case insensitive Class name: "person" as in "Person.Create" but "person" variable doesn't exist.
            else if (!memory.Contains(varName))
            {
                // Only do this check for "user" -> "User" class / static method.
                var first = Char.ToUpper(varName[0]);
                var name = first + varName.Substring(1);
                if (types.Contains(name))
                {
                    type = types.Get(name);
                }
            }
            if (type != null) isStatic = true;
            return new BoolMsgObj(type, isStatic, string.Empty);
        }
    }
}
