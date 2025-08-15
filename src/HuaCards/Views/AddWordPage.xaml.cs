using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using Windows.Globalization;

namespace HuaCards.Views
{
    public sealed partial class AddWordPage : Page
    {
        public AddWordPage()
        {
            InitializeComponent();
        }

        // ---- paste (button + Ctrl+V)
        private async void PasteButton_Click(object sender, RoutedEventArgs e) => await PasteFromClipboardIntoAsync(HanziTextBox);

        private async void PasteKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            await PasteFromClipboardIntoAsync(HanziTextBox);
        }

        private static OcrEngine CreateBestOcrEngine()
        {
            // Prefer Simplified Chinese when available, else fallback to user languages.
            var zh = new Language("zh-Hans");
            return OcrEngine.TryCreateFromLanguage(zh) ??
                   OcrEngine.TryCreateFromUserProfileLanguages();
        }

        private async Task PasteFromClipboardIntoAsync(TextBox target)
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
                        using IRandomAccessStream stream = await bmp.OpenReadAsync();
                        var decoder = await BitmapDecoder.CreateAsync(stream);
                        var sb = await decoder.GetSoftwareBitmapAsync();

                        var engine = CreateBestOcrEngine();
                        if (engine == null)
                        {
                            StatusText.Text = "OCR engine not available. Install Chinese (Simplified) OCR in Windows language features.";
                            return;
                        }

                        var result = await engine.RecognizeAsync(sb);
                        target.Text = result?.Text?.Trim();
                        StatusText.Text = string.IsNullOrWhiteSpace(target.Text)
                            ? "OCR ran, but no readable text was found."
                            : "Extracted text from image (OCR).";
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

        // ---- Save (button + Ctrl+Enter)
        private void SaveKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            // IMPORTANT: handler expects RoutedEventArgs, not KeyboardAcceleratorInvokedEventArgs.
            Save_Click(this, new RoutedEventArgs());   // ← fixes CS1503 runtime path. :contentReference[oaicite:2]{index=2}
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // TODO: put your DB insert here
            StatusText.Text = "Saved.";
        }
    }
}
