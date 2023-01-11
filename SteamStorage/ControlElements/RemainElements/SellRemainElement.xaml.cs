using SteamStorage.ApplicationLogic;
using SteamStorageDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SteamStorage.ControlElements
{
    public partial class SellRemainElement : UserControl
    {
        public RemainElementFull RemainElementFull { get; set; }
        public SellRemainElement(RemainElementFull remainElementFull)
        {
            InitializeComponent();
            RemainElementFull = remainElementFull;
            GroupsComboBoxInit();
            TextBoxesInit();
        }

        private void GroupsComboBoxInit()
        {
            List<ArchiveGroups> Groups = ArchiveMethods.GetArchiveGroups();
            foreach (ArchiveGroups item in Groups)
            {
                ComboBoxItem comboBoxItem = new()
                {
                    Content = item.Title
                };
                if (item.Title == "Без группы") comboBoxItem.IsSelected = true;
                GroupsComboBox.Items.Add(comboBoxItem);
            }
        }
        private void TextBoxesInit()
        {
            Title.Text = RemainElementFull.Title;
            Count.Text = "0";
            Cost_purchase.Text = RemainElementFull.CostPurchase.ToString();
            Cost_sold.Text = "0";
        }
        private void OkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                string Url = GeneralMethods.GetSkin(RemainElementFull.Id_skin).Url;
                int count = Math.Min(Convert.ToInt32(Count.Text), RemainElementFull.Count);
                Exception ex = ArchiveMethods.AddNewArchiveElement(Url, count, Convert.ToDouble(Cost_purchase.Text.Replace('.', ',')),
                    Convert.ToDouble(Cost_sold.Text.Replace('.', ',')), RemainElementFull.DatePurchase, DateTime.Now,
                    ArchiveMethods.GetArchiveGroups().Where(x => x.Title == ((ComboBoxItem)GroupsComboBox.SelectedItem).Content.ToString()).First().Id);
                if (count == RemainElementFull.Count)
                {
                    RemainsMethods.DeleteRemainElement(RemainElementFull);
                }
                else
                {
                    RemainsMethods.ChangeRemainElement(RemainElementFull, Url, RemainElementFull.Count - count, RemainElementFull.CostPurchase, RemainElementFull.DatePurchase,
                        RemainsMethods.GetRemainGroups().Where(x => x.Id == RemainsMethods.CurrentGroupId).First().Title);
                }
                if (ex != null) Messages.Error(ex.Message);
                else
                {
                    Messages.Information("Скин успешко перемещён в архив");
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
