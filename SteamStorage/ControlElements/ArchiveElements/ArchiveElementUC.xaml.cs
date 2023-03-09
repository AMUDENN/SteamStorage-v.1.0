using SteamStorage.ApplicationLogic;
using System.Windows;
using System.Windows.Controls;

namespace SteamStorage.ControlElements
{
    public partial class ArchiveElementUC : UserControl
    {
        public AdvancedArchive ArchiveElementFull { get; set; }
        public ArchiveElementUC(AdvancedArchive archiveElementFull)
        {
            InitializeComponent();
            DataContext = this;
            ArchiveElementFull = archiveElementFull;
        }
        private void ChangeClick(object sender, RoutedEventArgs e)
        {
            var childrens = MainWindow.ArchivePageInstance.MainStackPanel.Children;
            int index = childrens.IndexOf(this);
            childrens.Remove(this);
            childrens.Insert(index, new ChangeArchiveElement(ArchiveElementFull));
        }
        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            if (!Messages.ActionConfirmation($"Вы уверены, что хотите удалить скин {ArchiveElementFull.Title}?")) return;
            ArchiveMethods.DeleteArchiveElement(ArchiveElementFull);
            MainWindow.ArchivePageInstance.RefreshElements();
        }
    }
}
