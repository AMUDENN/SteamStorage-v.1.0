using SteamStorage.ApplicationLogic;
using SteamStorage.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SteamStorage.ControlElements
{
    public partial class AddGroup : UserControl
    {
        public AddGroup()
        {
            InitializeComponent();
        }
        private void OkClick(object sender, RoutedEventArgs e) => GroupAdd();
        private void GridKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) GroupAdd();
        }
        private void CancelClick(object sender, RoutedEventArgs e) => DeleteAddGroup();
        private void LimitPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length >= 15) e.Handled = true;
        }
        private void GroupAdd()
        {
            Exception? ex = GeneralMethods.AddGroup(GroupName.Text[..Math.Min(15, GroupName.Text.Length)]);
            if (ex != null) Messages.Error("Введённое название недопустимо");
            else Messages.Information("Успешно добавлена группа!");
            DeleteAddGroup();
        }
        private void DeleteAddGroup()
        {
            if (GeneralMethods.CurrentPage == GeneralMethods.ApplicationPages.Remains)
            {
                MainWindow.RemainsPageInstance.GroupsGrid.Children.Remove(this);
                MainWindow.RemainsPageInstance.GroupsGrid.Children[0].Visibility = Visibility.Visible;
                MainWindow.RemainsPageInstance.DisplayGroups();
            }
            else if (GeneralMethods.CurrentPage == GeneralMethods.ApplicationPages.Archive)
            {
                MainWindow.ArchivePageInstance.GroupsGrid.Children.Remove(this);
                MainWindow.ArchivePageInstance.GroupsGrid.Children[0].Visibility = Visibility.Visible;
                MainWindow.ArchivePageInstance.DisplayGroups();
            }
        }
    }
}
