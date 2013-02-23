// -----------------------------------------------------------------------------------
// Use it as you please, but keep this header.
// Author : Marcus Deecke, 2006
// Web    : www.yaowi.com
// Email  : code@yaowi.com
// -----------------------------------------------------------------------------------
using System;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Samba.Infrastructure.Data.Serializer
{
  /// <summary>
  /// This struct supports the Yaowi Framework infrastructure and is not intended to be used directly from your code. 
  /// <P>This struct records relevant object information.
  /// </summary>
  /// <remarks>
  /// Strings in a struct? Strings are reference types and structs should not contain types like this.
  /// </remarks>
  internal struct ObjectInfo
  {
    // Members
    public string Name;
    public string Type;
    public string Assembly;
    public string Value;
    // public bool HasBinaryConstructor;
    public string ConstructorParamType;
    public string ConstructorParamAssembly;

    /// <summary>
    /// ToString()
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      string n = Name;
      if (String.IsNullOrEmpty(n))
        n = "<Name not set>";

      string t = Type;
      if (String.IsNullOrEmpty(t))
        t = "<Type not set>";

      string a = Type;
      if (String.IsNullOrEmpty(a))
        a = "<Assembly not set>";

      return n + "; " + t + "; " + a;
    }

    /// <summary>
    /// Determines whether the values are sufficient to create an instance.
    /// </summary>
    /// <returns></returns>
    public bool IsSufficient
    {
      get
      {
        // Type and Assembly should be enough
        if (String.IsNullOrEmpty(Type) || String.IsNullOrEmpty(Assembly))
          return false;

        return true;
      }
    }
  }

  /// <summary>
  /// Helper class storing a Types creation information as well as providing various static methods 
  /// returning information about types.
  /// </summary>
  [Serializable]
  public class TypeInfo
  {
    #region Members & Properties

    private string typename = null;
    private string assemblyname = null;

    /// <summary>
    /// Gets or sets the Types name.
    /// </summary>
    public string TypeName
    {
      get { return typename; }
      set { typename = value; }
    }

    /// <summary>
    /// Gets or sets the Assemblys name.
    /// </summary>
    public string AssemblyName
    {
      get { return assemblyname; }
      set { assemblyname = value; }
    }

    #endregion Members & Properties

    #region Constructors

    /// <summary>
    /// Constructor.
    /// </summary>
    public TypeInfo()
    {
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="obj"></param>
    public TypeInfo(object obj)
    {
      if (obj == null)
        return;

      TypeName = obj.GetType().FullName;
      AssemblyName = obj.GetType().Assembly.FullName;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="type"></param>
    public TypeInfo(Type type)
    {
      if (type == null)
        return;

      TypeName = type.FullName;
      AssemblyName = type.Assembly.FullName;
    }

    #endregion Constructors

    #region static Helpers

    /// <summary>
    /// Determines whether a Type is a Collection type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool IsCollection(Type type)
    {
      if (typeof(ICollection).IsAssignableFrom(type))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determines whether a Type is a Dictionary type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool IsDictionary(Type type)
    {
      if (typeof(IDictionary).IsAssignableFrom(type))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determines whether a Type is a List type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool IsList(Type type)
    {
      if (typeof(IList).IsAssignableFrom(type))
      {
        return true;
      }
      return false;
    }

    /// <summary>
    /// Determines whether the typename describes an array.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool IsArray(String type)
    {
      // type.HasElementType

      // The simple way
      if (type != null && type.EndsWith("[]"))
        return true;

      return false;
    }

    /// <summary>
    /// Determines whether the Type has a binary constructor.<p>
    /// This is seen as an indication that this is binary data.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool HasBinaryConstructor(Type type)
    {
      ConstructorInfo[] cia = type.GetConstructors();
      for (int i = 0; i < cia.Length; i++)
      {
        ParameterInfo[] pai = cia[i].GetParameters();
        if (pai.Length == 1)
        {
          if (typeof(Stream).IsAssignableFrom(pai[0].ParameterType) || 
              typeof(byte[]).IsAssignableFrom(pai[0].ParameterType))
          {

            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Returns the <code>Type</code> of the constructor with exact one parameter which is
    /// either a <code>byte[]</code> or a <code>Stream</code>.<br>
    /// A a non-null returnvalue is also an inducator, that the given <code>Type</code> 
    /// has a constructor with a binary parameter.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static Type GetBinaryConstructorType(Type type)
    {
      ConstructorInfo[] cia = type.GetConstructors();
      for (int i = 0; i < cia.Length; i++)
      {
        ParameterInfo[] pai = cia[i].GetParameters();
        if (pai.Length == 1)
        {
          if (typeof(byte[]).IsAssignableFrom(pai[0].ParameterType))
          {
            return typeof(byte[]);
          }
          else if (typeof(Stream).IsAssignableFrom(pai[0].ParameterType))
          {
            return typeof(Stream);
          }
        }
      }
      return null;
    }

    /// <summary>
    /// Returns whether the specified <code>Type</code> has a constructor with exact 
    /// one parameter of <code>byte[]</code>.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool HasByteArrayConstructor(Type type)
    {
      ConstructorInfo[] cia = type.GetConstructors();
      for (int i = 0; i < cia.Length; i++)
      {
        ParameterInfo[] pai = cia[i].GetParameters();
        if(pai.Length == 1)
        {
          if (typeof(byte[]).IsAssignableFrom(pai[0].ParameterType))
          {
            return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Returns whether the specified <code>Type</code> has a constructor with exact 
    /// one parameter of <code>Stream</code>.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.Synchronized)]
    public static bool HasStreamConstructor(Type type)
    {
      ConstructorInfo[] cia = type.GetConstructors();
      for (int i = 0; i < cia.Length; i++)
      {
        ParameterInfo[] pai = cia[i].GetParameters();
        if (pai.Length == 1)
        {
          if (typeof(Stream).IsAssignableFrom(pai[0].ParameterType))
          {
            return true;
          }
        }
      }
      return false;
    }

    #endregion static Helpers
  }
}
