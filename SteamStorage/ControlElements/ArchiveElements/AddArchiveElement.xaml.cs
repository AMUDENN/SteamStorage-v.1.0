using SteamStorage.ApplicationLogic;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SteamStorage.ControlElements
{
    public partial class AddArchiveElement : UserControl
    {
        public AddArchiveElement()
        {
            InitializeComponent();
        }
        private void OkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Exception ex = ArchiveMethods.AddNewArchiveElement(Url.Text, Convert.ToInt32(Count.Text), Convert.ToDouble(CostPurchase.Text.Replace('.', ',')),
                    Convert.ToDouble(CostSold.Text.Replace('.', ',')), DateTime.Now, DateTime.Now);
                if (ex != null) Messages.Error(ex.Message);
                else
                {
                    Messages.Information("Скин успешко добавлен");
                    DeleteThisElement();
                    MainWindow.ArchivePageInstance.RefreshElements();
                }
            }
            catch
            {
                Messages.Error("Введите корректные данные");
            }
        }
        private void DeleteThisElement()
        {
            MainWindow.ArchivePageInstance.MainStackPanel.Children.Remove(this);
            MainWindow.ArchivePageInstance.MainStackPanel.Children[0].Visibility = Visibility.Visible;
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            DeleteThisElement();
        }
        private void IntPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!"0123456789".Contains(e.Text)) e.Handled = true;
        }
        private void DoublePreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!"0123456789.,".Contains(e.Text) || ((TextBox)sender).Text.Count(x => ".,".Contains(x)) == 1 && ".,".Contains(e.Text)) e.Handled = true;
        }
    }
}
