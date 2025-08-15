
using HuaCards.Helpers;
using HuaCards.Models;
using HuaCards.Services;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace HuaCards.ViewModels;

public class BrowseViewModel : ObservableObject
{
    public ObservableCollection<Word> Items { get; } = new();

    public async Task LoadAsync()
    {
        Items.Clear();
        var all = await DatabaseService.Context.Words.AsNoTracking().ToListAsync();
        foreach (var w in all) Items.Add(w);
    }
}
