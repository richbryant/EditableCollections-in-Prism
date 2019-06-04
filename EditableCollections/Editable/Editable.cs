using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using EditableCollections.Editable.MetaData;

namespace EditableCollections.Editable
{
    [TypeDescriptionProvider(typeof(EditableTypeDescriptionProvider))]
    public class Editable<TWrappedObject> :
        INotifyPropertyChanged,
        IEditable
        where TWrappedObject : class, new()
    {
        private readonly Dictionary<PropertyInfo, object> _changedProperties;
        private readonly TypeMetaData _metaData;
        private readonly TWrappedObject _current;
        private bool _isDeleted;
        private bool _isNew;

        /// <summary>
        /// Initializes a new instance of the <see cref="Editable&lt;TWrappedObject&gt;"/> class.
        /// </summary>
        public Editable() : this(new TWrappedObject())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Editable&lt;TWrappedObject&gt;"/> class.
        /// </summary>
        /// <param name="current">The current.</param>
        public Editable(TWrappedObject current)
        {
            _changedProperties = new Dictionary<PropertyInfo, object>();
            _current = current;
            _metaData = TypeMetaDataRepository.GetFor(GetType(), typeof(TWrappedObject));
        }

        /// <summary>
        /// Gets the current item.
        /// </summary>
        public TWrappedObject Original => _current;

        /// <summary>
        /// Gets the current item.
        /// </summary>
        object IEditable.WrappedInstance => _current;

        public bool IsDeleted
        {
            get => _isDeleted;
            set 
            { 
                _isDeleted = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsDeleted"));
                OnPropertyChanged(new PropertyChangedEventArgs("HasChanges"));
            }
        }

        public bool IsNew
        {
            get => _isNew;
            set
            {
                _isNew = value;
                OnPropertyChanged(new PropertyChangedEventArgs("IsNew"));
                OnPropertyChanged(new PropertyChangedEventArgs("HasChanges"));
            }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets a value indicating whether this instance has changes.
        /// </summary>
        public bool HasChanges => _changedProperties.Count > 0 || IsDeleted || IsNew;

        /// <summary>
        /// Begins an edit on an object.
        /// </summary>
        public void BeginChanges()
        {
        }

        /// <summary>
        /// Undoes all changes made to this adapter.
        /// </summary>
        public void UndoChanges()
        {
            _changedProperties.Clear();
            _metaData?.AllKnownProperties.ForEach(p => OnPropertyChanged(new PropertyChangedEventArgs(p.Name)));
            IsDeleted = false;
            OnPropertyChanged(new PropertyChangedEventArgs("HasChanges"));
        }

        /// <summary>
        /// Pushes changes since the last <see cref="M:System.ComponentModel.IEditableObject.BeginEdit"/> or <see cref="M:System.ComponentModel.IBindingList.AddNew"/> call into the underlying object.
        /// </summary>
        public void CommitChanges()
        {
            if (_metaData == null) return;
            
            foreach (var property in _changedProperties)
            {
                if (property.Key.CanWrite)
                {
                    _metaData.PropertyWriters[property.Key].SetValue(_current, property.Value);
                }
            }
            _changedProperties.Clear();
            _metaData.AllKnownProperties.ForEach(p => OnPropertyChanged(new PropertyChangedEventArgs(p.Name)));
            OnPropertyChanged(new PropertyChangedEventArgs("HasChanges"));
        }

        /// <summary>
        /// Reads the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns></returns>
        object IEditable.ReadProperty(PropertyInfo property)
        {
            if (!property.CanRead) return null;
            return _changedProperties.ContainsKey(property) ? _changedProperties[property] : _metaData.PropertyReaders[property].GetValue(_current);
        }

        /// <summary>
        /// Writes the property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        void IEditable.WriteProperty(PropertyInfo property, object value)
        {
            if (!property.CanWrite) return;
            if (property.GetValue(_current, null) == null || !property.GetValue(_current, null).Equals(value))
            {
                if (!_changedProperties.ContainsKey(property))
                {
                    _changedProperties.Add(property, value);
                }
                else
                {
                    _changedProperties[property] = value;
                }
            }
            else
            {
                if (_changedProperties.ContainsKey(property))
                {
                    _changedProperties.Remove(property);
                }
            }

            OnPropertyChanged(new PropertyChangedEventArgs(property.Name));
            OnPropertyChanged(new PropertyChangedEventArgs("HasChanges"));
        }

        /// <summary>
        /// Gets the edited value of a property, even if the value has not yet been committed to the underlying item.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        protected object ReadUncommitted(string propertyName)
        {
            var property = Original.GetType().GetProperty(propertyName);
            if (property != null)
            {
                return ((IEditable)this).ReadProperty(property);
            }
            throw new Exception(
                $"{GetType().Name}.ReadUncommitted was called for property '{propertyName}' which could not be found on the object '{Original}'");
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, e);
        }
    }
}