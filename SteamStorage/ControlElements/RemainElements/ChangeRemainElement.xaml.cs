using SteamStorage.ApplicationLogic;
using SteamStorage.SteamStorageDB;
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
        readonly AdvancedRemain RemainElementFull;
        public int IdGroup { get; set; }
        public ChangeRemainElement(AdvancedRemain remainElementFull)
        {
            InitializeComponent();
            RemainElementFull = remainElementFull;
            IdGroup = (int)(RemainsMethods.CurrentGroup is null ? 1 : RemainsMethods.CurrentGroup.Id);
            GroupsComboBoxInit();
            TextBoxesInit();
        }
        private void GroupsComboBoxInit()
        {
            List<RemainGroup> Groups = RemainsMethods.GetRemainGroups();
            foreach (RemainGroup item in Groups)
            {
                ComboBoxItem comboBoxItem = new();
                comboBoxItem.Content = item.Title;
                if (item.Id == IdGroup) comboBoxItem.IsSelected = true;
                GroupsComboBox.Items.Add(comboBoxItem);
            }
        }
        private void TextBoxesInit()
        {
            Url.Text = GeneralMethods.GetSkin(RemainElementFull.IdSkin).Url;
            Count.Text = RemainElementFull.Count.ToString();
            Cost.Text = RemainElementFull.CostPurchase.ToString();
        }
        private void OkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Exception ex = RemainsMethods.ChangeRemainElement(RemainElementFull, Url.Text, Convert.ToInt32(Count.Text), Convert.ToDouble(Cost.Text.Replace('.', ',')),
                    RemainElementFull.DatePurchase, ((ComboBoxItem)GroupsComboBox.SelectedItem).Content.ToString());
                if (ex != null) Messages.Error(ex.Message);
                else
                {
                    Messages.Information("Скин успешко изменён");
                    MainWindow.RemainsPageInstance.RefreshElements();
                }
            }
            catch
            {
                Messages.Error("Введите корректные данные");
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            MainWindow.RemainsPageInstance.RefreshElements();
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
