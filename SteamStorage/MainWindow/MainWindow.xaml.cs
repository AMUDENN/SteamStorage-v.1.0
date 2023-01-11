using System.Windows;
using System.ComponentModel;
using SteamStorage.Pages;
using SteamStorage.ApplicationLogic;


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
            MainFrame.Content = RemainsPageInstance;
            MaxHeight = System.Windows.SystemParameters.PrimaryScreenHeight - 26;
        }
        private void ShowCloseMessage(object sender, CancelEventArgs e)
        {
            if (!Messages.ActionConfirmation("Вы уверены, что хотите закрыть приложение?")) e.Cancel = true;
        }
    }
}
