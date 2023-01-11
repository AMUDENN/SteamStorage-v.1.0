using SteamStorage.ApplicationLogic;
using SteamStorage.Pages;
using SteamStorageDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SteamStorage.ControlElements
{
    public partial class ChangeRemainElement : UserControl
    {
        readonly RemainElementFull RemainElementFull;
        public int IdGroup { get; set; }
        public ChangeRemainElement(RemainElementFull remainElementFull)
        {
            InitializeComponent();
            RemainElementFull = remainElementFull;
            IdGroup = (int)(RemainsMethods.CurrentGroupId is null ? 1 : RemainsMethods.CurrentGroupId);
            GroupsComboBoxInit();
            TextBoxesInit();
        }
        private void GroupsComboBoxInit()
        {
            List<RemainGroups> Groups = RemainsMethods.GetRemainGroups();
            foreach (RemainGroups item in Groups)
            {
                ComboBoxItem comboBoxItem = new();
                comboBoxItem.Content = item.Title;
                if (item.Id == IdGroup) comboBoxItem.IsSelected = true;
                GroupsComboBox.Items.Add(comboBoxItem);
            }
        }
        private void TextBoxesInit()
        {
            Url.Text = GeneralMethods.GetSkin(RemainElementFull.Id_skin).Url;
            Count.Text = RemainElementFull.Count.ToString();
            Cost.Text = RemainElementFull.CostPurchase.ToString();
        }
        private void OkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Exception ex = RemainsMethods.ChangeRemainElement(RemainElementFull, Url.Text, Convert.ToInt32(Count.Text), Convert.ToDouble(Cost.Text.Replace('.', ',')), RemainElementFull.DatePurchase, ((ComboBoxItem)GroupsComboBox.SelectedItem).Content.ToString());
                if (ex != null) Messages.Error(ex.Message);
                else
                {
                    Messages.Information("Скин успешко изменён");
                    MainWindow.RemainsPageInstance.RefreshElements(RemainsMethods.CurrentGroupId);
                }
            }
            catch
            {
                Messages.Error("Введите корректные данные");
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            MainWindow.RemainsPageInstance.RefreshElements(RemainsMethods.CurrentGroupId);
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
