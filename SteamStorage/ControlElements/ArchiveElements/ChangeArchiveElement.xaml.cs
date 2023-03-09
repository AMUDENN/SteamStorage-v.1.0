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

    public partial class ChangeArchiveElement : UserControl
    {
        readonly AdvancedArchive ArchiveElementFull;
        public int IdGroup { get; set; }
        public ChangeArchiveElement(AdvancedArchive archiveElementFull)
        {
            InitializeComponent();
            ArchiveElementFull = archiveElementFull;
            IdGroup = (int)(ArchiveMethods.CurrentGroup is null ? 1 : ArchiveMethods.CurrentGroup.Id);
            GroupsComboBoxInit();
            TextBoxesInit();
        }
        private void GroupsComboBoxInit()
        {
            List<ArchiveGroup> Groups = ArchiveMethods.GetArchiveGroups();
            foreach (ArchiveGroup item in Groups)
            {
                ComboBoxItem comboBoxItem = new();
                comboBoxItem.Content = item.Title;
                if (item.Id == IdGroup) comboBoxItem.IsSelected = true;
                GroupsComboBox.Items.Add(comboBoxItem);
            }
        }
        private void TextBoxesInit()
        {
            Url.Text = GeneralMethods.GetSkin(ArchiveElementFull.IdSkin).Url;
            Count.Text = ArchiveElementFull.Count.ToString();
            CostPurchase.Text = ArchiveElementFull.CostPurchase.ToString();
            CostSold.Text = ArchiveElementFull.CostSold.ToString();
        }
        private void OkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Exception ex = ArchiveMethods.ChangeArchiveElement(ArchiveElementFull, Url.Text, Convert.ToInt32(Count.Text), Convert.ToDouble(CostPurchase.Text.Replace('.', ',')),
                    Convert.ToDouble(CostSold.Text.Replace('.', ',')), ArchiveElementFull.DatePurchase, ArchiveElementFull.DateSold, ((ComboBoxItem)GroupsComboBox.SelectedItem).Content.ToString());
                if (ex != null) Messages.Error(ex.Message);
                else
                {
                    Messages.Information("Скин успешко изменён");
                    MainWindow.ArchivePageInstance.RefreshElements();
                }
            }
            catch
            {
                Messages.Error("Введите корректные данные");
            }
        }
        private void CancelClick(object sender, RoutedEventArgs e)
        {
            MainWindow.ArchivePageInstance.RefreshElements();
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
