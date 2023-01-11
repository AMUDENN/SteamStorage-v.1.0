using SteamStorage.ApplicationLogic;
using SteamStorage.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SteamStorage.ControlElements
{
    public partial class ChangeGroup : UserControl
    {
        public string OldTitle { get; set; }
        public ChangeGroup(string oldTitle)
        {
            InitializeComponent();
            OldTitle = oldTitle;
        }
        private void OkClick(object sender, RoutedEventArgs e) => GroupChange();
        private void GridKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) GroupChange();
        }
        private void CancelClick(object sender, RoutedEventArgs e) => DeleteAddGroup();
        private void LimitPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text.Length >= 15) e.Handled = true;
        }
        private void GroupChange()
        {
            Exception? ex = GeneralMethods.ChangeGroup(OldTitle, GroupName.Text[..Math.Min(15, GroupName.Text.Length)]);
            if (ex != null) Messages.Error("Введённое название недопустимо");
            else Messages.Information("Успешно изменено название группы!");
            DeleteAddGroup();
        }
        private void DeleteAddGroup()
        {
            if (GeneralMethods.CurrentPage == GeneralMethods.ApplicationPages.Remains)
            {
                MainWindow.RemainsPageInstance.GroupStackPanel.Children.Remove(this);
                MainWindow.RemainsPageInstance.DisplayGroups();
            }
            else if (GeneralMethods.CurrentPage == GeneralMethods.ApplicationPages.Archive)
            {
                MainWindow.ArchivePageInstance.GroupStackPanel.Children.Remove(this);
                MainWindow.ArchivePageInstance.DisplayGroups();
            }
        }
    }
}
