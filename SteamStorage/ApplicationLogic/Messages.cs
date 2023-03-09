using SteamStorage.CustomWindows;

namespace SteamStorage.ApplicationLogic
{
    internal class Messages
    {
        public static bool ActionConfirmation(string question)
            => (bool)new CustomMessageBox("Подтверждение", question, CustomMessageBox.MessageBoxButton.OkCancel, CustomMessageBox.MessageBoxImage.Question).ShowDialog();
        public static bool Information(string info)
            => (bool)new CustomMessageBox("Уведомление", info, CustomMessageBox.MessageBoxButton.Ok, CustomMessageBox.MessageBoxImage.Info).ShowDialog();
        public static bool Error(string text)
            => (bool)new CustomMessageBox("Ошибка", text, CustomMessageBox.MessageBoxButton.Ok, CustomMessageBox.MessageBoxImage.Error).ShowDialog();
    }
}
