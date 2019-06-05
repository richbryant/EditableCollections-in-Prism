using System.Reflection;

namespace EditableCollections.Editable
{
    public interface IEditable
    {
        object WrappedInstance { get; }
        object ReadProperty(PropertyInfo property);
        void WriteProperty(PropertyInfo property, object value);
    }
}