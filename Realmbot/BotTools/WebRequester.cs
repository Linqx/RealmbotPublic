using Rekishi;
using System;
using System.ComponentModel;
using System.Net;

namespace BotTools {
    public class WebRequester {
        public delegate void RequestResponse(string response);

        private readonly WebClient _webClient;

        public WebRequester() {
            _webClient = new WebClient {
                Proxy = null
            };
        }

        /// <summary>
        /// Send an scynronous web request.
        /// </summary>
        /// <param name="fullRequest">Web request with queries</param>
        /// <param name="on_response">Callback with response</param>
        public void SendRequest(string fullRequest, RequestResponse resp) {
            try {
                Log.Debug($"[Web Requester] Sending request: {fullRequest}");
                string response = _webClient.DownloadString(new Uri(fullRequest));
                Log.Debug($"[Web Requester] Request response: {response}");
                resp.Invoke(response);
            }
            catch (Exception e) {
                Log.Error($"[Web Requester] {e}");
            }
        }

        public async void DownloadFileAsync(string fullRequest, string path, Action onComplete = null) {
            try {
                Log.Write($"[Web Requester] Downloading: {fullRequest}");
                _webClient.DownloadFileCompleted += DownloadFileCompleted;
                await _webClient.DownloadFileTaskAsync(new Uri(fullRequest), path);
            }
            catch (Exception e) {
                Log.Error($"[Web Requester] {e}");
            }

            onComplete?.Invoke();
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            _webClient.DownloadFileCompleted -= DownloadFileCompleted;

            if (e.Cancelled)
                Log.Warn("[Web Requester] Download was canceled");
            else if (e.Error != null)
                Log.Error($"[Web Requester] Error downloading file: {e.Error}");
            else
                Log.Write("[Web Requester] Successfully downloaded file.", ConsoleColor.Green);
        }

        public void Dispose() {
            _webClient?.Dispose();
        }
    }
}