using System;
using System.Reflection;

namespace EditableCollections.Editable.MetaData
{
    internal static class DelegatePropertyFactory
    {
        /// <summary>
        /// Creates the property reader.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public static IDelegatePropertyReader CreatePropertyReader(PropertyInfo property)
        {
            if (property == null) return null;
            var delegateReaderType = typeof(Func<,>).MakeGenericType(property.DeclaringType, property.PropertyType);
            var readerType = typeof(DelegatePropertyReader<,>).MakeGenericType(property.DeclaringType, property.PropertyType);
            if (!property.CanRead) return null;
            var propertyGetterMethodInfo = property.GetGetMethod();
            var propertyGetterDelegate = Delegate.CreateDelegate(
                delegateReaderType,
                propertyGetterMethodInfo);
            var reader = (IDelegatePropertyReader)Activator.CreateInstance(readerType, propertyGetterDelegate);
            return reader;
        }

        /// <summary>
        /// Creates the property writer.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        public static IDelegatePropertyWriter CreatePropertyWriter(PropertyInfo property)
        {
            if (property == null) return null;
            var delegateWriterType = typeof(Action<,>).MakeGenericType(property.DeclaringType, property.PropertyType);
            var writerType = typeof(DelegatePropertyWriter<,>).MakeGenericType(property.DeclaringType, property.PropertyType);
            if (!property.CanWrite) return null;
            var propertySetterMethodInfo = property.GetSetMethod();
            var propertySetterDelegate = Delegate.CreateDelegate(
                delegateWriterType,
                propertySetterMethodInfo);
            var writer = (IDelegatePropertyWriter)Activator.CreateInstance(writerType, propertySetterDelegate);
            return writer;
        }
    }
}
