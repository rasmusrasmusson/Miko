using Microsoft.EntityFrameworkCore;
using MikoMe.Helpers;
using MikoMe.Models;
using MikoMe.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MikoMe.ViewModels
{
    public class LanguageSessionViewModel : ObservableObject
    {
        private string _hanzi;
        public string Hanzi
        {
            get => _hanzi;
            set => SetProperty(ref _hanzi, value);
        }

        private string _pinyin;
        public string Pinyin
        {
            get => _pinyin;
            set => SetProperty(ref _pinyin, value);
        }

        public async Task LoadDataAsync()
        {
            // Fetch one Word from the database (for testing purposes)
            var db = DatabaseService.Context;
            var word = await db.Words.FirstOrDefaultAsync(); // For testing purposes, fetch one word

            if (word != null)
            {
                Hanzi = word.Hanzi;
                Pinyin = word.Pinyin;
            }
        }

        public LanguageSessionViewModel()
        {
            Task.Run(() => LoadDataAsync()); // Load data asynchronously
        }
    }
}
