using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditableCollections
{
    public class Customer : INotifyPropertyChanged
    {
        private string _firstName;
        private string _lastName;
        private int _age;

        public Customer()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                _firstName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("FirstName"));
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                _lastName = value;
                OnPropertyChanged(new PropertyChangedEventArgs("LastName"));
            }
        }

        public int Age
        {
            get { return _age; }
            set
            {
                _age = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Age"));
            }
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }
    }
}
