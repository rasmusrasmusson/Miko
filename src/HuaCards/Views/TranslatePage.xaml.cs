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
using HuaCards.Services;

namespace HuaCards.Views
{
    public sealed partial class TranslatePage : Page
    {
        // false = Chinese -> English (default)
        // true  = English -> Chinese
        private bool _toChinese = false;

        public TranslatePage()
        {
            InitializeComponent();
            UpdateLayoutForMode();
        }

        private void UpdateLayoutForMode()
        {
            InputHeader.Text = _toChinese ? "English" : "Chinese";
            OutputHeader.Text = _toChinese ? "Chinese" : "English";

            OutputEnglish.Visibility = _toChinese ? Visibility.Collapsed : Visibility.Visible;
            ZhTargetPanel.Visibility = _toChinese ? Visibility.Visible : Visibility.Collapsed;

            // clear outputs when flipping
            OutputEnglish.Text = string.Empty;
            OutputHanzi.Text = string.Empty;
            OutputPinyin.Text = string.Empty;
            StatusText.Text = string.Empty;
        }

        private void SwapButton_Click(object sender, RoutedEventArgs e)
        {
            _toChinese = !_toChinese;
            UpdateLayoutForMode();
        }

        private void SwapKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            SwapButton_Click(this, new RoutedEventArgs());
        }

        // ---- paste (button + Ctrl+V) ----
        private async void PasteButton_Click(object sender, RoutedEventArgs e) => await PasteIntoAsync(InputText);

        private async void PasteKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            await PasteIntoAsync(InputText);
        }

        private static OcrEngine CreateBestOcrEngine()
        {
            var zh = new Language("zh-Hans");
            return OcrEngine.TryCreateFromLanguage(zh) ??
                   OcrEngine.TryCreateFromUserProfileLanguages();
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

        // ---- translate (button + Ctrl+Enter) ----
        private async void TranslateButton_Click(object sender, RoutedEventArgs e) => await TranslateAsync();

        private void TranslateKbd_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            TranslateButton_Click(this, new RoutedEventArgs()); // keep signatures consistent. :contentReference[oaicite:4]{index=4}
        }

        private async Task TranslateAsync()
        {
            try
            {
                var input = (InputText.Text ?? "").Trim();
                if (string.IsNullOrEmpty(input))
                {
                    StatusText.Text = "Type or paste some text first.";
                    return;
                }

                if (_toChinese)
                {
                    var hanzi = await TranslatorService.TranslateToChineseAsync(input);
                    OutputHanzi.Text = hanzi;
                    OutputPinyin.Text = await TranslatorService.TransliterateToPinyinAsync(hanzi);
                }
                else
                {
                    OutputEnglish.Text = await TranslatorService.TranslateToEnglishAsync(input);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Translate failed: {ex.Message}";
            }
        }
    }
}
