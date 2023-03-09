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
            MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 26;
        }
        private void ShowCloseMessage(object sender, CancelEventArgs e)
        {
            if (!Messages.ActionConfirmation("Вы уверены, что хотите закрыть приложение?")) e.Cancel = true;
        }
    }
}
