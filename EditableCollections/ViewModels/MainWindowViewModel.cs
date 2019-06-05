using System.Linq;
using System.Windows;
using EditableCollections.Editable;
using Prism.Commands;
using Prism.Mvvm;

namespace EditableCollections.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private readonly CustomerRepository _customerRepository = new CustomerRepository();
        private bool _showDeletedItems;

        public MainWindowViewModel()
        {
            DataSource = new EditableSet<Customer>(
                _customerRepository.GetCustomers(),
                edit => _customerRepository.SaveCustomer(edit),
                add => _customerRepository.SaveCustomer(add),
                delete => _customerRepository.DeleteCustomer(delete)
                );

            ClickSave = new DelegateCommand(Save);
            ClickUndo = new DelegateCommand(Undo);
            ClickUndoItem = new DelegateCommand(UndoItem);
            ClickDelete = new DelegateCommand(DeleteItem);
        }

        public DelegateCommand ClickCreatingNewItem { get; set; }
        public DelegateCommand CommittingNewItem { get; set; }
        public DelegateCommand CancellingNewItem { get; set; }
        public  DelegateCommand ClickSave { get; private set; }
        public DelegateCommand ClickUndo { get; }
        public DelegateCommand ClickUndoItem { get; }
        public DelegateCommand ClickDelete { get; }


        private bool CanSave(object arg)
        {
            return DataSource.ChangedItems.Any();
        }

        public Editable<Customer> SelectedPerson { get; set; }= new Editable<Customer>();

        public EditableSet<Customer> DataSource { get; }

        public bool ShowDeletedItems
        {
            get => _showDeletedItems;
            set => SetProperty(ref _showDeletedItems, value);
        }

        private void Save()
        {
            // Delete items that were marked for deletion
            foreach (var item in DataSource.DeletedItems)
            {
                _customerRepository.DeleteCustomer(item.Original);
            }

            // Commit the changes in memory
            var changed = DataSource.CommitChanges();

            // Save the edited items
            foreach (var item in changed)
            {
                _customerRepository.SaveCustomer(item);
            }
            RaisePropertyChanged(string.Empty);
        }

        private void Undo()
        {
            DataSource.UndoChanges();
            RaisePropertyChanged(string.Empty);
        }

        private void UndoItem()
        {
            if (SelectedPerson.HasChanges)
            {
                SelectedPerson.UndoChanges();
            }
        }

        private void SimulateRemoteUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _customerRepository.SaveCustomer(new Customer { FirstName = "Mary", LastName = "Mellon" });
        }

        
        private void DeleteItem()
        {
            if (SelectedPerson != null)
            {
                SelectedPerson.IsDeleted = true;
            }
        }

    }
}
