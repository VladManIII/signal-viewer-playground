using System.Windows;

using CommunityToolkit.Mvvm.ComponentModel;

using SignalViewerPlayground.Pages;

namespace SignalViewerPlayground
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : BaseWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }

    public partial class MainWindowViewModel : BaseViewModel
    {
        [ObservableProperty] string _status = "Placeholder";

        public MainWindowViewModel()
        {
            Status = "Application started.";
        }
        protected override async void OnLoaded(RoutedEventArgs args)
        {
            base.OnLoaded(args);

            IsBusy = true;

            try
            {
                Status = "Loading data...";
                await LoadData();
                Status = "Data loaded successfully.";
            }
            catch (Exception ex)
            {
                Status = $"Error loading data: {ex.Message}";
            }
            finally { IsBusy = false; }
        }

        private Task LoadData()
        {
            return Task.Delay(5000);
        }
    }
}