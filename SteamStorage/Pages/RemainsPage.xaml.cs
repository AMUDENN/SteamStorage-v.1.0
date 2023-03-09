using SteamStorage.ApplicationLogic;
using SteamStorage.ControlElements;
using SteamStorage.SteamStorageDB;
using SteamStorageDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SteamStorage.Pages
{
    public partial class RemainsPage : Page
    {
        readonly Style MainButtonStyle = (Style)Application.Current.Resources["MainButtonStyle"];
        readonly Style NowPressedButton = (Style)Application.Current.Resources["NowPressedButton"];
        readonly Style SeparatorStyle = (Style)Application.Current.Resources["SeparatorStyle"];
        readonly Style ProgressBarStyle = (Style)Application.Current.Resources["ProgressBarStyle"];
        private new readonly ContextMenu ContextMenu;
        public RemainsPage()
        {
            InitializeComponent();
            ContextMenu = MakeContextMenu();
            RefreshElements();
            DisplayGroups();
        }
        public void RefreshElements()
        {
            List<AdvancedRemain> remainElements = RemainsMethods.GetRemainElements(RemainsMethods.CurrentGroup);
            RefreshRemainElements(remainElements);
            string title = RemainsMethods.CurrentGroup is null ? "Все" : RemainsMethods.CurrentGroup.Title;
            ChangeGroupButtonsStyle(title);
            RefreshConclusion(remainElements);
        }
        public void DisplayGroups()
        {
            GroupStackPanel.Children.RemoveRange(1, GroupStackPanel.Children.Count - 1);
            List<RemainGroup> groups = RemainsMethods.GetRemainGroups();

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
            DeleteAllItem.Click += DeleteGroupAndRemainsClick;
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
            RemainsMethods.DeleteGroupRemain(btn.Content.ToString());
            DisplayGroups();
            RefreshElements();
        }
        private void DeleteGroupAndRemainsClick(object sender, RoutedEventArgs e)
        {
            ContextMenu cm = ((MenuItem)sender).Parent as ContextMenu;
            Button btn = cm.PlacementTarget as Button;
            if (!Messages.ActionConfirmation($"Вы уверены, что хотите удалить группу: \"{btn.Content}\" и все скины, находящиеся в ней?")) return;
            RemainsMethods.DeleteGroupRemainAndRemainElements(btn.Content.ToString());
            DisplayGroups();
            RefreshElements();
        }
        private void GetGroupElementsClick(object sender, RoutedEventArgs e)
        {
            string content = ((Button)sender).Content.ToString();

            RemainsMethods.ChangeCurrentGroup(content);

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
            List<AdvancedRemain> remainElements = RemainsMethods.GetOrderedRemainElements(((Button)sender).Content.ToString());
            RefreshRemainElements(remainElements);
        }
        private void RefreshRemainElements(List<AdvancedRemain> remainElements)
        {
            MainStackPanel.Children.RemoveRange(1, MainStackPanel.Children.Count - 1);
            foreach (var item in remainElements)
            {
                RemainElementUC skinElement = new(item);
                skinElement.Height = 110;
                MainStackPanel.Children.Add(skinElement);
            }
        }
        public void RefreshConclusion(List<AdvancedRemain> remainElements)
        {
            double totalAmount = remainElements.Select(x => x.Amount).Sum();
            double totalCount = remainElements.Select(x => x.Count).Sum();
            double currentAmount = remainElements.Select(x => x.Count * x.LastCost).Sum();
            double percent = Math.Round((currentAmount - totalAmount) / totalAmount * 100, 2);
            TotalCount.Text = totalCount.ToString();
            AvgCost.Text = Math.Round(totalAmount / totalCount, 2).ToString();
            TotalAmount.Text = Math.Round(totalAmount, 2).ToString();
            CurrentAmount.Text = Math.Round(currentAmount, 2).ToString();
            PercentChange.Text = (totalAmount <= currentAmount ? "+" : "") + percent + "%";
            PercentChange.Foreground = totalAmount <= currentAmount ? Brushes.Green : Brushes.Red;
        }
        private void AddNewRemainElementClick(object sender, RoutedEventArgs e)
        {
            ((UIElement)sender).Visibility = Visibility.Collapsed;
            MainStackPanel.Children.Insert(0, new AddRemainElement());
        }
        private void AddButtonAndProgressBar()
        {
            Button cancel = new()
            {
                Style = MainButtonStyle,
                Height = 40,
                Content = "Отмена"
            };
            cancel.Click += UpdateCancelClick;
            UpdateUIElements.Children.Insert(0, cancel);

            ProgressBar bar = new()
            {
                Style = ProgressBarStyle,
                Minimum = 0,
                Maximum = 100,
                Height = 34,
                Margin = new Thickness(3)
            };

            UpdateUIElements.Children.Insert(1, bar);

            UpdateUIElements.Children[2].Visibility = Visibility.Hidden;
            UpdateUIElements.Children[3].Visibility = Visibility.Hidden;
        }
        private void UpdateAllClick(object sender, RoutedEventArgs e)
        {
            if (!RemainsMethods.BackgroundWorker.IsBusy)
            {
                AddButtonAndProgressBar();
                RemainsMethods.BackgroundWorker.RunWorkerAsync(null);
            }
        }
        private void UpdateGroupClick(object sender, RoutedEventArgs e)
        {
            if (!RemainsMethods.BackgroundWorker.IsBusy)
            {
                AddButtonAndProgressBar();
                RemainsMethods.BackgroundWorker.RunWorkerAsync(RemainsMethods.CurrentGroup);
            }
        }
        private void UpdateCancelClick(object sender, RoutedEventArgs e)
        {
            RemainsMethods.BackgroundWorker.CancelAsync();
        }
        public void UpdateComplete()
        {
            UpdateUIElements.Children.RemoveRange(0, 2);
            UpdateUIElements.Children[0].Visibility = Visibility.Visible;
            UpdateUIElements.Children[1].Visibility = Visibility.Visible;
            RefreshElements();
        }
        private void ArchiveClick(object sender, RoutedEventArgs e)
        {
            GeneralMethods.ChangeCurrentPage(GeneralMethods.ApplicationPages.Archive);
            MainWindow.Instance.MainFrame.Content = MainWindow.ArchivePageInstance;
            MainWindow.ArchivePageInstance.RefreshElements();
        }
    }
}
