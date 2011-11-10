using System;

namespace Samba.Infrastructure.Data.BinarySerializer
{
    public interface IStorage
    {
        /// <summary>
        /// Starts the serialization process, the serializer should initialize and wait for data
        /// </summary>
        void StartSerializing();
        /// <summary>
        /// Called when serialization is complete, should return the data or a key
        /// encoded as a byte array that will be used to reinitialize the serializer
        /// later
        /// </summary>
        /// <returns></returns>
        void FinishedSerializing();
        /// <summary>
        /// Called when deserialization is complete, so that resources may be released
        /// </summary>
        void FinishedDeserializing();
        /// <summary>
        /// Called when serializing a new object, the Entry parameter may have MustHaveName set
        /// when this is true the name must be persisted as is so that the property or field can
        /// be set when retrieving the data.
        /// If this routine returns TRUE then no further processing is executed and the object
        /// is presumed persisted in its entirety
        /// </summary>
        /// <returns>Normally FALSE.  True if the object is already fully persisted</returns>
        /// <param name="entry">The item being serialized</param>
        bool StartSerializing(Entry entry, int id);
        /// <summary>
        /// Called when the last information about an object has been written
        /// </summary>
        /// <param name="entry">The object being written</param>
        void FinishSerializing(Entry entry);

        /// <summary>
        /// Called when deserializing an object.  If the Entry parameter has MustHaveName set then
        /// the routine should return with the Entry parameter updated with the name and
        /// the type of the object in StoredType
        /// If  the storage is capable of fully recreating the object then this routine should return
        /// the fully constructed object, and no further processing will occur.  Not this does mean
        /// that it must handle its own references for previously seen objects
        /// This will be called after DeserializeGetName
        /// </summary>
        /// <returns>Normally NULL, it may also return a fully depersisted object</returns>
        /// <param name="entry"></param>
        object StartDeserializing(Entry entry);

        /// <summary>
        /// Called to allow the storage to retrieve the name of the item being deserialized
        /// All entries must be named before a call to StartDeserializing, this enables
        /// the system to fill out the property setter and capture default stored type
        /// information before deserialization commences
        /// </summary>
        /// <param name="entry">The entry whose name should be filled in</param>
        void DeserializeGetName(Entry entry);
        /// <summary>
        /// Called when an object has deserialization complete
        /// </summary>
        /// <param name="entry"></param>
        void FinishDeserializing(Entry entry);
        /// <summary>
        /// Reads a simple type (or array of bytes) from storage
        /// </summary>
        /// <param name="name">The name of the item</param>
        /// <param name="type">The type to be read</param>
        /// <returns></returns>

        Entry[] ShouldWriteFields(Entry[] fields);
        Entry[] ShouldWriteProperties(Entry[] properties);


        void StartDeserializing();


        #region reading


        Entry BeginReadProperty(Entry entry);
        void EndReadProeprty();
        Entry BeginReadField(Entry entry);
        void EndReadField();

        int BeginReadProperties();
        int BeginReadFields();
        void EndReadProperties();
        void EndReadFields();


        T ReadSimpleValue<T>();
        object ReadSimpleValue(Type type);

        bool IsMultiDimensionalArray(out int length);
        void BeginReadMultiDimensionalArray(out int dimension, out int count);
        void EndReadMultiDimensionalArray();
        int ReadArrayDimension(int index);


        Array ReadSimpleArray(Type elementType, int count);
        //int BeginRead();


        int BeginReadObject(out bool isReference);
        void EndReadObject();

        int BeginReadList();
        void BeginReadListItem(int index);
        void EndReadListItem();
        void EndReadList();

        int BeginReadDictionary();
        void BeginReadDictionaryKeyItem(int index);
        void EndReadDictionaryKeyItem();
        void BeginReadDictionaryValueItem(int index);
        void EndReadDictionaryValueItem();
        void EndReadDictionary();

        int BeginReadObjectArray();
        void BeginReadObjectArrayItem(int index);
        void EndReadObjectArrayItem();
        void EndReadObjectArray();


        #endregion

        #region writing

        void BeginWriteObject(int id, Type objectType, bool wasSeen);
        void EndWriteObject();



        void BeginWriteList(int count, Type listType);
        void BeginWriteListItem(int index);
        void EndWriteListItem();
        void EndWriteList();

        void BeginWriteObjectArray(int count, Type arrayType);
        void BeginWriteObjectArrayItem(int index);
        void EndWriteObjectArrayItem();
        void EndWriteObjectArray();

        void BeginMultiDimensionArray(Type arrayType, int dimensions, int count);
        void EndMultiDimensionArray();

        void WriteArrayDimension(int index, int count);
        void WriteSimpleArray(int count, Array array);
        void WriteSimpleValue(object value);

        // dictionaries
        void BeginWriteDictionary(int count, Type dictionaryType);
        void BeginWriteDictionaryKey(int id);
        void EndWriteDictionaryKey();
        void BeginWriteDictionaryValue(int id);
        void EndWriteDictionaryValue();
        void EndWriteDictionary();


        // properties and fields
        void BeginWriteProperties(int count);
        void EndWriteProperties();
        void BeginWriteProperty(string name, Type type);
        void EndWriteProperty();
        void BeginWriteFields(int count);
        void EndWriteFields();
        void BeginWriteField(string name, Type type);
        void EndWriteField();

        bool SupportsOnDemand { get; }
        void BeginOnDemand(int id);
        void EndOnDemand();


        #endregion


    }
}