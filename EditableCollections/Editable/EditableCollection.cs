using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace EditableCollections.Editable
{
    public class EditableCollection<TElement> : IEditableCollectionViewAddNewItem where TElement : class, new()
    {
        private ObservableCollection<Editable<TElement>> _list = new ObservableCollection<Editable<TElement>>();
        private ObservableCollection<Editable<TElement>> _added = new ObservableCollection<Editable<TElement>>();
        private ObservableCollection<Editable<TElement>> _deleted = new ObservableCollection<Editable<TElement>>();
        private ObservableCollection<TElement> _originals = new ObservableCollection<TElement>();

        public EditableCollection(IEnumerable<TElement> items)
        {
            foreach (var item in items)
            {
                _list.Add(new Editable<TElement>(item));
                _originals.Add(item);
            }
        }



        public object AddNew()
        {
            var item = new Editable<TElement>(new TElement());
            _added.Add(item);
            CurrentAddItem = item;
            IsAddingNew = true;
            CanAddNew = false;
            CanRemove = false;
            return item;
        }

        public void CommitNew()
        {
            _list.AddRange(_added);
            CurrentAddItem = null;
            IsAddingNew = false;
            CanAddNew = true;
            CanRemove = true;
            _added.Clear();
        }

        public void CancelNew()
        {
            _added.Clear();
            IsAddingNew = false;
            CurrentAddItem = null;
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _deleted.Add(item);
            _list.Remove(item);
        }

        public void Remove(object item)
        {
            _deleted.Add(new Editable<TElement>(item as TElement));
            var toRemove = _list.First(x => x.Original == item);
            _list.Remove(toRemove);
        }

        public void EditItem(object item)
        {
            if (!(item is Editable<TElement> obj)) return;
            obj.BeginChanges();
            CanAddNew = false;
        }

        public void CommitEdit()
        {
            foreach (var editable in _list)
            {
                editable.CommitChanges();
                CanAddNew = true;
            }
        }

        public void CancelEdit()
        {
            foreach (var editable in _list)
            {
                editable.UndoChanges();
                CanAddNew = true;
            }
        }

        public NewItemPlaceholderPosition NewItemPlaceholderPosition { get; set; } = NewItemPlaceholderPosition.AtEnd;
        public bool CanAddNew { get; set; } = true;
        public bool IsAddingNew { get; set; }
        public object CurrentAddItem { get; set; }
        public bool CanRemove { get; set; }
        public bool CanCancelEdit { get; set; }
        public bool IsEditingItem { get; set; }
        public object CurrentEditItem { get; set; }
        public object AddNewItem(object newItem)
        {
            if (!(newItem is Editable<TElement> item)) return newItem;
            _added.Add(item);
            CurrentAddItem = item;
            IsAddingNew = true;
            CanAddNew = false;
            CanRemove = false;
            return item;
        }

        public bool CanAddNewItem
        {
            get => CanAddNew;
            set => CanAddNew = value;
        }
    }
}