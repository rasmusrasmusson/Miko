using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MikoMe.ViewModels;

namespace MikoMe.Views
{
    public sealed partial class SessionPage : Page
    {
        public SessionPage()
        {
            InitializeComponent();
            // Set the DataContext for this page to the SessionViewModel
            DataContext = new SessionViewModel();
        }

        // Define Speak_Click method for the speaker button
        private void Speak_Click(object sender, RoutedEventArgs e)
        {
            // Call SpeakCommand in the ViewModel
            var viewModel = (SessionViewModel)DataContext;
            viewModel.SpeakCommand.Execute(null);
        }
    }
}
