using System;

namespace EditableCollections.Editable.MetaData
{
    internal interface IDelegatePropertyWriter
    {
        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value.</param>
        void SetValue(object instance, object value);
    }

    /// <summary>
    /// A class which wraps writing to properties via the set method.
    /// </summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <typeparam name="TInstance"></typeparam>
    internal sealed class DelegatePropertyWriter<TInstance, TProperty> :
        IDelegatePropertyWriter
    {
        private readonly Action<TInstance, TProperty> _setValueDelegate;

        /// <summary>
        /// Initializes a new instance of the DelegatePropertyWriter class.
        /// </summary>
        /// <param name="setValueDelegate">The set value delegate.</param>
        public DelegatePropertyWriter(Action<TInstance, TProperty> setValueDelegate)
        {
            _setValueDelegate = setValueDelegate;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The value.</param>
        public void SetValue(object instance, object value)
        {
            _setValueDelegate((TInstance)instance, (TProperty)value);
        }
    }
}
