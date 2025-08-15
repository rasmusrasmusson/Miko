
using HuaCards.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace HuaCards.Views;

public sealed partial class BrowsePage : Page
{
    public BrowsePage()
    {
        this.InitializeComponent();
        this.Loaded += async (_, __) => await VM.LoadAsync();
    }

    public HuaCards.ViewModels.BrowseViewModel VM => (HuaCards.ViewModels.BrowseViewModel)DataContext;

    private async void Refresh_Click(object sender, RoutedEventArgs e)
        => await VM.LoadAsync();
}
protected override async void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var items = await DatabaseService.GetAllAsync(); // adjust to your method
        WordsList.ItemsSource = items;
        EmptyText.Visibility = (items == null || !items.Any()) ? Visibility.Visible : Visibility.Collapsed;
    }
