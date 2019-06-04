using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EditableCollections
{
    public static class DataGridAddRows
    {
        public static readonly DependencyProperty RegisterAddCommandProperty = DependencyProperty.RegisterAttached("RegisterAddCommand", typeof(bool), typeof(DataGridAddRows), 
            new PropertyMetadata(false, OnRegisterAddCommandChanged));

        public static bool GetRegisterAddCommand(DependencyObject obj)
        {
            return (bool)obj.GetValue(RegisterAddCommandProperty);
        }

        public static void SetRegisterAddCommand(DependencyObject obj, bool value)
        {
            obj.SetValue(RegisterAddCommandProperty, value);
        }

        private static void OnRegisterAddCommandChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is DataGrid dataGrid)) return;
            if ((bool)e.NewValue)
                dataGrid.CommandBindings.Add(new CommandBinding(AddCommand, AddCommand_Executed, AddCommand_CanExecute));
        }

        public static readonly RoutedUICommand AddCommand = new RoutedUICommand("AddCommand", "AddCommand", typeof(DataGridAddRows));

        private static void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dataGrid = sender as DataGrid;

            var itemsSourceType = dataGrid?.ItemsSource.GetType();
            if (itemsSourceType == null) return;
            var itemType = itemsSourceType.GetGenericArguments().Single();

            dataGrid.Items.Add(Activator.CreateInstance(itemType));
        }

        private static void AddCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((DataGrid) sender).CanUserAddRows;
        }
    }
}