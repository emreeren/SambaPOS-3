namespace Samba.Infrastructure.Data.Serializer
{
  public interface IXmlSerializationTag
  {
    string ASSEMBLY_TAG { get; }
    string INDEX_TAG { get; }
    string ITEM_TAG { get; }
    string ITEMS_TAG { get; }
    string NAME_ATT_KEY_TAG { get; }
    string NAME_ATT_VALUE_TAG { get; }
    string NAME_TAG { get; }
    string OBJECT_TAG { get; }
    string PROPERTIES_TAG { get; }
    string PROPERTY_TAG { get; }
    string TYPE_DICTIONARY_TAG { get; }
    string TYPE_TAG { get; }
    string GENERIC_TYPE_ARGUMENTS_TAG { get; }
    string CONSTRUCTOR_TAG { get;}
    string BINARY_DATA_TAG { get;}
  }
}