using System;
using System.Windows.Input;

namespace EditableCollections.Editable
{
    public class EditableCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool> _canExecute;

        public EditableCommand(Action<T> executedAction, Func<T, bool> canExecute)
        {
            _execute = executedAction;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T) parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute?.Invoke((T) parameter);
        }
    }

    public class EditableCommand : EditableCommand<object>
    {
        public EditableCommand(Action<object> executedAction, Func<object, bool> canExecute): base(executedAction, canExecute)
        {}
    }
}