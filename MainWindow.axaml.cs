using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace CrawlBoost
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OpenButton_Click(object? sender, RoutedEventArgs e)
        {
            OpenButton.IsEnabled = false;
            LoadingBar.IsVisible = true;

            await Task.Delay(2000);

            MainScreen.IsVisible = false;
            SecondScreen.IsVisible = true;
        }

        private void BackButton_Click(object? sender, RoutedEventArgs e)
        {
            SecondScreen.IsVisible = false;
            MainScreen.IsVisible = true;

            LoadingBar.IsVisible = false;
            OpenButton.IsEnabled = true;
        }
    }
}