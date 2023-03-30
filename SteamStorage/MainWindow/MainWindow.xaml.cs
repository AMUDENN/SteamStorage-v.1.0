using SteamStorage.ApplicationLogic;
using SteamStorage.Pages;
using System.ComponentModel;
using System.Windows;

namespace SteamStorage
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        public static RemainsPage RemainsPageInstance { get; private set; }
        public static ArchivePage ArchivePageInstance { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
            Closing += ShowCloseMessage;
            Instance = this;
            RemainsPageInstance = new RemainsPage();
            ArchivePageInstance = new ArchivePage();
            RemainsPageInstance.Version.Text = Constants.Version;
            ArchivePageInstance.Version.Text = Constants.Version;
            MainFrame.Content = RemainsPageInstance;

            double height = SystemParameters.PrimaryScreenHeight / 1.75;
            double width = SystemParameters.PrimaryScreenWidth / 1.75;
            Height = height > 630 ? 630 : height;
            Width = width > 1120 ? 1120 : width;
        }
        private void ShowCloseMessage(object sender, CancelEventArgs e)
        {
            if (!Messages.ActionConfirmation("Вы уверены, что хотите закрыть приложение?")) e.Cancel = true;
        }
    }
}
