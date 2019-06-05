using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace EditableCollections.Editable
{
    public class EditableSet<TElement> where TElement : class, new()
    {
        private readonly ObservableCollection<Editable<TElement>> _editableOriginals = new ObservableCollection<Editable<TElement>>();
        private readonly ObservableCollection<Editable<TElement>> _inserted;
        private readonly ObservableCollection<Editable<TElement>> _allItems;
        private readonly EditableCommand<Editable<TElement>> _undoItemChangesCommand;
        private readonly EditableCommand<Editable<TElement>> _deleteItemCommand;
        private readonly EditableCommand _undoChangesCommand;
        private readonly EditableCommand _commitChangesCommand;
        private readonly Action<TElement> _editItemAction;
        private readonly Action<TElement> _addItemAction;
        private readonly Action<TElement> _deleteItemAction;

        public EditableSet(ObservableCollection<TElement> originals, Action<TElement> editItemAction, Action<TElement> addItemAction, Action<TElement> deleteItemAction)
        {
            OriginalItems = originals;
            _deleteItemAction = deleteItemAction;
            _addItemAction = addItemAction;
            _editItemAction = editItemAction;

            _inserted = new ObservableCollection<Editable<TElement>>();
            _editableOriginals.AddRange(OriginalItems.Select(orig => new Editable<TElement>(orig)));
            _allItems = new ObservableCollection<Editable<TElement>>(_editableOriginals);
            _allItems.CollectionChanged += CollectionChangedMethod;

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

        public ObservableCollection<TElement> OriginalItems { get; }

        public ObservableCollection<Editable<TElement>> EditableItems => _allItems;

        public ObservableCollection<Editable<TElement>> AllItems
        {
            get
            {
                var items = _editableOriginals;
                _allItems.Clear();
                foreach (var item in _inserted)
                {
                    if (!items.Contains(item))
                    {
                        _allItems.Add(item);
                    }
                }

                _allItems.AddRange(items);
                return _allItems;
            }
        }

        public ObservableCollection<Editable<TElement>> ChangedItems
        {
            get { return new ObservableCollection<Editable<TElement>>(EditableItems.Where(editable => editable.HasChanges)); }
        }

        public ObservableCollection<Editable<TElement>> DeletedItems
        {
            get { return new ObservableCollection<Editable<TElement>>(_editableOriginals.Where(editable => editable.IsDeleted)); }
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

        private void CollectionChangedMethod(object sender, NotifyCollectionChangedEventArgs e)
        {
            var list = (e.NewItems.Cast<object>().Select(item => item as Editable<TElement>)).ToList();

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in list)
                {
                    if (_inserted.Contains(item)) continue;
                    AddNewItem(item);
                }
            }

            if (e.Action != NotifyCollectionChangedAction.Remove) return;
            {
                foreach (var item in list)
                {
                    DeleteItem(item);
                }
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