using System.Windows;
using System.Windows.Controls;

namespace FtpClient.DependencyProperties
{
    static class DataGridUtils
    {
        public static readonly DependencyProperty DisplayRowNumberProperty =
            DependencyProperty.RegisterAttached("DisplayRowNumber", typeof(bool), typeof(DataGridUtils),
            new PropertyMetadata(false, DisplayRowNumberPropertyChanged));

        public static void SetDisplayRowNumber(DependencyObject dp, bool value)
        {
            dp.SetValue(DisplayRowNumberProperty, value);
        }

        public static bool GetDisplayRowNumber(DependencyObject dp)
        {
            return (bool)dp.GetValue(DisplayRowNumberProperty);
        }

        private static void DisplayRowNumberPropertyChanged(DependencyObject dp,
            DependencyPropertyChangedEventArgs e)
        {
            var datagrid = dp as DataGrid;
            if (datagrid == null)
                return;
            if ((bool)e.NewValue)
            {
                datagrid.LoadingRow += DataGrid_LoadingRow;
            }
            else
            {
                datagrid.LoadingRow -= DataGrid_LoadingRow;
            }
        }

        private static void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var datagrid = sender as DataGrid;
            e.Row.Header = (e.Row.GetIndex() + 1).ToString(); 
        }
    }
}
