// Services/TranslatorService.cs
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HuaCards.Services
{
    /// <summary>
    /// Helper for Microsoft Azure Cognitive Services Translator (v3) usage.
    /// Reads subscription key and region from environment variables by default.
    /// </summary>
    public static class TranslatorService
    {
        // Preferred: store your key and region in environment variables.
        // e.g. set AZURE_TRANSLATOR_KEY and AZURE_TRANSLATOR_REGION in Windows env.
        private static readonly string Key = Environment.GetEnvironmentVariable("1FtsOiiYRyiXDrribImHGSqv8zjxcMTxaSXFfnBfFAdU2aYKbjV8JQQJ99BHAC3pKaRXJ3w3AAAbACOGy19a") ?? string.Empty;
        private static readonly string Region = Environment.GetEnvironmentVariable("eastasia") ?? string.Empty;
        // If you prefer to hard-code for a quick test, replace the above with:
        // private static readonly string Key = "YOUR_KEY_HERE";
        // private static readonly string Region = "eastasia";

        // Endpoint for the Translator service. Change if using a custom endpoint.
        private static readonly string Endpoint = "https://api.cognitive.microsofttranslator.com";

        // Shared HttpClient instance
        private static readonly HttpClient _http = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var http = new HttpClient();
            if (!string.IsNullOrWhiteSpace(Key))
            {
                // required header for the Translator resource
                // If you use a global Cognitive Services resource with key/region,
                // the header names are the same.
                http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Key);
            }

            if (!string.IsNullOrWhiteSpace(Region))
            {
                // For multi-service cognitive accounts you might need the region header
                http.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Region", Region);
            }

            // Small default timeout (adjust if needed)
            http.Timeout = TimeSpan.FromSeconds(30);
            return http;
        }

        /// <summary>
        /// Convert Hanzi (zh-Hans) to Pinyin using the transliterate endpoint.
        /// Returns a plain pinyin string (or empty string on error).
        /// </summary>
        public static async Task<string> TransliterateToPinyinAsync(string hanzi)
        {
            if (string.IsNullOrWhiteSpace(hanzi))
                return string.Empty;

            // Request parameters: language=zh-Hans and convert from Hans -> Latn (pinyin)
            var route = "/transliterate?api-version=3.0&language=zh-Hans&fromScript=Hans&toScript=Latn";

            var body = new object[] { new { Text = hanzi } };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var resp = await _http.PostAsync(Endpoint + route, content).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

            // Response: [ { "text": "pīnyīn …", "script": "Latn" } ]
            try
            {
                var root = doc.RootElement;
                if (root.GetArrayLength() > 0)
                {
                    var text = root[0].GetProperty("text").GetString();
                    return text ?? string.Empty;
                }
            }
            catch { /* fallthrough */ }

            return string.Empty;
        }

        /// <summary>
        /// Translate simplified Chinese (zh-Hans) to English (en).
        /// Returns the translated English text or throws on network/error.
        /// </summary>
        public static async Task<string> TranslateToEnglishAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var route = "/translate?api-version=3.0&from=zh-Hans&to=en";
            var body = new object[] { new { Text = text } };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var resp = await _http.PostAsync(Endpoint + route, content).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

            // Response: [ { "translations": [ { "text": "…", "to": "en" } ] } ]
            try
            {
                var translations = doc.RootElement[0].GetProperty("translations");
                if (translations.GetArrayLength() > 0)
                    return translations[0].GetProperty("text").GetString() ?? string.Empty;
            }
            catch { /* fallthrough */ }

            return string.Empty;
        }

        /// <summary>
        /// Translate English (en) to simplified Chinese (zh-Hans).
        /// Returns the translated Chinese (Hanzi) string.
        /// </summary>
        public static async Task<string> TranslateToChineseAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var route = "/translate?api-version=3.0&from=en&to=zh-Hans";
            var body = new object[] { new { Text = text } };
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            using var resp = await _http.PostAsync(Endpoint + route, content).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);

            try
            {
                var translations = doc.RootElement[0].GetProperty("translations");
                if (translations.GetArrayLength() > 0)
                    return translations[0].GetProperty("text").GetString() ?? string.Empty;
            }
            catch { /* fallthrough */ }

            return string.Empty;
        }
    }
}
