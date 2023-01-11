using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SteamStorage.CustomWindows
{
    public partial class CustomMessageBox : Window
    {
        readonly Style MainButtonStyle = (Style)Application.Current.Resources["MainButtonStyle"];
        public enum MessageBoxButton { Ok, OkCancel };
        public enum MessageBoxImage { Info, Error, Question };
        public CustomMessageBox(string title, string text, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
            InitializeComponent();
            Title.Text = title;
            Text.Text = text;
            switch (messageBoxButton)
            {
                case MessageBoxButton.Ok:
                    SetOk();
                    break;
                case MessageBoxButton.OkCancel:
                    SetOkCancel();
                    break;
            }
            switch (messageBoxImage)
            {
                case MessageBoxImage.Info:
                    SetImageSource("Info.png");
                    break;
                case MessageBoxImage.Error:
                    SetImageSource("Error.png");
                    break;
                case MessageBoxImage.Question:
                    SetImageSource("Question.png");
                    break;
            }
        }
        private void SetOk()
        {
            Button ok = new()
            {
                Style = MainButtonStyle,
                Content = "Ок",
                Margin = new Thickness(3)
            };
            ok.Click += OkClick;
            Grid.SetRow(ok, 1);
            Grid.SetColumn(ok, 3);
            MainGrid.Children.Add(ok);
        }
        private void SetOkCancel()
        {
            Button ok = new()
            {
                Style = MainButtonStyle,
                Content = "Ок",
                Margin = new Thickness(3)
            };
            ok.Click += OkClick;
            Grid.SetRow(ok, 1);
            Grid.SetColumn(ok, 2);
            MainGrid.Children.Add(ok);

            Button cancel = new()
            {
                Style = MainButtonStyle,
                Content = "Отмена",
                Margin = new Thickness(3)
            };
            cancel.Click += CancelClick;    
            Grid.SetRow(cancel, 1);
            Grid.SetColumn(cancel, 3);
            MainGrid.Children.Add(cancel);
        }
        private void SetImageSource(string name)
        {
            Image.Source = new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"../../../../Resources/Images/" + name));
        }
        private void DragMove(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
                this.DragMove();
        }
        private void OkClick(object sender, RoutedEventArgs e)
            => DialogResult = true;
        private void CancelClick(object sender, RoutedEventArgs e)
            => DialogResult = false;
    }
}
