﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EditableCollections.Editable
{
    public sealed class EditableTypeDescriptionProvider : TypeDescriptionProvider
    {
        private static readonly Dictionary<Type, EditableTypeDescriptor> AlreadyCreated = new Dictionary<Type, EditableTypeDescriptor>();
        private readonly object _alreadyCreatedLock = new object();

        /// <summary>
        /// Gets a custom type descriptor for the given type and object.
        /// </summary>
        /// <param name="objectType">The type of object for which to retrieve the type descriptor.</param>
        /// <param name="instance">An instance of the type. Can be null if no instance was passed to the <see cref="T:System.ComponentModel.TypeDescriptor"/>.</param>
        /// <returns>
        /// An <see cref="T:System.ComponentModel.ICustomTypeDescriptor"/> that can provide metadata for the type.
        /// </returns>
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            lock (_alreadyCreatedLock)
            {
                if (!AlreadyCreated.ContainsKey(objectType))
                {
                    AlreadyCreated.Add(objectType, new EditableTypeDescriptor(objectType));
                }
                return AlreadyCreated[objectType];
            }
        }
    }
}
