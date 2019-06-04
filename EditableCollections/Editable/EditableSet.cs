using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Bindable.Linq;
using Bindable.Linq.Interfaces;
using Prism.Commands;

namespace EditableCollections.Editable
{
    public class EditableSet<TElement> where TElement : class, new()
    {
        private readonly IBindableCollection<Editable<TElement>> _editableOriginals;
        private readonly ObservableCollection<Editable<TElement>> _inserted;
        private readonly EditableCommand<Editable<TElement>> _undoItemChangesCommand;
        private readonly EditableCommand<Editable<TElement>> _deleteItemCommand;
        private readonly EditableCommand _undoChangesCommand;
        private readonly EditableCommand _commitChangesCommand;
        private readonly Action<TElement> _editItemAction;
        private readonly Action<TElement> _addItemAction;
        private readonly Action<TElement> _deleteItemAction;

        public EditableSet(IBindableCollection<TElement> originals, Action<TElement> editItemAction, Action<TElement> addItemAction, Action<TElement> deleteItemAction)
        {
            OriginalItems = originals;
            _deleteItemAction = deleteItemAction;
            _addItemAction = addItemAction;
            _editItemAction = editItemAction;

            _inserted = new ObservableCollection<Editable<TElement>>();
            _editableOriginals = OriginalItems.AsBindable().Select(orig => new Editable<TElement>(orig));
            _editableOriginals.Evaluate();

            _undoItemChangesCommand = new EditableCommand<Editable<TElement>>(
                UndoItemChanges,
                item => item.HasChanges
                );
            _deleteItemCommand = new EditableCommand<Editable<TElement>>(
                DeleteItem,
                item => true
                );
            _undoChangesCommand = new EditableCommand(ignored => UndoChanges(), ignored => true);
            _commitChangesCommand = new EditableCommand(ignored => CommitChanges(), ignored => true);
        }

        
        public ICommand DeleteItemCommand => _deleteItemCommand;

        public ICommand UndoItemChangesCommand => _undoItemChangesCommand;

        public ICommand UndoChangesCommand => _undoChangesCommand;

        public ICommand CommitChangesCommand => _commitChangesCommand;

        public IBindableCollection<TElement> OriginalItems { get; }

        public IBindableCollection<Editable<TElement>> EditableItems
        {
            get
            {
                return AllItems.Where(e => e.IsDeleted == false);
            }
        }

        public IBindableCollection<Editable<TElement>> AllItems
        {
            get
            {
                var items = _editableOriginals;
                items = items.Union(_inserted.AsBindable());
                items.Evaluate();
                return items;
            }
        }

        public IBindableCollection<Editable<TElement>> ChangedItems
        {
            get { return EditableItems.Where(editable => editable.HasChanges); }
        }

        public IBindableCollection<Editable<TElement>> DeletedItems
        {
            get { return _editableOriginals.Where(editable => editable.IsDeleted); }
        }

        public ObservableCollection<Editable<TElement>> Inserted => _inserted;

        public void UndoChanges()
        {
            foreach (var editable in ChangedItems.ToArray())
            {
                editable.UndoChanges();
            }
            foreach (var deleted in DeletedItems.ToArray())
            {
                deleted.UndoChanges();
            }
            _inserted.Clear();
        }

        public void UndoItemChanges(Editable<TElement> item)
        {
            item.UndoChanges();
            if (_inserted.Contains(item))
            {
                _inserted.Remove(item);
            }
        }

        public void AddNewItem(Editable<TElement> item)
        {
            item.IsNew = true;
            _inserted.Add(item);
        }

        private void DeleteItem(Editable<TElement> item)
        {
            item.IsDeleted = true;
        }

        public TElement[] CommitChanges()
        {
            var results = new List<TElement>();
            foreach (var editable in ChangedItems.ToArray())
            {
                editable.CommitChanges();
                results.Add(editable.Original);
            }
            foreach (var inserted in Inserted.ToArray())
            {
                inserted.CommitChanges();
                results.Add(inserted.Original);
            }
            _inserted.Clear();

            return results.ToArray();
        }
    }
}