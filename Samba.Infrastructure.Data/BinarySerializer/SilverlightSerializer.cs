// SilverlightSerializer by Mike Talbot
//                          http://whydoidoit.com
//                          email:   mike.talbot@alterian.com
//                          twitter: mike_talbot
//
// This code is free to use, no warranty is offered or implied.
// If you redistribute, please retain this header.

#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

#endregion


namespace Samba.Infrastructure.Data.BinarySerializer
{
    /// <summary>
    ///   Indicates that a property or field should not be serialized
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class)]
    public class DoNotSerialize : Attribute
    {

    }
    /// <summary>
    /// Used in checksum mode to flag a property as not being part
    /// of the "meaning" of an object - i.e. two objects with the
    /// same checksum "mean" the same thing, even if some of the
    /// properties are different, those properties would not be
    /// relevant to the purpose of the object
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DoNotChecksum : Attribute
    {
    }
    /// <summary>
    /// Attribute used to flag IDs this can be useful for check object
    /// consistence when the serializer is in a mode that does not 
    /// serialize identifiers
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class SerializerId : Attribute
    {
    }
    /// <summary>
    /// Always use an event to create instances of this type
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CreateUsingEvent : Attribute
    {
    }


    public interface ISerializeObject
    {
        object[] Serialize(object target);
        object Deserialize(object[] data);
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SerializerAttribute : Attribute
    {
        internal Type SerializesType;
        public SerializerAttribute(Type serializesType)
        {
            SerializesType = serializesType;
        }
    }

    /// <summary>
    ///   Silverlight/.NET compatible binary serializer with suppression support
    ///   produces compact representations, suitable for further compression
    /// </summary>
    public static class SilverlightSerializer
    {
        private static readonly Dictionary<RuntimeTypeHandle, IEnumerable<FieldInfo>> FieldLists = new Dictionary<RuntimeTypeHandle, IEnumerable<FieldInfo>>();
        private static readonly Dictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> PropertyLists = new Dictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        private static readonly Dictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> ChecksumLists = new Dictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        [ThreadStatic]
        internal static List<RuntimeTypeHandle> KnownTypes;
        [ThreadStatic]
        private static Dictionary<object, int> _seenObjects;

        [ThreadStatic]
        private static Dictionary<int, object> _loadedObjects;
        [ThreadStatic]
        internal static List<string> PropertyIds;

        [ThreadStatic]
        private static Stack<Dictionary<int, object>> _loStack;
        [ThreadStatic]
        private static Stack<Dictionary<object, int>> _soStack;
        [ThreadStatic]
        internal static Stack<List<RuntimeTypeHandle>> KtStack;
        [ThreadStatic]
        internal static Stack<List<string>> PiStack;
        [ThreadStatic]
        private static bool _isChecksum;
        [ThreadStatic]
        public static bool IgnoreIds;

        [ThreadStatic]
        public static bool IsReference;

        public static T Copy<T>(T item)
        {
            return (T)Deserialize(Serialize(item));
        }

        /// <summary>
        /// Arguments for a missing type event
        /// </summary>
        public class TypeMappingEventArgs : EventArgs
        {
            /// <summary>
            /// The missing types name
            /// </summary>
            public string TypeName = String.Empty;
            /// <summary>
            /// Supply a type to use instead
            /// </summary>
            public Type UseType = null;
        }

        /// <summary>
        /// Arguments for object creation event
        /// </summary>
        public class ObjectMappingEventArgs : EventArgs
        {
            /// <summary>
            /// The type that cannot be
            /// </summary>
            public Type TypeToConstruct;
            /// <summary>
            /// Supply a type to use instead
            /// </summary>
            public object Instance = null;
        }


        /// <summary>
        /// Event that is fired if a particular type cannot be instantiated
        /// </summary>
        public static event EventHandler<ObjectMappingEventArgs> CreateType;


        internal static void InvokeCreateType(ObjectMappingEventArgs e)
        {
            EventHandler<ObjectMappingEventArgs> handler = CreateType;
            if (handler != null)
                handler(null, e);
        }


        /// <summary>
        /// Event that is fired if a particular type cannot be found
        /// </summary>
        public static event EventHandler<TypeMappingEventArgs> MapMissingType;


        internal static void InvokeMapMissingType(TypeMappingEventArgs e)
        {
            EventHandler<TypeMappingEventArgs> handler = MapMissingType;
            if (handler != null)
                handler(null, e);
        }

        /// <summary>
        /// Put the serializer into Checksum mode
        /// </summary>
        public static bool IsChecksum
        {
            get
            {
                return _isChecksum;
            }
            set
            {
                _isChecksum = value;
            }
        }

        //public static Type SerializerType
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Deserialize to a type
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] array) where T : class
        {
            return Deserialize(array) as T;

        }

        /// <summary>
        /// Deserialize from a stream to a type
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static T Deserialize<T>(Stream stream) where T : class
        {
            return Deserialize(stream) as T;
        }

        /// <summary>
        /// Get a checksum for an item.  Checksums "should" be different 
        /// for every object that has a different "meaning".  You can
        /// flag properties as DoNotChecksum if that helps to keep decorative
        /// properties away from the checksum whilst including meaningful ones
        /// </summary>
        /// <param name="item">The object to checksum</param>
        /// <returns>A checksum string, this includes no illegal characters and can be used as a file name</returns>
        public static string GetChecksum(object item)
        {
            if (item == null)
                return "";
            byte[] checksum = new byte[17];
            checksum.Initialize();
            var isChecksum = IsChecksum;
            IsChecksum = true;
            var toBytes = Serialize(item);
            IsChecksum = isChecksum;

            for (var i = 0; i < toBytes.Length; i++)
            {
                checksum[i & 15] ^= toBytes[i];
            }
            return item.GetType().Name + "-" + toBytes.Count().ToString() + "-" + Encode(checksum);
        }

        private static string Encode(byte[] checksum)
        {
            var s = Convert.ToBase64String(checksum);
            return s.Aggregate("", (current, c) => current + (Char.IsLetterOrDigit(c)
                                                                  ? c
                                                                  : Char.GetNumericValue(c)));
        }


        //Holds a reference to the custom serializers
        private static readonly Dictionary<Type, ISerializeObject> Serializers = new Dictionary<Type, ISerializeObject>();
        //Dictionary to ensure we only scan an assembly once
        private static readonly Dictionary<Assembly, bool> Assemblies = new Dictionary<Assembly, bool>();

        /// <summary>
        /// Register all of the custom serializers in an assembly
        /// </summary>
        /// <param name="assembly">Leave blank to register the assembly that the method is called from, or pass an assembly</param>
        public static void RegisterSerializationAssembly(Assembly assembly = null)
        {
            if (assembly == null)
                assembly = Assembly.GetCallingAssembly();
            if (Assemblies.ContainsKey(assembly))
                return;
            Assemblies[assembly] = true;
            ScanAllTypesForAttribute((tp, attr) =>
            {
                Serializers[((SerializerAttribute)attr).SerializesType] = Activator.CreateInstance(tp) as ISerializeObject;
            }, assembly, typeof(SerializerAttribute));
        }

        //Function to be called when scanning types
        internal delegate void ScanTypeFunction(Type type, Attribute attribute);

        /// <summary>
        /// Scan all of the types in an assembly for a particular attribute
        /// </summary>
        /// <param name="function">The function to call</param>
        /// <param name="assembly">The assembly to scan</param>
        /// <param name="attribute">The attribute to look for</param>
        internal static void ScanAllTypesForAttribute(ScanTypeFunction function, Assembly assembly, Type attribute = null)
        {
            try
            {
                foreach (var tp in assembly.GetTypes())
                {
                    if (attribute != null)
                    {
                        var attrs = Attribute.GetCustomAttributes(tp, attribute, false);
                        if (attrs != null)
                        {
                            foreach (var attr in attrs)
                                function(tp, attr);
                        }
                    }
                    else
                        function(tp, null);
                }
            }
            catch (Exception)
            {


            }
        }
        /// <summary>
        /// Write persistence debugging information to the debug output window
        /// often used with Verbose
        /// </summary>
        public static bool IsLoud;
        /// <summary>
        /// Write all types, even if they are known, often used with Loud mode
        /// </summary>
        public static bool Verbose;

        /// <summary>
        ///   Caches and returns property info for a type
        /// </summary>
        /// <param name = "itm">The type that should have its property info returned</param>
        /// <returns>An enumeration of PropertyInfo objects</returns>
        /// <remarks>
        ///   It should be noted that the implementation converts the enumeration returned from reflection to an array as this more than double the speed of subsequent reads
        /// </remarks>
        internal static IEnumerable<PropertyInfo> GetPropertyInfo(RuntimeTypeHandle itm)
        {
            lock (PropertyLists)
            {
                IEnumerable<PropertyInfo> ret = null;
                if (!IsChecksum)
                {
                    if (!PropertyLists.TryGetValue(itm, out ret))
                    {
                        ret = Type.GetTypeFromHandle(itm).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType.GetCustomAttributes(typeof(DoNotSerialize), true).Count() == 0 && p.GetCustomAttributes(typeof(DoNotSerialize), true).Count() == 0 && !(p.GetIndexParameters().Count() > 0) && (p.GetSetMethod() != null)).ToArray();
                        PropertyLists[itm] = ret;
                    }
                }
                else
                {
                    if (!ChecksumLists.TryGetValue(itm, out ret))
                    {
                        ret = Type.GetTypeFromHandle(itm).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType.GetCustomAttributes(typeof(DoNotSerialize), true).Count() == 0 && p.GetCustomAttributes(typeof(DoNotSerialize), true).Count() == 0 && p.GetCustomAttributes(typeof(DoNotChecksum), true).Count() == 0 && !(p.GetIndexParameters().Count() > 0) && (p.GetSetMethod() != null)).ToArray();
                        ChecksumLists[itm] = ret;
                    }
                }
                return IgnoreIds && ret != null
                           ? ret.Where(p => p.GetCustomAttributes(typeof(SerializerId), true).Count() == 0)
                           : ret;
            }
        }


        public static IEnumerable<PropertyInfo> GetProperties(Type item)
        {
            bool tempChecksum = IsChecksum;
            bool tempIgnoreIds = IgnoreIds;
            IsChecksum = false;
            IgnoreIds = false;

            IEnumerable<PropertyInfo> result = GetPropertyInfo(item.TypeHandle);

            IsChecksum = tempChecksum;
            IgnoreIds = tempIgnoreIds;

            return result;
        }
        public static IEnumerable<FieldInfo> GetFields(Type item)
        {
            bool tempChecksum = IsChecksum;
            bool tempIgnoreIds = IgnoreIds;
            IsChecksum = false;
            IgnoreIds = false;

            IEnumerable<FieldInfo> result = GetFieldInfo(item.TypeHandle);

            IsChecksum = tempChecksum;
            IgnoreIds = tempIgnoreIds;

            return result;
        }

        //static SilverlightSerializer()
        //{
        //    SerializerType = typeof(BinarySerializer);
        //}

        /// <summary>
        ///   Caches and returns field info for a type
        /// </summary>
        /// <param name = "itm">The type that should have its field info returned</param>
        /// <returns>An enumeration of FieldInfo objects</returns>
        /// <remarks>
        ///   It should be noted that the implementation converts the enumeration returned from reflection to an array as this more than double the speed of subsequent reads
        /// </remarks>
        internal static IEnumerable<FieldInfo> GetFieldInfo(RuntimeTypeHandle itm)
        {
            lock (FieldLists)
            {
                IEnumerable<FieldInfo> ret = null;
                if (FieldLists.ContainsKey(itm))
                    ret = FieldLists[itm];
                else
                {
                    ret = FieldLists[itm] = Type.GetTypeFromHandle(itm).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetField).Where(p => p.FieldType.GetCustomAttributes(typeof(DoNotSerialize), true).Count() == 0 && p.GetCustomAttributes(typeof(DoNotSerialize), false).Count() == 0).ToArray();
                }

                return IsChecksum ? ret.Where(p => p.GetCustomAttributes(typeof(DoNotChecksum), true).Count() == 0) : ret;

            }
        }

        /// <summary>
        ///   Returns a token that represents the name of the property
        /// </summary>
        /// <param name = "name">The name for which to return a token</param>
        /// <returns>A 2 byte token representing the name</returns>
        internal static ushort GetPropertyDefinitionId(string name)
        {
            lock (PropertyIds)
            {
                var ret = PropertyIds.IndexOf(name);
                if (ret >= 0)
                    return (ushort)ret;
                PropertyIds.Add(name);
                return (ushort)(PropertyIds.Count - 1);
            }
        }

        [ThreadStatic]
        public static int currentVersion;



        public static object Deserialize(IStorage storage)
        {
            bool v = Verbose;
            Verbose = false;
            CreateStacks();
            try
            {
                KtStack.Push(KnownTypes);
                PiStack.Push(PropertyIds);
                _loStack.Push(_loadedObjects);
                _loadedObjects = new Dictionary<int, object>();

                IStorage serializer = storage;
                serializer.StartDeserializing();
                var ob = DeserializeObject(new Entry()
                {
                    Name = "root"
                }, serializer);
                serializer.FinishedDeserializing();
                return ob;
            }

            finally
            {
                IsReference = false;
                KnownTypes = KtStack.Pop();
                PropertyIds = PiStack.Pop();
                _loadedObjects = _loStack.Pop();
                Verbose = v;
            }
        }




        /// <summary>
        /// Deserializes from a stream, potentially into an existing instance
        /// </summary>
        /// <param name="inputStream">Stream to deserialize from</param>
        /// <param name="instance">Instance to use</param>
        /// <returns></returns>
        public static object Deserialize(Stream inputStream, object instance = null)
        {
            // this version always uses the BinarySerializer
            var v = Verbose;
            CreateStacks();
            try
            {
                KtStack.Push(KnownTypes);
                PiStack.Push(PropertyIds);
                _loStack.Push(_loadedObjects);
                _loadedObjects = new Dictionary<int, object>();

                var rw = new BinaryReader(inputStream);
                var version = rw.ReadString();
                currentVersion = Int32.Parse(version.Substring(4));

                if (currentVersion >= 5)
                {
                    inputStream.Position = 0;
                    BinarySerializer serializer = new BinarySerializer(rw.ReadBytes((int)inputStream.Length));
                    serializer.StartDeserializing();
                    var ob = DeserializeObject(new Entry()
                    {
                        Name = "root"
                    }, serializer);
                    serializer.FinishedDeserializing();
                    return ob;
                }

                var count = rw.ReadInt32();
                if (currentVersion >= 3)
                    Verbose = rw.ReadBoolean();
                PropertyIds = new List<string>();
                KnownTypes = new List<RuntimeTypeHandle>();

                for (var i = 0; i < count; i++)
                {
                    var typeName = rw.ReadString();
                    var tp = Type.GetType(typeName);
                    if (tp == null)
                    {
                        var map = new TypeMappingEventArgs
                        {
                            TypeName = typeName
                        };
                        InvokeMapMissingType(map);
                        tp = map.UseType;
                    }
                    if (!Verbose)
                        if (tp == null)
                            throw new ArgumentException(String.Format("Cannot reference type {0} in this context", typeName));
                    KnownTypes.Add(tp.TypeHandle);
                }
                count = rw.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    PropertyIds.Add(rw.ReadString());
                }

                object obj = OldDeserializeObject(rw, null, instance);


                return obj;
            }
            finally
            {
                IsReference = false;
                KnownTypes = KtStack.Pop();
                PropertyIds = PiStack.Pop();
                _loadedObjects = _loStack.Pop();
                Verbose = v;
            }
        }

        /// <summary>
        ///   Convert a previously serialized object from a byte array 
        ///   back into a .NET object
        /// </summary>
        /// <param name = "bytes">The data stream for the object</param>
        /// <returns>The rehydrated object represented by the data supplied</returns>
        public static object Deserialize(byte[] bytes)
        {
            using (MemoryStream inputStream = new MemoryStream(bytes))
            {
                return Deserialize(inputStream);
            }
        }

        /// <summary>
        ///   Convert a previously serialized object from a byte array 
        ///   back into a .NET object
        /// </summary>
        /// <param name = "bytes">The data stream for the object</param>
        /// <returns>The rehydrated object represented by the data supplied</returns>
        public static void DeserializeInto(byte[] bytes, object instance)
        {
            using (MemoryStream inputStream = new MemoryStream(bytes))
            {
                Deserialize(inputStream, instance);
            }
        }


        /// <summary>
        ///   Creates a set of stacks on the current thread
        /// </summary>
        private static void CreateStacks()
        {
            if (PiStack == null)
                PiStack = new Stack<List<string>>();
            if (KtStack == null)
                KtStack = new Stack<List<RuntimeTypeHandle>>();
            if (_loStack == null)
                _loStack = new Stack<Dictionary<int, object>>();
            if (_soStack == null)
                _soStack = new Stack<Dictionary<object, int>>();
        }

        #region Old Deserialization
        /// <summary>
        ///   Deserializes an object or primitive from the stream
        /// </summary>
        /// <param name = "reader">The reader of the binary file</param>
        /// <param name = "itemType">The expected type of the item being read (supports compact format)</param>
        /// <returns>The value read from the file</returns>
        /// <remarks>
        ///   The function is supplied with the type of the property that the object was stored in (if known) this enables
        ///   a compact format where types only have to be specified if they differ from the expected one
        /// </remarks>
        private static object OldDeserializeObject(BinaryReader reader, Type itemType = null, object instance = null)
        {
            var tpId = (ushort)reader.ReadUInt16();
            if (tpId == 0xFFFE)
                return null;

            IsReference = false;

            //Lookup the value type if necessary
            if (tpId != 0xffff)
                itemType = Type.GetTypeFromHandle(KnownTypes[tpId]);

            object obj = null;
            if (itemType != null)
            {
                //Check for custom serialization
                if (Serializers.ContainsKey(itemType))
                {
                    //Read the serializer and its data
                    var serializer = Serializers[itemType];
                    object[] data = OldDeserializeObject(reader, typeof(object[])) as object[];
                    return serializer.Deserialize(data);
                }

                //Check if this is a simple value and read it if so
                if (IsSimpleType(itemType))
                {
                    if (itemType.IsEnum)
                    {
                        return Enum.Parse(itemType, ReadValue(reader, typeof(int)).ToString(), true);
                    }
                    return ReadValue(reader, itemType);
                }
            }
            //See if we should lookup this object or create a new one
            var found = reader.ReadChar();
            int refItemID;
            if (found == 'S') //S is for Seen
            {
                IsReference = true;
                refItemID = reader.ReadInt32();
                return _loadedObjects[refItemID];
            }
            else
            {
                refItemID = _loadedObjects.Keys.Count;
            }
            if (itemType != null)
            {
                //Otherwise create the object
                if (itemType.IsArray)
                {
                    int baseCount = reader.ReadInt32();

                    if (baseCount == -1)
                    {
                        return OldDeserializeMultiDimensionArray(itemType, reader, baseCount);
                    }
                    else
                    {
                        return OldDeserializeArray(itemType, reader, baseCount);
                    }
                }

                obj = instance ?? CreateObject(itemType);
                _loadedObjects.Add(refItemID, obj);
            }
            //Check for collection types)
            if (obj is IDictionary)
                return OldDeserializeDictionary(obj as IDictionary, itemType, reader);
            if (obj is IList)
                return OldDeserializeList(obj as IList, itemType, reader);


            //Otherwise we are serializing an object
            return OldDeserializeObjectAndProperties(obj, itemType, reader);

        }

        /// <summary>
        ///   Deserializes an array of values
        /// </summary>
        /// <param name = "itemType">The type of the array</param>
        /// <param name = "reader">The reader of the stream</param>
        /// <returns>The deserialized array</returns>
        /// <remarks>
        ///   This routine optimizes for arrays of primitives and bytes
        /// </remarks>
        private static object OldDeserializeArray(Type itemType, BinaryReader reader, int count)
        {
            // If the count is -1 at this point, then it is being called from the
            // deserialization of a multi-dimensional array - so we need
            // to read the size of the array
            if (count == -1)
            {
                count = reader.ReadInt32();
            }

            //Get the expected element type
            var elementType = itemType.GetElementType();
            //Optimize for byte arrays
            if (elementType == typeof(byte))
            {
                var ret = reader.ReadBytes(count);
                _loadedObjects.Add(_loadedObjects.Keys.Count, ret);
                return ret;
            }

            //Create an array of the correct type
            var array = Array.CreateInstance(elementType, count);
            _loadedObjects.Add(_loadedObjects.Keys.Count, array);
            //Check whether the array contains primitives, if it does we don't
            //need to store the type of each member
            if (IsSimpleType(elementType))
                for (var l = 0; l < count; l++)
                {
                    array.SetValue(ReadValue(reader, elementType), l);
                }
            else
                for (var l = 0; l < count; l++)
                {
                    array.SetValue(OldDeserializeObject(reader, elementType), l);
                }
            return array;
        }

        /// <summary>
        ///   Deserializes a multi-dimensional array of values
        /// </summary>
        /// <param name = "itemType">The type of the array</param>
        /// <param name = "reader">The reader of the stream</param>
        /// <param name="count">The base size of the multi-dimensional array</param>
        /// <returns>The deserialized array</returns>
        /// <remarks>
        ///   This routine deserializes values serialized on a 'row by row' basis, and
        ///   calls into DeserializeArray to do this
        /// </remarks>
        private static object OldDeserializeMultiDimensionArray(Type itemType, BinaryReader reader, int count)
        {
            //Read the number of dimensions the array has
            var dimensions = reader.ReadInt32();
            var totalLength = reader.ReadInt32();

            int rowLength = 0;

            // Establish the length of each array element
            // and get the total 'row size'
            int[] lengths = new int[dimensions];
            int[] indices = new int[dimensions];

            for (int item = 0; item < dimensions; item++)
            {
                lengths[item] = reader.ReadInt32();
                rowLength += lengths[item];
                indices[item] = 0;
            }

            int cols = lengths[lengths.Length - 1];
            //int cols = dimensions == 1 ? 1 : lengths[lengths.Length - 1];

            //Get the expected element type
            var elementType = itemType.GetElementType();



            Array sourceArrays = Array.CreateInstance(elementType, lengths);
            OldDeserializeArrayPart(sourceArrays, 0, indices, itemType, reader);
            return sourceArrays;
        }

        private static void OldDeserializeArrayPart(Array sourceArrays, int i, int[] indices, Type itemType, BinaryReader binaryReader)
        {
            int length = sourceArrays.GetLength(i);
            for (var l = 0; l < length; l++)
            {
                indices[i] = l;
                if (i != sourceArrays.Rank - 2)
                    OldDeserializeArrayPart(sourceArrays, i + 1, indices, itemType, binaryReader);
                else
                {
                    Array sourceArray = (Array)OldDeserializeArray(itemType, binaryReader, -1);
                    int cols = sourceArrays.GetLength(i + 1);
                    for (int arrayStartIndex = 0; arrayStartIndex < cols; arrayStartIndex++)
                    {
                        indices[i + 1] = arrayStartIndex;
                        sourceArrays.SetValue(sourceArray.GetValue(arrayStartIndex), indices);
                    }
                }
            }
        }

        /// <summary>
        ///   Deserializes a dictionary from storage, handles generic types with storage optimization
        /// </summary>
        /// <param name = "o">The newly created dictionary</param>
        /// <param name = "itemType">The type of the dictionary</param>
        /// <param name = "reader">The binary reader for the current bytes</param>
        /// <returns>The dictionary object updated with the values from storage</returns>
        private static object OldDeserializeDictionary(IDictionary o, Type itemType, BinaryReader reader)
        {
            Type keyType = null;
            Type valueType = null;
            if (itemType.IsGenericType)
            {
                var types = itemType.GetGenericArguments();
                keyType = types[0];
                valueType = types[1];
            }

            var count = reader.ReadInt32();
            var list = new List<object>();
            for (var i = 0; i < count; i++)
            {
                list.Add(OldDeserializeObject(reader, keyType));
            }
            for (var i = 0; i < count; i++)
            {
                o[list[i]] = OldDeserializeObject(reader, valueType);
            }
            return o;
        }

        /// <summary>
        ///   Deserialize a list from the data stream
        /// </summary>
        /// <param name = "o">The newly created list</param>
        /// <param name = "itemType">The type of the list</param>
        /// <param name = "reader">The reader for the current bytes</param>
        /// <returns>The list updated with values from the stream</returns>
        private static object OldDeserializeList(IList o, Type itemType, BinaryReader reader)
        {
            Type valueType = null;
            if (itemType.IsGenericType)
            {
                var types = itemType.GetGenericArguments();
                valueType = types[0];
            }

            var count = reader.ReadInt32();
            var list = new List<object>();
            for (var i = 0; i < count; i++)
            {
                o.Add(OldDeserializeObject(reader, valueType));
            }
            return o;
        }

        /// <summary>
        ///   Deserializes a class based object that is not a collection, looks for both public properties and fields
        /// </summary>
        /// <param name = "o">The object being deserialized</param>
        /// <param name = "itemType">The type of the object</param>
        /// <param name = "reader">The reader for the current stream of bytes</param>
        /// <returns>The object updated with values from the stream</returns>
        private static object OldDeserializeObjectAndProperties(object o, Type itemType, BinaryReader reader)
        {
            OldDeserializeProperties(reader, itemType, o);
            OldDeserializeFields(reader, itemType, o);
            return o;
        }


        /// <summary>
        ///   Deserializes the properties of an object from the stream
        /// </summary>
        /// <param name = "reader">The reader of the bytes in the stream</param>
        /// <param name = "itemType">The type of the object</param>
        /// <param name = "o">The object to deserialize</param>
        private static void OldDeserializeProperties(BinaryReader reader, Type itemType, object o)
        {
            //Get the number of properties
            var propCount = reader.ReadByte();
            int length = 0;
            if (Verbose)
                length = reader.ReadInt32();
            if (o == null)
            {
                reader.BaseStream.Seek(length, SeekOrigin.Current);
                return;
            }
            for (var i = 0; i < propCount; i++)
            {
                //Get a property name identifier
                var propId = reader.ReadUInt16();
                //Lookup the name
                var propName = PropertyIds[propId];
                //Use the name to find the type
                var propType = itemType.GetProperty(propName);
                //Deserialize the value
                var value = OldDeserializeObject(reader, propType != null ? propType.PropertyType : null);
                if (propType != null && value != null)
                {
                    try
                    {
                        if (propType.PropertyType == typeof(string))
                        {
                            propType.SetValue(o, value.ToString(), null);
                        }
                        else
                            propType.SetValue(o, value, null);
                    }
                    catch (Exception)
                    {
                        //Suppress cases where the old value is no longer compatible with the new property type


                    }

                }
            }
        }

        /// <summary>
        ///   Deserializes the fields of an object from the stream
        /// </summary>
        /// <param name = "reader">The reader of the bytes in the stream</param>
        /// <param name = "itemType">The type of the object</param>
        /// <param name = "o">The object to deserialize</param>
        private static void OldDeserializeFields(BinaryReader reader, Type itemType, object o)
        {
            var fieldCount = reader.ReadByte();
            int length = 0;
            if (Verbose)
                length = reader.ReadInt32();
            if (o == null)
            {
                reader.BaseStream.Seek(length, SeekOrigin.Current);
                return;
            }
            for (var i = 0; i < fieldCount; i++)
            {
                var fieldId = reader.ReadUInt16();
                var fieldName = PropertyIds[fieldId];
                var fieldType = itemType.GetField(fieldName);
                var value = OldDeserializeObject(reader, fieldType != null ? fieldType.FieldType : null);
                if (fieldType != null && value != null)
                {
                    try
                    {
                        fieldType.SetValue(o, value);
                    }
                    catch (Exception)
                    {
                        //Suppress cases where the old value is no longer compatible with the new property type
                    }

                }
            }
        }


        #endregion

        #region New Deserialization

        /// <summary>
        ///   Deserializes an object or primitive from the stream
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="storage"></param>
        /// <returns>The value read from the file</returns>
        /// <remarks>
        ///   The function is supplied with the type of the property that the object was stored in (if known) this enables
        ///   a compact format where types only have to be specified if they differ from the expected one
        /// </remarks>
        private static object DeserializeObject(Entry entry, IStorage storage)
        {
            //Get a name for the item
            storage.DeserializeGetName(entry);
            //Update the core info including a property getter
            if (entry.MustHaveName) UpdateEntryWithName(entry);
            //Start to deserialize
            var candidate = storage.StartDeserializing(entry);
            if (candidate != null)
            {
                storage.FinishDeserializing(entry);
                return candidate;
            }


            var itemType = entry.StoredType;

            if (itemType == null)
                return null;

            IsReference = false;
            object obj = null;
            //Check for custom serialization
            if (Serializers.ContainsKey(itemType))
            {
                //Read the serializer and its data
                var serializer = Serializers[itemType];
                var data =
                    (object[])
                    DeserializeObject(new Entry()
                    {
                        Name = "data",
                        StoredType = typeof(object[])
                    }, storage);
                object result = serializer.Deserialize(data);
                storage.FinishDeserializing(entry);
                return result;
            }

            //Check if this is a simple value and read it if so
            if (IsSimpleType(itemType))
            {
                if (itemType.IsEnum)
                {
                    //return Enum.Parse(itemType, storage.ReadValue<int>("value").ToString(), true);
                    //return Enum.Parse(itemType, storage.ReadSimpleValue<int>().ToString(), true);
                    //return Convert.ChangeType(storage.ReadSimpleValue(Enum.GetUnderlyingType(itemType)), itemType, System.Globalization.CultureInfo.InvariantCulture);
                    return Enum.Parse(itemType, storage.ReadSimpleValue(Enum.GetUnderlyingType(itemType)).ToString(), true);
                }
                return storage.ReadSimpleValue(itemType);
            }
            //See if we should lookup this object or create a new one
            bool isReference;
            int objectID = storage.BeginReadObject(out isReference);
            if (isReference)
            {
                IsReference = true;
                storage.EndReadObject();
                storage.FinishDeserializing(entry);
                if (storage.SupportsOnDemand && !_loadedObjects.ContainsKey(objectID))
                {
                    storage.BeginOnDemand(objectID);
                    _loadedObjects[objectID] = DeserializeObject(entry, storage);
                    storage.EndOnDemand();
                }
                return _loadedObjects[objectID];
            }
            else if (storage.SupportsOnDemand)
            {
                if (_loadedObjects.ContainsKey(objectID))
                {
                    obj = _loadedObjects[objectID];
                    storage.EndReadObject();
                    storage.FinishDeserializing(entry);
                    return obj;
                }
            }


            //Otherwise create the object
            if (itemType.IsArray)
            {
                int baseCount;
                bool isMultiDimensionArray = storage.IsMultiDimensionalArray(out baseCount);

                if (isMultiDimensionArray)
                {
                    object result = DeserializeMultiDimensionArray(itemType, storage, objectID);
                    storage.EndReadObject();
                    storage.FinishDeserializing(entry);
                    return result;
                }
                else
                {
                    object result = DeserializeArray(itemType, storage, baseCount, objectID);
                    storage.EndReadObject();
                    storage.FinishDeserializing(entry);
                    return result;

                }
            }

            obj = entry.Value ?? CreateObject(itemType);
            _loadedObjects[objectID] = obj;


            //Check for collection types)
            if (obj is IDictionary)
            {
                object result = DeserializeDictionary(obj as IDictionary, itemType, storage);
                storage.EndReadObject();
                storage.FinishDeserializing(entry);
                return result;
            }
            if (obj is IList)
            {
                object result = DeserializeList(obj as IList, itemType, storage);
                storage.EndReadObject();
                storage.FinishDeserializing(entry);
                return result;
            }

            //Otherwise we are serializing an object
            object result2 = DeserializeObjectAndProperties(obj, itemType, storage);
            storage.EndReadObject();
            storage.FinishDeserializing(entry);
            //Check for null
            if (obj is Nuller)
            {
                return null;
            }
            return result2;
        }

        /// <summary>
        ///   Deserializes an array of values
        /// </summary>
        /// <param name = "itemType">The type of the array</param>
        /// <param name="storage"></param>
        /// <param name="count"></param>
        /// <returns>The deserialized array</returns>
        /// <remarks>
        ///   This routine optimizes for arrays of primitives and bytes
        /// </remarks>
        private static object DeserializeArray(Type itemType, IStorage storage, int count, int objectID)
        {
            Type elementType = itemType.GetElementType();
            Array result = null;

            if (IsSimpleType(elementType))
            {
                result = storage.ReadSimpleArray(elementType, count);
                _loadedObjects[objectID] = result;
            }
            else
            {
                result = Array.CreateInstance(elementType, count);
                _loadedObjects[objectID] = result;

                if (count == -1)
                {
                    count = storage.BeginReadObjectArray();
                }
                for (var l = 0; l < count; l++)
                {
                    storage.BeginReadObjectArrayItem(l);
                    result.SetValue(DeserializeObject(new Entry()
                    {
                        Name = "array_entry" + l,
                        StoredType = elementType
                    }, storage), l);
                    storage.EndReadObjectArrayItem();
                }
                storage.EndReadObjectArray();
            }


            return result;


        }

        /// <summary>
        ///   Deserializes a multi-dimensional array of values
        /// </summary>
        /// <param name = "itemType">The type of the array</param>
        /// <param name="storage"></param>
        /// <param name="count">The base size of the multi-dimensional array</param>
        /// <returns>The deserialized array</returns>
        /// <remarks>
        ///   This routine deserializes values serialized on a 'row by row' basis, and
        ///   calls into DeserializeArray to do this
        /// </remarks>
        private static object DeserializeMultiDimensionArray(Type itemType, IStorage storage, int objectID)
        {
            //Read the number of dimensions the array has
            //var dimensions = storage.ReadValue<int>("dimensions");
            //var totalLength = storage.ReadValue<int>("length");
            int dimensions, totalLength, rowLength;
            storage.BeginReadMultiDimensionalArray(out dimensions, out totalLength);

            rowLength = 0;

            // Establish the length of each array element
            // and get the total 'row size'
            int[] lengths = new int[dimensions];
            int[] indices = new int[dimensions];

            for (int item = 0; item < dimensions; item++)
            {
                lengths[item] = storage.ReadArrayDimension(item); //.ReadValue<int>("dim_len" + item);
                rowLength += lengths[item];
                indices[item] = 0;
            }

            int cols = lengths[lengths.Length - 1];
            //int cols = dimensions == 1 ? 1 : lengths[lengths.Length - 1];

            //Get the expected element type
            var elementType = itemType.GetElementType();



            Array sourceArrays = Array.CreateInstance(elementType, lengths);
            DeserializeArrayPart(sourceArrays, 0, indices, itemType, storage, objectID);
            return sourceArrays;
        }

        private static void DeserializeArrayPart(Array sourceArrays, int i, int[] indices, Type itemType, IStorage storage, int objectID)
        {
            int length = sourceArrays.GetLength(i);
            for (var l = 0; l < length; l++)
            {
                indices[i] = l;
                if (i != sourceArrays.Rank - 2)
                    DeserializeArrayPart(sourceArrays, i + 1, indices, itemType, storage, objectID);
                else
                {
                    Array sourceArray = (Array)DeserializeArray(itemType, storage, -1, objectID);
                    int cols = sourceArrays.GetLength(i + 1);
                    for (int arrayStartIndex = 0; arrayStartIndex < cols; arrayStartIndex++)
                    {
                        indices[i + 1] = arrayStartIndex;
                        sourceArrays.SetValue(sourceArray.GetValue(arrayStartIndex), indices);
                    }
                }
            }
        }

        /// <summary>
        ///   Deserializes a dictionary from storage, handles generic types with storage optimization
        /// </summary>
        /// <param name = "o">The newly created dictionary</param>
        /// <param name = "itemType">The type of the dictionary</param>
        /// <param name="storage"></param>
        /// <returns>The dictionary object updated with the values from storage</returns>
        private static object DeserializeDictionary(IDictionary o, Type itemType, IStorage storage)
        {
            Type keyType = null;
            Type valueType = null;
            if (itemType.IsGenericType)
            {
                var types = itemType.GetGenericArguments();
                keyType = types[0];
                valueType = types[1];
            }

            //var count = storage.ReadValue<int>("no_items");
            //int count = storage.G
            //int count = storage.BeginRead();
            //int count = storage.GetItemCount<int>();
            int count = storage.BeginReadDictionary();
            var list = new List<object>();
            for (var i = 0; i < count; i++)
            {
                storage.BeginReadDictionaryKeyItem(i);
                list.Add(DeserializeObject(new Entry()
                {
                    Name = "dic_key" + i,
                    StoredType = keyType
                }, storage));
                storage.EndReadDictionaryKeyItem();
            }
            for (var i = 0; i < count; i++)
            {
                storage.BeginReadDictionaryValueItem(i);
                o[list[i]] = DeserializeObject(new Entry()
                {
                    Name = "dic_value" + i,
                    StoredType = valueType
                }, storage);
                storage.EndReadDictionaryValueItem();
            }
            storage.EndReadDictionary();

            if (currentVersion >= 7)
                DeserializeObjectAndProperties(o, itemType, storage);

            return o;
        }

        /// <summary>
        ///   Deserialize a list from the data stream
        /// </summary>
        /// <param name = "o">The newly created list</param>
        /// <param name = "itemType">The type of the list</param>
        /// <param name="storage"></param>
        /// <returns>The list updated with values from the stream</returns>
        private static object DeserializeList(IList o, Type itemType, IStorage storage)
        {
            Type valueType = null;
            if (itemType.IsGenericType)
            {
                var types = itemType.GetGenericArguments();
                valueType = types[0];
            }

            //var count = storage.ReadValue<int>("no_items");
            //int count = storage.BeginRead();
            int count = storage.BeginReadList();
            var list = new List<object>();
            for (var i = 0; i < count; i++)
            {
                storage.BeginReadListItem(i);
                o.Add(DeserializeObject(new Entry()
                {
                    Name = "list_item" + i,
                    StoredType = valueType
                }, storage));
                storage.EndReadListItem();
            }
            storage.EndReadList();

            if (currentVersion >= 7)
                DeserializeObjectAndProperties(o, itemType, storage);

            return o;
        }

        /// <summary>
        ///   Deserializes a class based object that is not a collection, looks for both public properties and fields
        /// </summary>
        /// <param name = "o">The object being deserialized</param>
        /// <param name = "itemType">The type of the object</param>
        /// <param name="storage"></param>
        /// <returns>The object updated with values from the stream</returns>
        private static object DeserializeObjectAndProperties(object o, Type itemType, IStorage storage)
        {
            DeserializeProperties(storage, itemType, o);
            DeserializeFields(storage, itemType, o);
            return o;
        }






        /// <summary>
        ///   Deserializes the properties of an object from the stream
        /// </summary>
        /// <param name="storage"></param>
        /// <param name = "itemType">The type of the object</param>
        /// <param name = "o">The object to deserialize</param>
        private static void DeserializeProperties(IStorage storage, Type itemType, object o)
        {
            //Get the number of properties
            //var propCount = storage.ReadValue<byte>("property_count");
            int propCount = storage.BeginReadProperties();

            for (var i = 0; i < propCount; i++)
            {
                //Deserialize the value
                Entry entry = storage.BeginReadProperty(new Entry()
                {
                    OwningType = itemType,
                    MustHaveName = true
                });
                var value = DeserializeObject(entry, storage);

                if (entry.Setter != null && value != null)
                {
                    try
                    {
                        entry.Setter.Set(o, value);
                    }
                    catch (ArgumentException)
                    {
                        try
                        {
                            // if the property is nullable enum we need to handle it differently because a straight ChangeType doesn't work
                            Type type = Nullable.GetUnderlyingType(entry.Setter.Info.PropertyType);
                            if (type != null && type.IsEnum)
                            {
                                entry.Setter.Info.SetValue(o, Enum.Parse(type, value.ToString(), true), null);
                            }
                            else
                            {
                                entry.Setter.Info.SetValue(o, Convert.ChangeType(value, entry.Setter.Info.PropertyType, null), null);
                            }
                        }
                        catch
                        {

                        }
                    }
                    catch (Exception)
                    {
                        //Suppress cases where the old value is no longer compatible with the new property type

                    }

                }
                storage.EndReadProeprty();
            }
            storage.EndReadProperties();
        }


        /// <summary>
        ///   Deserializes the fields of an object from the stream
        /// </summary>
        /// <param name = "reader">The reader of the bytes in the stream</param>
        /// <param name = "itemType">The type of the object</param>
        /// <param name = "o">The object to deserialize</param>
        private static void DeserializeFields(IStorage storage, Type itemType, object o)
        {
            //var fieldCount = storage.ReadValue<byte>("field_count");
            int fieldCount = storage.BeginReadFields();
            //int length = 0;
            for (var i = 0; i < fieldCount; i++)
            {
                Entry entry = storage.BeginReadField(new Entry()
                {
                    OwningType = itemType,
                    MustHaveName = true
                });
                var value = DeserializeObject(entry, storage);
                if (entry.Setter != null && value != null)
                {
                    try
                    {
                        entry.Setter.Set(o, value);
                    }
                    catch (ArgumentException)
                    {
                        try
                        {
                            // if the property is nullable enum we need to handle it differently because a straight ChangeType doesn't work
                            Type type = Nullable.GetUnderlyingType(entry.Setter.FieldInfo.FieldType);
                            if (type != null && type.IsEnum)
                            {
                                entry.Setter.FieldInfo.SetValue(o, Enum.Parse(type, value.ToString(), true));
                            }
                            else
                            {
                                entry.Setter.FieldInfo.SetValue(o, Convert.ChangeType(value, entry.Setter.FieldInfo.FieldType, null));
                            }
                        }
                        catch
                        {

                        }
                    }
                    catch (Exception)
                    {
                        //Suppress cases where the old value is no longer compatible with the new property type
                    }
                }
                storage.EndReadField();
            }
            storage.EndReadFields();
        }


        #endregion


        public static void Serialize(object item, IStorage storage)
        {
            bool verbose = Verbose;
            Verbose = false;
            CreateStacks();

            try
            {
                _soStack.Push(_seenObjects);
                _seenObjects = new Dictionary<object, int>();
                storage.StartSerializing();
                SerializeObject(new Entry()
                {
                    Name = "root",
                    Value = item
                }, storage);
                storage.FinishedSerializing();
            }
            finally
            {
                _seenObjects = _soStack.Pop();
                Verbose = verbose;
            }

        }

        public static void Serialize(object item, Stream outputStream)
        {
            CreateStacks();


            try
            {
                _soStack.Push(_seenObjects);
                _seenObjects = new Dictionary<object, int>();

                //var serializer = Activator.CreateInstance(SerializerType) as IStorage;
                BinarySerializer serializer = new BinarySerializer();
                serializer.StartSerializing();
                SerializeObject(new Entry()
                {
                    Name = "root",
                    Value = item
                }, serializer);
                serializer.FinishedSerializing();

                var outputWr = new BinaryWriter(outputStream);
                outputWr.Write(serializer.Data);
            }
            finally
            {
                _seenObjects = _soStack.Pop();
            }

        }

        /// <summary>
        ///   Serialize an object into an array of bytes
        /// </summary>
        /// <param name = "item">The object to serialize</param>
        /// <returns>A byte array representation of the item</returns>
        public static byte[] Serialize(object item)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                Serialize(item, outputStream);
                //Reset the verbose mode
                return outputStream.ToArray();
            }
        }

        /// <summary>
        ///   Serialize an object into an array of bytes
        /// </summary>
        /// <param name = "item">The object to serialize</param>
        /// <param name="makeVerbose">Whether the object should be serialized for forwards compatibility</param>
        /// <returns>A byte array representation of the item</returns>
        public static byte[] Serialize(object item, bool makeVerbose)
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                var v = Verbose;
                Verbose = makeVerbose;
                Serialize(item, outputStream);
                Verbose = v;
                //Reset the verbose mode
                return outputStream.ToArray();
            }
        }

        #region Serialization

        public class Nuller
        {

        }

        private static void SerializeObject(Entry entry, IStorage storage)
        {
            if (entry.Value == null)
                entry.Value = new Nuller();

            var item = entry.Value;

            if (storage.StartSerializing(entry, _seenObjects.Count))
            {
                _seenObjects[item] = _seenObjects.Count;
                return;
            }

            var itemType = item.GetType();

            //Check for custom serialization
            if (Serializers.ContainsKey(itemType))
            {
                //If we have a custom serializer then use it!
                var serializer = Serializers[itemType];
                var data = serializer.Serialize(item);
                SerializeObject(new Entry()
                {
                    Name = "data",
                    Value = data,
                    StoredType = typeof(object[])
                }, storage);
                return;
            }


            //Check for simple types again
            if (IsSimpleType(itemType))
            {
                storage.WriteSimpleValue(itemType.IsEnum ? Convert.ChangeType(item, Enum.GetUnderlyingType(itemType), CultureInfo.InvariantCulture) : item);
                return;
            }

            //Check whether this object has been seen
            if (_seenObjects.ContainsKey(item))
            {
                storage.BeginWriteObject(_seenObjects[item], item.GetType(), true);
                storage.EndWriteObject();
                return;
            }

            //We are going to serialize an object
            _seenObjects[item] = _seenObjects.Count;
            storage.BeginWriteObject(_seenObjects[item], item.GetType(), false);

            //Check for collection types)
            if (item is Array)
            {
                if (((Array)item).Rank == 1)
                {
                    SerializeArray(item as Array, itemType, storage);
                }
                else
                {
                    SerializeMultiDimensionArray(item as Array, itemType, storage);
                }
                storage.EndWriteObject();
                return;
            }
            if (item is IDictionary)
            {
                SerializeDictionary(item as IDictionary, itemType, storage);
                storage.EndWriteObject();
                return;
            }
            if (item is IList)
            {
                SerializeList(item as IList, itemType, storage);
                storage.EndWriteObject();
                return;
            }
            //Otherwise we are serializing an object
            SerializeObjectAndProperties(item, itemType, storage);
            storage.EndWriteObject();
            storage.FinishSerializing(entry);
        }

        private static void SerializeList(IList item, Type tp, IStorage storage)
        {


            Type valueType = null;
            //Try to optimize the storage of types based on the type of list
            if (tp.IsGenericType)
            {
                var types = tp.GetGenericArguments();
                valueType = types[0];
            }

            //storage.WriteValue("no_items", item.Count);
            storage.BeginWriteList(item.Count, item.GetType());
            int i = 0;
            Entry entry = new Entry();


            foreach (var val in item.Cast<object>().ToArray())
            {
                storage.BeginWriteListItem(i);
                entry.Name = "list_item" + i++;
                entry.Value = val;
                entry.StoredType = valueType;
                SerializeObject(entry, storage);
                storage.EndWriteListItem();
            }
            storage.EndWriteList();

            SerializeObjectAndProperties(item, tp, storage);
        }

        private static void SerializeDictionary(IDictionary item, Type tp, IStorage storage)
        {

            Type keyType = null;
            Type valueType = null;
            //Try to optimise storage based on the type of dictionary
            if (tp.IsGenericType)
            {
                var types = tp.GetGenericArguments();
                keyType = types[0];
                valueType = types[1];
            }

            //storage.WriteValue("no_items", item.Count);
            storage.BeginWriteDictionary(item.Count, item.GetType());

            //Serialize the pairs
            var i = 0;
            foreach (var key in item.Keys)
            {
                storage.BeginWriteDictionaryKey(i);
                SerializeObject(new Entry()
                {
                    Name = "dic_key" + i++,
                    StoredType = keyType,
                    Value = key
                }, storage);
                storage.EndWriteDictionaryKey();
            }
            i = 0;
            foreach (var val in item.Values)
            {
                storage.BeginWriteDictionaryValue(i);
                SerializeObject(new Entry()
                {
                    Name = "dic_value" + i++,
                    StoredType = valueType,
                    Value = val
                }, storage);
                storage.EndWriteDictionaryValue();
            }

            storage.EndWriteDictionary();

            SerializeObjectAndProperties(item, tp, storage);
        }

        private static void SerializeArray(Array item, Type tp, IStorage storage)
        {

            Type elementType = tp.GetElementType();

            //if (propertyType == typeof(byte))
            //{
            //    storage.WriteSimpleArray(item.Length, item);
            //}
            //else 
            if (IsSimpleType(elementType))
            {
                storage.WriteSimpleArray(item.Length, item);
            }
            else
            {
                int length = item.Length;
                storage.BeginWriteObjectArray(length, item.GetType());
                for (int l = 0; l < length; l++)
                {
                    storage.BeginWriteObjectArrayItem(l);
                    SerializeObject(new Entry()
                    {
                        Name = "array_entry" + l,
                        Value = item.GetValue(l),
                        StoredType = elementType
                    }, storage);
                    storage.EndWriteObjectArrayItem();
                }
                storage.EndWriteObjectArray();
            }


        }

        private static void SerializeMultiDimensionArray(Array item, Type tp, IStorage storage)
        {

            // Multi-dimension serializer data is:
            // Int32: Ranks
            // Int32 (x number of ranks): length of array dimension 

            int dimensions = item.Rank;

            var length = item.GetLength(0);

            // Determine the number of cols being populated
            var cols = item.GetLength(item.Rank - 1);

            // Explicitly write this value, to denote that this is a multi-dimensional array
            // so it doesn't break the deserializer when reading values for existing arrays

            //storage.WriteValue("no_items", (int)-1);
            //storage.WriteValue("dimensions", dimensions);
            //storage.WriteValue("length", item.Length);
            storage.BeginMultiDimensionArray(item.GetType(), dimensions, length);


            Type propertyType = tp.GetElementType();
            int[] indicies = new int[dimensions];

            // Write out the length of each array, if we are dealing with the first array
            for (int arrayStartIndex = 0; arrayStartIndex < dimensions; arrayStartIndex++)
            {
                indicies[arrayStartIndex] = 0;
                //storage.WriteValue("dim_len" + arrayStartIndex, item.GetLength(arrayStartIndex));
                storage.WriteArrayDimension(arrayStartIndex, item.GetLength(arrayStartIndex));
            }

            SerializeArrayPart(item, 0, indicies, storage);

            storage.EndMultiDimensionArray();

        }

        private static void SerializeArrayPart(Array item, int i, int[] indices, IStorage storage)
        {
            var length = item.GetLength(i);
            for (var l = 0; l < length; l++)
            {
                indices[i] = l;
                if (i != item.Rank - 2)
                    SerializeArrayPart(item, i + 1, indices, storage);
                else
                {
                    Type arrayType = item.GetType().GetElementType();
                    var cols = item.GetLength(i + 1);

                    var baseArray = Array.CreateInstance(arrayType, cols);

                    // Convert the whole multi-dimensional array to be 'row' based
                    // and serialize using the existing code
                    for (int arrayStartIndex = 0; arrayStartIndex < cols; arrayStartIndex++)
                    {
                        indices[i + 1] = arrayStartIndex;
                        baseArray.SetValue(item.GetValue(indices), arrayStartIndex);
                    }

                    SerializeArray(baseArray, baseArray.GetType(), storage);
                }
            }


        }


        private static void WriteProperties(Type itemType, object item, IStorage storage)
        {
            Entry[] propList = storage.ShouldWriteProperties(GetWritableAttributes.GetProperties(item));
            storage.BeginWriteProperties(propList.Length);
            foreach (Entry entry in propList)
            {
                storage.BeginWriteProperty(entry.PropertyInfo.Name, entry.PropertyInfo.PropertyType);
                SerializeObject(entry, storage);
                storage.EndWriteProperty();
            }
            storage.EndWriteProperties();
        }


        private static void WriteFields(Type itemType, object item, IStorage storage)
        {
            Entry[] fieldList = storage.ShouldWriteFields(GetWritableAttributes.GetFields(item));
            storage.BeginWriteFields(fieldList.Length);
            foreach (Entry entry in fieldList)
            {
                storage.BeginWriteField(entry.FieldInfo.Name, entry.FieldInfo.FieldType);
                SerializeObject(entry, storage);
                storage.EndWriteField();
            }
            storage.EndWriteFields();
        }

        #endregion

        /// <summary>
        ///   Return whether the type specified is a simple type that can be serialized fast
        /// </summary>
        /// <param name = "tp">The type to check</param>
        /// <returns>True if the type is a simple one and can be serialized directly</returns>
        private static bool IsSimpleType(Type tp)
        {
            return tp.IsPrimitive || tp == typeof(DateTime) || tp == typeof(TimeSpan) || tp == typeof(string) || tp.IsEnum || tp == typeof(Guid) || tp == typeof(decimal);
        }

        private static void SerializeObjectAndProperties(object item, Type itemType, IStorage storage)
        {
            WriteProperties(itemType, item, storage);
            WriteFields(itemType, item, storage);
        }
        /// <summary>
        /// Create an instance of a type
        /// </summary>
        /// <param name="itemType">The type to construct</param>
        /// <returns></returns>
        internal static object CreateObject(Type itemType)
        {
            try
            {
                return itemType.IsDefined(typeof(CreateUsingEvent), false) ? CreateInstance(itemType) : Activator.CreateInstance(itemType);
            }
            catch (Exception)
            {
                try
                {
                    var constructorInfo = itemType.GetConstructor(new Type[] { });
                    return constructorInfo != null ? constructorInfo.Invoke(new object[] { }) : CreateInstance(itemType);
                }
                catch
                {
                    return CreateInstance(itemType);
                }

            }

        }

        private static object CreateInstance(Type itemType)
        {
            //Raise an event to construct the object
            var ct = new ObjectMappingEventArgs();
            ct.TypeToConstruct = itemType;
            InvokeCreateType(ct);
            //Check if we created the right thing
            if (ct.Instance != null && (ct.Instance.GetType() == itemType || ct.Instance.GetType().IsSubclassOf(itemType)))
            {
                return ct.Instance;
            }

            var error = string.Format("Could not construct an object of type '{0}', it must be creatable in this scope and have a default parameterless constructor or you should handle the CreateType event on SilverlightSerializer to construct the object", itemType.FullName);
            throw new MissingConstructorException(error);
        }


        public class MissingConstructorException : Exception
        {
            public MissingConstructorException(string message)
                : base(message)
            {

            }
        }

        #region Basic IO

        private delegate void WriteAValue(BinaryWriter writer, object value);

        public delegate object ReadAValue(BinaryReader reader);
        private static readonly Dictionary<Type, WriteAValue> Writers = new Dictionary<Type, WriteAValue>();
        public static readonly Dictionary<Type, ReadAValue> Readers = new Dictionary<Type, ReadAValue>();

        static SilverlightSerializer()
        {
            Writers[typeof(string)] = StringWriter;
            Writers[typeof(Decimal)] = DecimalWriter;
            Writers[typeof(float)] = FloatWriter;
            Writers[typeof(byte[])] = ByteArrayWriter;
            Writers[typeof(bool)] = BoolWriter;
            Writers[typeof(Guid)] = GuidWriter;
            Writers[typeof(DateTime)] = DateTimeWriter;
            Writers[typeof(TimeSpan)] = TimeSpanWriter;
            Writers[typeof(char)] = CharWriter;
            Writers[typeof(ushort)] = UShortWriter;
            Writers[typeof(double)] = DoubleWriter;
            Writers[typeof(ulong)] = ULongWriter;
            Writers[typeof(int)] = IntWriter;
            Writers[typeof(uint)] = UIntWriter;
            Writers[typeof(byte)] = ByteWriter;
            Writers[typeof(long)] = LongWriter;
            Writers[typeof(short)] = ShortWriter;
            Writers[typeof(sbyte)] = SByteWriter;

            Readers[typeof(string)] = AStringReader;
            Readers[typeof(Decimal)] = DecimalReader;
            Readers[typeof(float)] = FloatReader;
            Readers[typeof(byte[])] = ByteArrayReader;
            Readers[typeof(bool)] = BoolReader;
            Readers[typeof(Guid)] = GuidReader;
            Readers[typeof(DateTime)] = DateTimeReader;
            Readers[typeof(TimeSpan)] = TimeSpanReader;
            Readers[typeof(char)] = CharReader;
            Readers[typeof(ushort)] = UShortReader;
            Readers[typeof(double)] = DoubleReader;
            Readers[typeof(ulong)] = ULongReader;
            Readers[typeof(int)] = IntReader;
            Readers[typeof(uint)] = UIntReader;
            Readers[typeof(byte)] = ByteReader;
            Readers[typeof(long)] = LongReader;
            Readers[typeof(short)] = ShortReader;
            Readers[typeof(sbyte)] = SByteReader;


        }

        private static object ShortReader(BinaryReader reader)
        {
            return reader.ReadInt16();
        }

        private static object LongReader(BinaryReader reader)
        {
            return reader.ReadInt64();
        }

        private static object GuidReader(BinaryReader reader)
        {
            return new Guid(reader.ReadString());
        }

        private static object SByteReader(BinaryReader reader)
        {
            return reader.ReadSByte();
        }

        private static object ByteReader(BinaryReader reader)
        {
            return reader.ReadByte();
        }

        private static object UIntReader(BinaryReader reader)
        {
            return reader.ReadUInt32();
        }

        private static object IntReader(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        private static object ULongReader(BinaryReader reader)
        {
            return reader.ReadUInt64();
        }

        private static object DoubleReader(BinaryReader reader)
        {
            return reader.ReadDouble();
        }

        private static object UShortReader(BinaryReader reader)
        {
            return reader.ReadUInt16();
        }

        private static object CharReader(BinaryReader reader)
        {
            return reader.ReadChar();
        }

        private static object FloatReader(BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        private static object TimeSpanReader(BinaryReader reader)
        {
            return new TimeSpan(reader.ReadInt64());
        }

        private static object DateTimeReader(BinaryReader reader)
        {
            return new DateTime(reader.ReadInt64());
        }

        private static object ByteArrayReader(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            return reader.ReadBytes(len);
        }

        private static object DecimalReader(BinaryReader reader)
        {
            var array = new int[4];
            array[0] = (int)reader.ReadInt32();
            array[1] = (int)reader.ReadInt32();
            array[2] = (int)reader.ReadInt32();
            array[3] = (int)reader.ReadInt32();

            return new Decimal(array);
        }

        private static object BoolReader(BinaryReader reader)
        {
            return reader.ReadChar() == 'Y';
        }

        private static object AStringReader(BinaryReader reader)
        {
            var retString = reader.ReadString();

            return retString == "~~NULL~~"
                       ? null
                       : retString;
        }

        private static void SByteWriter(BinaryWriter writer, object value)
        {
            writer.Write((sbyte)value);
        }

        private static void ShortWriter(BinaryWriter writer, object value)
        {
            writer.Write((short)value);
        }

        private static void LongWriter(BinaryWriter writer, object value)
        {
            writer.Write((long)value);
        }

        private static void ByteWriter(BinaryWriter writer, object value)
        {
            writer.Write((byte)value);
        }

        private static void UIntWriter(BinaryWriter writer, object value)
        {
            writer.Write((uint)value);
        }

        private static void IntWriter(BinaryWriter writer, object value)
        {
            writer.Write((int)value);
        }

        private static void ULongWriter(BinaryWriter writer, object value)
        {
            writer.Write((ulong)value);
        }

        private static void DoubleWriter(BinaryWriter writer, object value)
        {
            writer.Write((double)value);
        }

        private static void UShortWriter(BinaryWriter writer, object value)
        {
            writer.Write((ushort)value);
        }

        private static void CharWriter(BinaryWriter writer, object value)
        {
            writer.Write((char)value);
        }

        private static void TimeSpanWriter(BinaryWriter writer, object value)
        {
            writer.Write(((TimeSpan)value).Ticks);
        }

        private static void DateTimeWriter(BinaryWriter writer, object value)
        {
            writer.Write(((DateTime)value).Ticks);
        }

        private static void GuidWriter(BinaryWriter writer, object value)
        {
            writer.Write(value.ToString());
        }

        private static void BoolWriter(BinaryWriter writer, object value)
        {
            writer.Write((bool)value
                               ? 'Y'
                               : 'N');
        }

        private static void ByteArrayWriter(BinaryWriter writer, object value)
        {
            var array = value as byte[];
            writer.Write((int)array.Length);
            writer.Write(array);
        }

        private static void FloatWriter(BinaryWriter writer, object value)
        {
            writer.Write((float)value);
        }

        private static void DecimalWriter(BinaryWriter writer, object value)
        {
            int[] array = Decimal.GetBits((Decimal)value);
            writer.Write(array[0]);
            writer.Write(array[1]);
            writer.Write(array[2]);
            writer.Write(array[3]);
        }

        private static void StringWriter(BinaryWriter writer, object value)
        {
            writer.Write((string)value);
        }

        ///// <summary>
        /////   Write a basic untyped value
        ///// </summary>
        ///// <param name = "writer">The writer to commit byte to</param>
        ///// <param name = "value">The value to write</param>
        //internal static void WriteValue(BinaryWriter writer, object value)
        //{
        //    if (value is string)
        //        writer.Write((string)value);
        //    else if (value == null)
        //        writer.Write("~~NULL~~");
        //    else if (value is bool)
        //        writer.Write((bool)value
        //                         ? 'Y'
        //                         : 'N');
        //    else if (value is double)
        //        writer.Write((double)value);
        //    else if (value is int)
        //        writer.Write((int)value);
        //    else if (value is Guid)
        //        writer.Write(value.ToString());
        //    else if (value is DateTime)
        //        writer.Write(((DateTime)value).Ticks);
        //    else if (value is TimeSpan)
        //        writer.Write(((TimeSpan)value).Ticks);
        //    else if (value is char)
        //        writer.Write((char)value);
        //    else if (value is ushort)
        //        writer.Write((ushort)value);
        //    else if (value is decimal)
        //    {
        //        int[] array = Decimal.GetBits((Decimal)value);
        //        writer.Write(array[0]);
        //        writer.Write(array[1]);
        //        writer.Write(array[2]);
        //        writer.Write(array[3]);

        //    }
        //    else if (value is float)
        //        writer.Write((float)value);
        //    else if (value is byte[])
        //    {
        //        var array = value as byte[];
        //        writer.Write((int)array.Length);
        //        writer.Write(array);
        //    }

        //    else if (value is ulong)
        //        writer.Write((ulong)value);

        //    else if (value is uint)
        //        writer.Write((uint)value);
        //    else if (value is byte)
        //        writer.Write((byte)value);
        //    else if (value is long)
        //        writer.Write((long)value);
        //    else if (value is short)
        //        writer.Write((short)value);
        //    else if (value is sbyte)
        //        writer.Write((sbyte)value);
        //    else
        //        writer.Write((int)value);
        //}

        ///// <summary>
        /////   Read a basic value from the stream
        ///// </summary>
        ///// <param name = "reader">The reader with the stream</param>
        ///// <param name = "tp">The type to read</param>
        ///// <returns>The hydrated value</returns>
        //internal static object ReadValue(BinaryReader reader, Type tp)
        //{

        //    if (tp == typeof(bool))
        //        return reader.ReadChar() == 'Y';
        //    if (tp == typeof(int))
        //        return reader.ReadInt32();
        //    if (tp == typeof(double))
        //        return reader.ReadDouble();
        //    if (tp == typeof(string))
        //    {
        //        var retString = reader.ReadString();

        //        return retString == "~~NULL~~"
        //                   ? null
        //                   : retString;
        //    }
        //    if (tp == typeof(decimal))
        //    {
        //        var array = new int[4];
        //        array[0] = (int)reader.ReadInt32();
        //        array[1] = (int)reader.ReadInt32();
        //        array[2] = (int)reader.ReadInt32();
        //        array[3] = (int)reader.ReadInt32();

        //        return new Decimal(array);
        //    }
        //    if (tp == typeof(byte[]))
        //    {
        //        var len = reader.ReadInt32();
        //        return reader.ReadBytes(len);
        //    }
        //    if (tp == typeof(DateTime))
        //        return new DateTime(reader.ReadInt64());
        //    if (tp == typeof(TimeSpan))
        //        return new TimeSpan(reader.ReadInt64());
        //    if (tp == typeof(float))
        //        return reader.ReadSingle();
        //    if (tp == typeof(char))
        //        return reader.ReadChar();
        //    if (tp == typeof(ushort))
        //        return reader.ReadUInt16();
        //    if (tp == typeof(ulong))
        //        return reader.ReadUInt64();

        //    if (tp == typeof(uint))
        //        return reader.ReadUInt32();
        //    if (tp == typeof(byte))
        //        return reader.ReadByte();
        //    if (tp == typeof(long))
        //        return reader.ReadInt64();
        //    if (tp == typeof(short))
        //        return reader.ReadInt16();
        //    if (tp == typeof(sbyte))
        //        return reader.ReadSByte();
        //    if (tp == typeof(Guid))
        //        return new Guid(reader.ReadString());
        //    return reader.ReadInt32();
        //}

        /// <summary>
        ///   Write a basic untyped value
        /// </summary>
        /// <param name = "writer">The writer to commit byte to</param>
        /// <param name = "value">The value to write</param>
        internal static void WriteValue(BinaryWriter writer, object value)
        {
            WriteAValue write;
            if (!Writers.TryGetValue(value.GetType(), out write))
            {
                writer.Write((int)value);
                return;
            }
            write(writer, value);
        }

        /// <summary>
        ///   Read a basic value from the stream
        /// </summary>
        /// <param name = "reader">The reader with the stream</param>
        /// <param name = "tp">The type to read</param>
        /// <returns>The hydrated value</returns>
        internal static object ReadValue(BinaryReader reader, Type tp)
        {
            ReadAValue read;
            if (!Readers.TryGetValue(tp, out read))
            {
                return reader.ReadInt32();
            }
            return read(reader);

        }

        #endregion

        /// <summary>
        ///   Logs a type and returns a unique token for it
        /// </summary>
        /// <param name = "tp">The type to retrieve a token for</param>
        /// <returns>A 2 byte token representing the type</returns>
        internal static ushort GetTypeId(RuntimeTypeHandle tp)
        {
            var tpId = KnownTypes.IndexOf(tp);

            if (tpId < 0)
            {
                tpId = KnownTypes.Count;
                KnownTypes.Add(tp);
            }
            return (ushort)tpId;
        }

        /// <summary>
        /// Stores configurations for entries
        /// </summary>
        private class EntryConfiguration
        {
            public GetSet Setter;
            public Type Type;
        }

        /// <summary>
        /// Cache for property name to item lookups
        /// </summary>
        private static readonly Dictionary<Type, Dictionary<string, EntryConfiguration>> StoredTypes = new Dictionary<Type, Dictionary<string, EntryConfiguration>>();

        /// <summary>
        /// Gets a property setter and a standard default type for an entry
        /// </summary>
        /// <param name="entry"></param>
        private static void UpdateEntryWithName(Entry entry)
        {
            Dictionary<string, EntryConfiguration> configurations;
            if (!StoredTypes.TryGetValue(entry.OwningType, out configurations))
            {
                configurations = new Dictionary<string, EntryConfiguration>();
                StoredTypes[entry.OwningType] = configurations;
            }

            EntryConfiguration entryConfiguration;
            if (!configurations.TryGetValue(entry.Name, out entryConfiguration))
            {
                entryConfiguration = new EntryConfiguration();

                var pi = entry.OwningType.GetProperty(entry.Name);
                if (pi != null)
                {
                    entryConfiguration.Type = pi.PropertyType;
                    var gs = typeof(GetSetGeneric<,>);
                    var tp = gs.MakeGenericType(new Type[] { entry.OwningType, pi.PropertyType });
                    entryConfiguration.Setter = (GetSet)Activator.CreateInstance(tp, new object[] { pi });
                }
                else
                {
                    var fi = entry.OwningType.GetField(entry.Name);
                    if (fi != null)
                    {
                        entryConfiguration.Type = fi.FieldType;
                        var gs = typeof(GetSetGeneric<,>);
                        var tp = gs.MakeGenericType(new Type[] { entry.OwningType, fi.FieldType });
                        entryConfiguration.Setter = (GetSet)Activator.CreateInstance(tp, new object[] { fi });
                    }
                }
                configurations[entry.Name] = entryConfiguration;
            }
            entry.StoredType = entryConfiguration.Type;
            entry.Setter = entryConfiguration.Setter;
        }
    }
}
