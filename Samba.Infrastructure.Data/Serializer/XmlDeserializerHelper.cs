// -----------------------------------------------------------------------------------
// Use it as you please, but keep this header.
// Author : Marcus Deecke, 2006
// Web    : www.yaowi.com
// Email  : code@yaowi.com
// -----------------------------------------------------------------------------------
using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Xml;

namespace Samba.Infrastructure.Data.Serializer
{
    /// <summary>
    /// Deserializes complex objects serialized with the XmlSerializer.
    /// </summary>
    public class XmlDeserializerHelper : IDisposable
    {
        private bool ignorecreationerrors = false;
        private Hashtable typedictionary = new Hashtable(); // Parsed Types
        private Dictionary<string, Assembly> assemblycache = new Dictionary<string, Assembly>();  // Found Assemblies
        private Dictionary<string, Assembly> assemblyregister = new Dictionary<string, Assembly>(); // Predefined Assemblies
        private Dictionary<Type, TypeConverter> typeConverterCache = new Dictionary<Type, TypeConverter>(); // used TypeConverters

        private Dictionary<Type, Dictionary<int, IEntityClass>> identifyibleCache = new Dictionary<Type, Dictionary<int, IEntityClass>>();

        private IXmlSerializationTag taglib = new XmlSerializationTag();

        public IXmlSerializationTag TagLib
        {
            get { return taglib; }
            set { taglib = value; }
        }

        #region XmlDeserializer Properties

        /// <summary>
        /// Gets whether the current root node provides a type dictionary.
        /// </summary>
        protected bool HasTypeDictionary
        {
            get
            {
                if (typedictionary != null && typedictionary.Count > 0)
                    return true;

                return false;
            }
        }

        /// <summary>
        /// Gets or sets whether creation errors shall be ignored.
        /// <br>Creation errors can occur if e.g. a type has no parameterless constructor
        /// and an instance cannot be instantiated from String.
        /// </summary>
        [Description("Gets or sets whether creation errors shall be ignored.")]
        public bool IgnoreCreationErrors
        {
            get { return ignorecreationerrors; }
            set { ignorecreationerrors = value; }
        }

        #endregion XmlDeserializer Properties

        #region Deserialize

        /// <summary>
        /// Deserialzes an object from a xml file.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public object Deserialize(string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);
            return Deserialize(doc);
        }

        public object Deserialize(XmlDocument document)
        {
            XmlNode node = document.SelectSingleNode(taglib.OBJECT_TAG);
            return Deserialize(node);
        }

        public object Deserialize(XmlDocument document, Dictionary<Type, Dictionary<int, IEntityClass>> cache)
        {
            identifyibleCache = cache;
            XmlNode node = document.SelectSingleNode(taglib.OBJECT_TAG);
            return Deserialize(node);
        }


        /// <summary>
        /// Deserializes an Object from the specified XmlNode. 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public object Deserialize(XmlNode node)
        {
            // Clear previous collections
            Reset();
            AddAssemblyRegisterToCache();

            XmlNode rootnode = node;
            if (!rootnode.Name.Equals(taglib.OBJECT_TAG))
            {
                rootnode = node.SelectSingleNode(taglib.OBJECT_TAG);

                if (rootnode == null)
                {
                    throw new ArgumentException("Invalid node. The specified node or its direct children do not contain a " + taglib.OBJECT_TAG + " tag.", "XmlNode node");
                }
            }

            // Load TypeDictionary
            this.typedictionary = ParseTypeDictionary(rootnode);

            // Get the Object
            object obj = GetObject(rootnode);

            if (obj != null)
            {
                obj = GetProperties(obj, rootnode);
            }
            else
                Console.WriteLine("Object is null");

            return obj;
        }

        /// <summary>
        /// Parses the TypeDictionary (if given).
        /// </summary>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        /// <remarks>
        /// The TypeDictionary is Hashtable in which TypeInfo items are stored.
        /// </remarks>
        protected Hashtable ParseTypeDictionary(XmlNode parentNode)
        {
            Hashtable dict = new Hashtable();

            XmlNode dictNode = parentNode.SelectSingleNode(taglib.TYPE_DICTIONARY_TAG);
            if (dictNode == null)
                return dict;

            object obj = GetObject(dictNode);

            if (obj != null && typeof(Hashtable).IsAssignableFrom(obj.GetType()))
            {
                dict = (Hashtable)obj;
                GetProperties(dict, dictNode);
            }

            return dict;
        }

        #endregion Deserialize

        #region Properties & values

        /// <summary>
        /// Reads the properties of the specified node and sets them an the parent object.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="node"></param>
        /// <remarks>
        /// This is the central method which is called recursivly!
        /// </remarks>
        protected object GetProperties(object parent, XmlNode node)
        {
            if (parent == null)
                return parent;

            // Get the properties
            XmlNodeList nl = node.SelectNodes(taglib.PROPERTIES_TAG + "/" + taglib.PROPERTY_TAG);

            // Properties found?
            if (nl == null || nl.Count == 0)
            {
                // No properties found... perhaps a collection?
                if (TypeInfo.IsCollection(parent.GetType()))
                {
                    SetCollectionValues((ICollection)parent, node);
                }
                else
                {
                    // Nothing to do here
                    return parent;
                }
            }

            // Loop the properties found
            foreach (XmlNode prop in nl)
            {
                // Collect the nodes type information about the property to deserialize
                ObjectInfo oi = GetObjectInfo(prop);

                // Enough info?
                if (oi.IsSufficient && !String.IsNullOrEmpty(oi.Name))
                {
                    object obj = null;

                    // Create an instance, but note: arrays always need the size for instantiation
                    if (TypeInfo.IsArray(oi.Type))
                    {
                        int c = GetArrayLength(prop);
                        obj = CreateArrayInstance(oi, c);
                    }
                    else
                    {
                        obj = CreateInstance(oi);
                    }

                    // Process the property's properties (recursive call of this method)
                    if (obj != null)
                    {
                        obj = GetProperties(obj, prop);
                    }

                    // Setting the instance (or null) as the property's value
                    PropertyInfo pi = parent.GetType().GetProperty(oi.Name);
                    if (obj != null && pi != null)
                    {
                        pi.SetValue(parent, obj, null);
                    }
                }
            }

            var identifyible = parent as IEntityClass;
            if (identifyible != null)
            {
                var list = GetIdentifyibleTypeList(identifyible.GetType());
                if (!list.ContainsKey(identifyible.Id))
                {
                    list.Add(identifyible.Id, identifyible);
                    return list[identifyible.Id];
                }
                return list[identifyible.Id];
            }

            return parent;
        }

        private Dictionary<int, IEntityClass> GetIdentifyibleTypeList(Type type)
        {
            if (!identifyibleCache.ContainsKey(type))
                identifyibleCache.Add(type, new Dictionary<int, IEntityClass>());
            return identifyibleCache[type];
        }

        #region Collections

        /// <summary>
        /// Sets the entries on an ICollection implementation.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="parentNode"></param>
        protected void SetCollectionValues(ICollection coll, XmlNode parentNode)
        {
            if (typeof(IDictionary).IsAssignableFrom(coll.GetType()))
            {
                // IDictionary
                SetDictionaryValues((IDictionary)coll, parentNode);
                return;
            }
            else if (typeof(IList).IsAssignableFrom(coll.GetType()))
            {
                // IList
                SetListValues((IList)coll, parentNode);
                return;
            }
        }

        /// <summary>
        /// Sets the entries on an IList implementation.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="parentNode"></param>
        protected void SetListValues(IList list, XmlNode parentNode)
        {
            // Get the item nodes
            XmlNodeList nlitems = parentNode.SelectNodes(taglib.ITEMS_TAG + "/" + taglib.ITEM_TAG);

            // Loop them
            for (int i = 0; i < nlitems.Count; i++)
            {
                XmlNode nodeitem = nlitems[i];

                // Create an instance
                object obj = GetObject(nodeitem);

                // Process the properties
                obj = GetProperties(obj, nodeitem);

                if (list.IsFixedSize)
                    list[i] = obj;
                else
                    list.Add(obj);
            }
        }

        /// <summary>
        /// Sets the entries of an IDictionary implementation.
        /// </summary>
        /// <param name="coll"></param>
        /// <param name="parentNode"></param>
        protected void SetDictionaryValues(IDictionary dictionary, XmlNode parentNode)
        {
            // Get the item nodes
            XmlNodeList nlitems = parentNode.SelectNodes(taglib.ITEMS_TAG + "/" + taglib.ITEM_TAG);

            // Loop them
            for (int i = 0; i < nlitems.Count; i++)
            {
                XmlNode nodeitem = nlitems[i];

                // Retrieve the single property
                string path = taglib.PROPERTIES_TAG + "/" + taglib.PROPERTY_TAG + "[@" + taglib.NAME_TAG + "='" + taglib.NAME_ATT_KEY_TAG + "']";
                XmlNode nodekey = nodeitem.SelectSingleNode(path);

                path = taglib.PROPERTIES_TAG + "/" + taglib.PROPERTY_TAG + "[@" + taglib.NAME_TAG + "='" + taglib.NAME_ATT_VALUE_TAG + "']";
                XmlNode nodeval = nodeitem.SelectSingleNode(path);

                // Create an instance of the key
                object objkey = GetObject(nodekey);
                object objval = null;

                // Try to get the value
                if (nodeval != null)
                {
                    objval = GetObject(nodeval);
                }

                // Set the entry if the key is not null
                if (objkey != null)
                {
                    // Set the entry's value if its is not null and process its properties
                    if (objval != null)
                        objval = GetProperties(objval, nodeval);

                    dictionary.Add(objkey, objval);
                }
            }
        }

        #endregion Collections

        #endregion Properties & values

        #region Creating instances and types

        /// <summary>
        /// Creates an instance by the contents of the given XmlNode.
        /// </summary>
        /// <param name="node"></param>
        protected object GetObject(XmlNode node)
        {
            ObjectInfo oi = GetObjectInfo(node);

            if (TypeInfo.IsArray(oi.Type))
            {
                int c = GetArrayLength(node);
                return CreateArrayInstance(oi, c);
            }

            return CreateInstance(oi);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        protected Assembly GetAssembly(String assembly)
        {
            Assembly a = null;

            // Cached already?
            if (assemblycache.ContainsKey(assembly))
            {
                return (Assembly)assemblycache[assembly];
            }

            // Shortnamed, version independent assembly name
            int x = assembly.IndexOf(",");
            string ass = null;
            if (x > 0)
                ass = assembly.Substring(0, x);

            // Cached already?
            if (ass != null && assemblycache.ContainsKey(ass))
            {
                return (Assembly)assemblycache[ass];
            }


            // Try to get the Type in any way
            try
            {
                String path = Path.GetDirectoryName(assembly);
                if (!String.IsNullOrEmpty(path))
                {
                    // Assembly cached already?
                    if (assemblycache.ContainsKey(assembly))
                    {
                        // Cached
                        a = (Assembly)assemblycache[assembly];
                    }
                    else
                    {
                        // Not cached, yet
                        a = Assembly.LoadFrom(path);
                        assemblycache.Add(assembly, a);
                    }
                }
                else
                {
                    try
                    {
                        // Try to load the assembly version independent
                        if (ass != null)
                        {
                            // Assembly cached already?
                            if (assemblycache.ContainsKey(ass))
                            {
                                // Cached
                                a = (Assembly)assemblycache[ass];
                            }
                            else
                            {
                                // Not cached, yet
                                a = Assembly.Load(ass);
                                assemblycache.Add(ass, a);
                            }
                        }
                        else
                        {
                            // Assembly cached already?
                            if (assemblycache.ContainsKey(assembly))
                            {
                                // Cached
                                a = (Assembly)assemblycache[assembly];
                            }
                            else
                            {
                                // Not cached, yet
                                a = Assembly.Load(assembly);
                                assemblycache.Add(assembly, a);
                            }
                        }
                    }
                    catch
                    {
                        // Loading the assembly version independent failed: load it with the given version.

                        if (assemblycache.ContainsKey(assembly))
                        {
                            // Cached
                            a = (Assembly)assemblycache[assembly];
                        }
                        else
                        {
                            // Not cached, yet
                            a = Assembly.Load(assembly);
                            assemblycache.Add(assembly, a);
                        }
                    }
                }
            }
            catch { /* ok, we did not get the Assembly */ }

            return a;
        }

        /// <summary>
        /// Creates a type from the specified assembly and type names included in the TypeInfo parameter.
        /// <b>In case of failure null will be returned.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected Type CreateType(TypeInfo info)
        {
            return CreateType(info.AssemblyName, info.TypeName);
        }

        /// <summary>
        /// Creates a type from the specified assembly and type names. 
        /// <b>In case of failure null will be returned.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected Type CreateType(String assembly, string type)
        {
            Type t = null;

            try
            {
                Assembly a = GetAssembly(assembly);

                if (a != null)
                {
                    t = a.GetType(type);
                }
            }
            catch { /* ok, we did not get the Type */ }

            return t;
        }

        /// <summary>
        /// Creates an instance of an Array by the specified ObjectInfo.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private Array CreateArrayInstance(ObjectInfo info, int length)
        {
            Contract.Requires(0 <= info.Type.Length - 2);
            Contract.Requires(length >= 0);
            // The Type name of an array ends with "[]"
            // Exclude this to get the real Type
            string typename = info.Type.Substring(0, info.Type.Length - 2);

            Type t = CreateType(info.Assembly, typename);

            Array arr = Array.CreateInstance(t, length);

            return arr;
        }

        /// <summary>
        /// Creates an instance by the specified ObjectInfo.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        private object CreateInstance(ObjectInfo info)
        {
            try
            {
                // Enough information to create an instance?
                if (!info.IsSufficient)
                    return null;

                object obj = null;

                // Get the Type
                Type type = CreateType(info.Assembly, info.Type);

                if (type == null)
                {
                    throw new Exception("Assembly or Type not found.");
                }

                // Ok, we've got the Type, now try to create an instance.

                // Is there a binary constructor?
                if (!String.IsNullOrEmpty(info.ConstructorParamType))
                {
                    Object ctorparam = null;
                    byte[] barr = null;

                    if (!String.IsNullOrEmpty(info.Value))
                    {
                        barr = System.Convert.FromBase64String(info.Value);

                        Type ctorparamtype = CreateType(info.ConstructorParamAssembly, info.ConstructorParamType);

                        // What type of parameter is needed?
                        if (typeof(Stream).IsAssignableFrom(ctorparamtype))
                        {
                            // Stream
                            ctorparam = new MemoryStream(barr);
                        }
                        else if (typeof(byte[]).IsAssignableFrom(ctorparamtype))
                        {
                            // byte[]
                            ctorparam = barr;
                        }
                    }

                    obj = Activator.CreateInstance(type, new object[] { ctorparam });

                    return obj;
                }

                // Until now only properties with binary data support constructors with parameters

                // Problem: only parameterless constructors or constructors with one parameter
                // which can be converted from String are supported.
                // Failure Example:
                // string s = new string();
                // string s = new string("");
                // This cannot be compiled, but the follwing works;
                // string s = new string("".ToCharArray());
                // The TypeConverter provides a way to instantite objects by non-parameterless 
                // constructors if they can be converted fro String
                try
                {
                    TypeConverter tc = GetConverter(type);
                    if (tc.CanConvertFrom(typeof(string)))
                    {
                        obj = tc.ConvertFrom(info.Value);
                        return obj;
                    }
                }
                catch { ; }

                obj = Activator.CreateInstance(type);

                if (obj == null)
                    throw new Exception("Instance could not be created.");

                return obj;
            }
            catch (Exception e)
            {
                string msg = "Creation of an instance failed. Type: " + info.Type + " Assembly: " + info.Assembly + " Cause: " + e.Message;
                if (IgnoreCreationErrors)
                {
                    return null;
                }
                else
                    throw new Exception(msg, e);
            }
        }

        #endregion Creating instances and types

        #region Misc


        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private TypeInfo TranslateTypeByKey(String key)
        {
            Contract.Requires(key != null);
            if (HasTypeDictionary)
            {
                if (this.typedictionary.ContainsKey(key))
                {
                    TypeInfo ti = (TypeInfo)this.typedictionary[key];

                    return ti;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets an ObjectInfo instance by the attributes of the specified XmlNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private ObjectInfo GetObjectInfo(XmlNode node)
        {
            ObjectInfo oi = new ObjectInfo();

            String typekey = GetAttributeValue(node, taglib.TYPE_TAG);
            TypeInfo ti = TranslateTypeByKey(typekey);

            if (ti != null)
            {
                oi.Type = ti.TypeName;
                oi.Assembly = ti.AssemblyName;
            }

            // If a TypeDictionary is given, did we find the necessary information to create an instance?
            // If not, try to get information by the Node itself
            if (!oi.IsSufficient)
            {
                oi.Type = GetAttributeValue(node, taglib.TYPE_TAG);
                oi.Assembly = GetAttributeValue(node, taglib.ASSEMBLY_TAG);
            }

            // Name and Value
            oi.Name = GetAttributeValue(node, taglib.NAME_TAG);
            oi.Value = node.InnerText;

            // Binary Constructor
            ti = GetBinaryConstructorType(node);

            if (ti != null)
            {
                // Binary constructor info given
                oi.ConstructorParamType = ti.TypeName;
                oi.ConstructorParamAssembly = ti.AssemblyName;

                // Make sure to read the value from the binary data Node (setting oi.Value = node.InnerText as above is a bit dirty)
                XmlNode datanode = node.SelectSingleNode(taglib.BINARY_DATA_TAG);
                if (datanode != null)
                {
                    oi.Value = datanode.InnerText;
                }
                else
                {
                    datanode = node.SelectSingleNode(taglib.CONSTRUCTOR_TAG);
                    if (datanode != null)
                    {
                        datanode = datanode.SelectSingleNode(taglib.BINARY_DATA_TAG);
                        if (datanode != null)
                        {
                            oi.Value = datanode.InnerText;
                        }
                    }
                }
            }

            return oi;
        }

        /// <summary>
        /// Returns the length of the array of a arry-XmlNode.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        protected int GetArrayLength(XmlNode parent)
        {
            XmlNodeList nl = parent.SelectNodes(taglib.ITEMS_TAG + "/" + taglib.ITEM_TAG);
            int c = 0;

            if (nl != null)
                c = nl.Count;

            return c;
        }

        /// <summary>
        /// Returns the value or the attribute with the specified name from the given node if it is not null or empty.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        protected string GetAttributeValue(XmlNode node, string name)
        {
            if (node == null || String.IsNullOrEmpty(name))
                return null;

            String val = null;
            XmlAttribute att = node.Attributes[name];

            if (att != null)
            {
                val = att.Value;
                if (val.Equals(""))
                    val = null;
            }
            return val;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected TypeInfo GetBinaryConstructorType(XmlNode node)
        {
            if (node == null)
                return null;

            XmlNode ctornode = node.SelectSingleNode(taglib.CONSTRUCTOR_TAG);
            if (ctornode == null)
                return null;

            String typekey = GetAttributeValue(ctornode, taglib.TYPE_TAG);

            TypeInfo ti = TranslateTypeByKey(typekey);

            return ti;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        protected bool HasBinaryConstructor(XmlNode node)
        {
            if (node == null)
                return false;

            XmlNode ctornode = node.SelectSingleNode(taglib.CONSTRUCTOR_TAG);
            if (ctornode == null)
                return false;

            XmlNode binnode = ctornode.SelectSingleNode(taglib.BINARY_DATA_TAG);
            if (binnode == null)
                return false;

            return true;
        }

        /// <summary>
        /// Returns the TypeConverter of a Type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        protected TypeConverter GetConverter(Type type)
        {
            TypeConverter retConverter = null;

            if (!typeConverterCache.TryGetValue(type, out retConverter))
            {
                retConverter = TypeDescriptor.GetConverter(type);
                typeConverterCache[type] = retConverter;
            }

            return retConverter;
        }

        /// <summary>
        /// Registers an Assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <remarks>
        /// Register Assemblies which are not known at compile time, e.g. PlugIns or whatever.
        /// </remarks>
        public void RegisterAssembly(Assembly assembly)
        {
            string ass = assembly.FullName;

            int x = ass.IndexOf(",");
            if (x > 0)
                ass = ass.Substring(0, x);

            assemblyregister[ass] = assembly;
        }

        /// <summary>
        /// Registers a list of assemblies.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public int RegisterAssemblies(List<Assembly> assemblies)
        {
            if (assemblies == null)
                return 0;

            int cnt = 0;

            foreach (Assembly ass in assemblies)
            {
                this.RegisterAssembly(ass);
                cnt++;
            }

            return cnt;
        }

        /// <summary>
        /// Registers a list of assemblies.
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public int RegisterAssemblies(Assembly[] assemblies)
        {
            if (assemblies == null)
                return 0;

            int cnt = 0;

            for (int i = 0; i < assemblies.Length; i++)
            {
                this.RegisterAssembly(assemblies[i]);
                cnt++;
            }

            return cnt;
        }

        /// <summary>
        /// Adds the assembly register items to the assembly cache.
        /// </summary>
        protected void AddAssemblyRegisterToCache()
        {
            if (assemblyregister == null)
                return;

            Dictionary<string, Assembly>.Enumerator de = assemblyregister.GetEnumerator();
            while (de.MoveNext())
            {
                assemblycache[de.Current.Key] = de.Current.Value;
            }
        }

        /// <summary>
        /// Clears the typedictionary collection.
        /// </summary>
        public void Reset()
        {
            if (this.typedictionary != null)
                this.typedictionary.Clear();
        }

        /// <summary>
        /// Dispose, release references.
        /// </summary>
        public void Dispose()
        {
            Reset();

            if (this.assemblycache != null)
                this.assemblycache.Clear();

            if (assemblyregister != null)
                assemblyregister.Clear();

            if (typeConverterCache != null)
                typeConverterCache.Clear();
        }

        #endregion Misc
    }
}
