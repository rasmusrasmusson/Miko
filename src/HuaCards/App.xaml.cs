using Microsoft.UI.Xaml;

namespace HuaCards
{
    public partial class App : Application
    {
        // Allow MainWindow to set the instance (public set).
        public static Window? MainWindowInstance { get; set; }

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            MainWindowInstance = new MainWindow();
            MainWindowInstance.Activate();
        }
    }
}
