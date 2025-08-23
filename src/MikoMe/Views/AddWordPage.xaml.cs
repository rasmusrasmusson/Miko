using MikoMe.Models;
using MikoMe.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;

namespace MikoMe.Views
{
    public sealed partial class AddWordPage : Page
    {
        public AddWordPage()
        {
            InitializeComponent();
            HanziTextBox.TextChanged += HanziTextBox_TextChanged;
        }

        // ---- Paste ----
        private async void PasteButton_Click(object sender, RoutedEventArgs e) =>
            await PasteIntoAsync(HanziTextBox);

        private async void PasteKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            await PasteIntoAsync(HanziTextBox);
        }

        private static OcrEngine CreateBestOcrEngine()
        {
            var zh = new Language("zh-Hans");
            return OcrEngine.TryCreateFromLanguage(zh) ??
                   OcrEngine.TryCreateFromUserProfileLanguages();
        }
        // Keyboard shortcut: Ctrl+Enter → Save
        private void SaveKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            Save_Click(this, new RoutedEventArgs());
        }

        private async Task PasteIntoAsync(TextBox target)
        {
            try
            {
                var data = Clipboard.GetContent();
                if (data == null) { StatusText.Text = "Clipboard is empty."; return; }

                if (data.Contains(StandardDataFormats.Text))
                {
                    target.Text = (await data.GetTextAsync())?.Trim();
                    StatusText.Text = "Pasted text from clipboard.";
                    return;
                }

                if (data.Contains(StandardDataFormats.Bitmap))
                {
                    var bmp = await data.GetBitmapAsync();
                    if (bmp != null)
                    {
                        using var stream = await bmp.OpenReadAsync();
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        var sb = await decoder.GetSoftwareBitmapAsync();

                        var engine = CreateBestOcrEngine();
                        if (engine == null)
                        {
                            StatusText.Text = "OCR engine not available.";
                            return;
                        }

                        var result = await engine.RecognizeAsync(sb);
                        target.Text = result?.Text?.Trim().Replace(" ", ""); // remove spaces from Hanzi
                        StatusText.Text = string.IsNullOrWhiteSpace(target.Text)
                            ? "OCR ran, but no text found."
                            : "Extracted text from image.";
                        return;
                    }
                }

                StatusText.Text = "Clipboard has no text or image.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Paste/OCR failed: {ex.Message}";
            }
        }

        // ---- Auto translate ----
        private async void HanziTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var hanzi = (HanziTextBox.Text ?? "").Trim();
            if (string.IsNullOrEmpty(hanzi))
            {
                PinyinTextBox.Text = "";
                EnglishTextBox.Text = "";
                return;
            }

            try
            {
                PinyinTextBox.Text = await TranslatorService.TransliterateToPinyinAsync(hanzi);
                EnglishTextBox.Text = await TranslatorService.TranslateToEnglishAsync(hanzi);
                StatusText.Text = "Translation updated.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Translation failed: {ex.Message}";
            }
        }

        // ---- Save ----
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var hanzi = HanziTextBox.Text.Trim();
                var pinyin = PinyinTextBox.Text.Trim();
                var english = EnglishTextBox.Text.Trim();

                if (string.IsNullOrEmpty(hanzi) || string.IsNullOrEmpty(english))
                {
                    StatusText.Text = "Please enter at least Hanzi and English.";
                    return;
                }

                var db = DatabaseService.Context;
                var word = new Word { Hanzi = hanzi, Pinyin = pinyin, English = english };
                db.Words.Add(word);
                db.Cards.Add(new Card { Word = word, Direction = CardDirection.ZhToEn, DueAtUtc = DateTime.UtcNow });
                db.Cards.Add(new Card { Word = word, Direction = CardDirection.EnToZh, DueAtUtc = DateTime.UtcNow });
                await db.SaveChangesAsync();

                ClearForm();
                StatusText.Text = "Saved. You can add another word.";
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Save failed: {ex.Message}";
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e) => ClearForm();

        private void ClearForm()
        {
            HanziTextBox.Text = "";
            PinyinTextBox.Text = "";
            EnglishTextBox.Text = "";
        }
    }
}
