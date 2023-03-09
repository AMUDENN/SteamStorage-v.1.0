using SteamStorage.ApplicationLogic;
using SteamStorage.ControlElements;
using SteamStorage.SteamStorageDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SteamStorage.Pages
{
    public partial class ArchivePage : Page
    {
        readonly Style MainButtonStyle = (Style)Application.Current.Resources["MainButtonStyle"];
        readonly Style NowPressedButton = (Style)Application.Current.Resources["NowPressedButton"];
        readonly Style SeparatorStyle = (Style)Application.Current.Resources["SeparatorStyle"];
        private new readonly ContextMenu ContextMenu;
        public ArchivePage()
        {
            InitializeComponent();
            ContextMenu = MakeContextMenu();
            RefreshElements();
            DisplayGroups();
        }
        public void RefreshElements()
        {
            List<AdvancedArchive> archiveElements = ArchiveMethods.GetArchiveElements(ArchiveMethods.CurrentGroup);
            RefreshArchiveElements(archiveElements);
            string title = ArchiveMethods.CurrentGroup is null ? "Все" : ArchiveMethods.CurrentGroup.Title;
            ChangeGroupButtonsStyle(title);
            RefreshConclusion(archiveElements);
        }
        public void DisplayGroups()
        {
            GroupStackPanel.Children.RemoveRange(1, GroupStackPanel.Children.Count - 1);
            List<ArchiveGroup> groups = ArchiveMethods.GetArchiveGroups();

            foreach (var group in groups)
            {
                Button button = new()
                {
                    Style = MainButtonStyle,
                    Content = group.Title,
                    Height = 50
                };
                button.Click += GetGroupElementsClick;
                if (group.Title != "Без группы") button.ContextMenu = ContextMenu;
                GroupStackPanel.Children.Add(button);
            }
        }
        private ContextMenu MakeContextMenu()
        {
            ContextMenu contextMenu = new();
            MenuItem ChangeItem = new()
            {
                Header = "Изменить"
            };
            ChangeItem.Click += ChangeGroupClick;
            contextMenu.Items.Add(ChangeItem);

            Separator separator = new();
            separator.Style = SeparatorStyle;
            contextMenu.Items.Add(separator);

            MenuItem DeleteItem = new()
            {
                Header = "Удалить группу"
            };
            DeleteItem.Click += DeleteGroupClick;
            contextMenu.Items.Add(DeleteItem);

            separator = new();
            separator.Style = SeparatorStyle;
            contextMenu.Items.Add(separator);

            MenuItem DeleteAllItem = new()
            {
                Header = "Удалить группу и скины"
            };
            DeleteAllItem.Click += DeleteGroupAndArchiveClick;
            contextMenu.Items.Add(DeleteAllItem);

            return contextMenu;
        }
        private void AddGroupClick(object sender, RoutedEventArgs e)
        {
            AddButton.Visibility = Visibility.Collapsed;
            GroupsGrid.Children.Insert(0, new AddGroup());
        }
        private void ChangeGroupClick(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = ((MenuItem)sender).Parent as ContextMenu;
            Button btn = cm.PlacementTarget as Button;
            int index = GroupStackPanel.Children.IndexOf(btn);
            GroupStackPanel.Children[index].Visibility = Visibility.Collapsed;
            ChangeGroup changeGroup = new(btn.Content.ToString())
            {
                Height = 50
            };
            GroupStackPanel.Children.Insert(index, changeGroup);
        }
        private void DeleteGroupClick(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = ((MenuItem)sender).Parent as ContextMenu;
            Button btn = cm.PlacementTarget as Button;
            if (!Messages.ActionConfirmation($"Вы уверены, что хотите удалить группу: \"{btn.Content}\"?")) return;
            ArchiveMethods.DeleteGroupArchive(btn.Content.ToString());
            DisplayGroups();
            RefreshElements();
        }
        private void DeleteGroupAndArchiveClick(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = ((MenuItem)sender).Parent as ContextMenu;
            Button btn = cm.PlacementTarget as Button;
            if (!Messages.ActionConfirmation($"Вы уверены, что хотите удалить группу: \"{btn.Content}\" и все скины, находящиеся в ней?")) return;
            ArchiveMethods.DeleteGroupArchiveAndArchiveElements(btn.Content.ToString());
            DisplayGroups();
            RefreshElements();
        }
        private void GetGroupElementsClick(object sender, RoutedEventArgs e)
        {
            string content = ((Button)sender).Content.ToString();

            ArchiveMethods.ChangeCurrentGroupId(content);

            RefreshElements();
        }
        private void ChangeGroupButtonsStyle(string content)
        {
            var childrens = GroupStackPanel.Children;

            foreach (Button item in childrens)
            {
                if (item.Content.ToString() == content) item.Style = NowPressedButton;
                else item.Style = MainButtonStyle;
            }
        }
        private void OrderByClick(object sender, RoutedEventArgs e)
        {
            List<AdvancedArchive> archiveElements = ArchiveMethods.GetOrderedArchiveElements(((Button)sender).Content.ToString());
            RefreshArchiveElements(archiveElements);
        }
        private void RefreshArchiveElements(List<AdvancedArchive> archiveElements)
        {
            MainStackPanel.Children.RemoveRange(1, MainStackPanel.Children.Count - 1);
            foreach (var item in archiveElements)
            {
                ArchiveElementUC skinElement = new(item);
                MainStackPanel.Children.Add(skinElement);
            }
        }
        public void RefreshConclusion(List<AdvancedArchive> archiveElements)
        {
            double totalAmount = archiveElements.Select(x => x.Amount).Sum();
            double totalCount = archiveElements.Select(x => x.Count).Sum();
            double saleAmount = archiveElements.Select(x => x.Count * x.CostSold).Sum();
            double percent = Math.Round((saleAmount - totalAmount) / totalAmount * 100, 2);
            TotalCount.Text = totalCount.ToString();
            AvgCost.Text = Math.Round(totalAmount / totalCount, 2).ToString();
            TotalAmount.Text = Math.Round(totalAmount, 2).ToString();
            SaleAmount.Text = Math.Round(saleAmount, 2).ToString();
            PercentChange.Text = (totalAmount <= saleAmount ? "+" : "") + percent + "%";
            PercentChange.Foreground = totalAmount <= saleAmount ? Brushes.Green : Brushes.Red;
        }
        private void AddNewArchiveElementClick(object sender, RoutedEventArgs e)
        {
            ((UIElement)sender).Visibility = Visibility.Collapsed;
            MainStackPanel.Children.Insert(0, new AddArchiveElement());
        }
        private void RemainsClick(object sender, RoutedEventArgs e)
        {
            GeneralMethods.ChangeCurrentPage(GeneralMethods.ApplicationPages.Remains);
            MainWindow.Instance.MainFrame.Content = MainWindow.RemainsPageInstance;
            MainWindow.RemainsPageInstance.RefreshElements();
        }
    }
}
