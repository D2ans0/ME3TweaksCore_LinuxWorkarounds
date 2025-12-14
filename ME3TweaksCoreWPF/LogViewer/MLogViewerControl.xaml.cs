using LegendaryExplorerCore.Helpers;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ME3TweaksCoreWPF.LogViewer
{
    public partial class MLogViewerControl : UserControl
    {
        /// <summary>
        /// The text of the log/diagnostic to display
        /// </summary>
        private string logText;

        /// <summary>
        /// Local log viewer URI
        /// </summary>
        private const string LOG_VIEW_LOCAL_URI = @"https://me3tweaks.com/modmanager/logservice/logviewer.html?manualLoad=true";

        public MLogViewerControl(string logText)
        {
            InitializeComponent();
            this.logText = logText;
            InitializeAsync();

        }

        async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.NavigationStarting += CoreWebView2_NavigationStarting;
            webView.CoreWebView2.SetVirtualHostNameToFolderMapping("me3tweaks.com", "C:\\Users\\mgame\\Desktop\\log", CoreWebView2HostResourceAccessKind.Allow);
            string script = await File.ReadAllTextAsync(@"C:\Users\mgame\Desktop\log\modmanager\logservice\shared\jquery-3.5.0.min.js");
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
            webView.CoreWebView2.Navigate(LOG_VIEW_LOCAL_URI);
         


        }

        private async void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // This event fires when the page is being reloaded or navigated
            // You can check if it's a reload by comparing the URI or checking navigation kind
            // Add your logic here to handle the reload
            // await webView.ExecuteScriptAsync($@"handleLoadedLog(""{logText}"")");
            if (e.Uri == LOG_VIEW_LOCAL_URI)
            {
                await Task.Run(async () =>
                {
                    Thread.Sleep(1000);
                }).ContinueWithOnUIThread(async x =>
                {
                    var base64Log = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(logText));
                    var result = await webView.ExecuteScriptAsync($@"handleBase64Log(""{base64Log}"");");
                    Debug.WriteLine(result);
                });
            }
        }
    }
}