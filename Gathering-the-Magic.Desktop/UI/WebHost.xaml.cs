using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Celestial.Components;
using Gathering_the_Magic.Desktop.Data;
using Lemon;
using Lemon.Text.Matching;
using Microsoft.Web.WebView2.Core;

namespace Gathering_the_Magic.Desktop.UI
{
    /// <summary>
    /// Interaktionslogik für WebHost.xaml
    /// </summary>
    public partial class WebHost
    {
        public WebHost()
        {
            Current = this;
            InitializeComponent();
        }

        static public WebHost Current { get; private set; }

        private void webHost_Loaded(object _sender, RoutedEventArgs _e)
        {
            initWebView();
        }

        private CoreWebView2Environment cwv2Environment;
        private async Task initWebView()
        {
            string cacheFolderPath = Program.MyFolderPath;
            if (cwv2Environment == null)
            {
                CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions();
                cwv2Environment = await CoreWebView2Environment.CreateAsync(null, cacheFolderPath, options);
            }
            await webView.EnsureCoreWebView2Async(cwv2Environment);

            webView.CoreWebView2.NewWindowRequested += coreWebView2_NewWindowRequested;
            webView.CoreWebView2.AddHostObjectToScript("bridge", new Bridge());

            string page = "index.html";
            switch (StartUp.UI)
            {
                case UIMode.All:
                default:
                    page = "index.html";
                    break;
                case UIMode.Workbench:
                    page = "workbench.html";
                    break;
                case UIMode.Library:
                    page = "library.html";
                    break;
                case UIMode.Shelf:
                    page = "shelf.html";
                    break;
            }

            if (Debugger.IsAttached)
            {
                webView.CoreWebView2.AddWebResourceRequestedFilter("http://localhost:5414/repository/*", CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
                webView.CoreWebView2.WebResourceRequested += coreWebView2_WebResourceRequested;
                webView.Source = new Uri("http://localhost:5414/" + page);
            }
            else
            {
                webView.CoreWebView2.AddWebResourceRequestedFilter(virtualHost + "/*", CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
                webView.CoreWebView2.WebResourceRequested += coreWebView2_WebResourceRequested;
                webView.Source = new Uri(virtualHost + "/" + page);
            }
        }

        private void coreWebView2_NewWindowRequested(object _sender, CoreWebView2NewWindowRequestedEventArgs _e)
        {
            _e.Handled = true;
            Process.Start(new ProcessStartInfo
            {
                FileName = _e.Uri,
                UseShellExecute = true
            });
        }

        private string virtualHost = "https://web.example";
        private void coreWebView2_WebResourceRequested(object _sender, CoreWebView2WebResourceRequestedEventArgs _e)
        {
            CoreWebView2Deferral deferral = _e.GetDeferral();
            Url url = new Url(_e.Request.Uri);
            Task.Run(() =>
            {
                (Stream file, string mimeType) = resourceRequested(url);

                if (file != null && mimeType != null)
                    Dispatcher.Invoke(() =>
                    {
                        CoreWebView2WebResourceResponse response = webView.CoreWebView2.Environment.CreateWebResourceResponse(file, 200, "OK", "Content-Type: " + mimeType);
                        _e.Response = response;
                    });
                else
                    Dispatcher.Invoke(() =>
                    {
                        CoreWebView2WebResourceResponse response = webView.CoreWebView2.Environment.CreateWebResourceResponse(new MemoryStream(), 404, "NOT FOUND", "Content-Type: application/octet-stream");
                        _e.Response = response;
                    });

                deferral.Complete();
            });
        }

        private (Stream file, string mimeType) resourceRequested(Url _url)
        {
            if (_url.Path.StartsWith("repository"))
                return interceptRepository(_url);

            string decodedPath = WebUtility.UrlDecode(_url.Path);
            string localPath = Path.MakeRooted(Path.Combine(StartUp.WebFolderPath, decodedPath));
            if (!localPath.StartsWith(StartUp.WebFolderPath)) return (null, null);

            if (File.Exists(localPath))
            {
                string extension = Path.GetExtension(localPath);
                MemoryStream mem = new MemoryStream();
                using (FileStream fs = File.OpenRead(localPath))
                    fs.CopyTo(mem);
                mem.Position = 0;
                string mimeType = MimeTypes.MimeTypeMap.GetMimeType(extension.ToLower().TrimStart("."));

                return (mem, mimeType);
            }

            return (null, null);
        }

        private (Stream file, string mimeType) interceptRepository(Url _url)
        {
            string path = _url.Path;
            string virtualPath = path.Substring("repository".Length).TrimStart("/");
            string decodedPath = WebUtility.UrlDecode(virtualPath);
            if (decodedPath.Split("/").Any(x => x.StartsWith("."))) return (null, null); // prevent access to parent folders and hidden folders

            string localPath = Path.Combine(Config.Current.RepositoryFolderPath, decodedPath);
            string fileName = Path.GetFileName(localPath);
            if (Directory.Exists(localPath))
            {
                string html = createDirectoryListing(localPath);
                MemoryStream mem = new MemoryStream();
                using (StreamWriter sw = new StreamWriter(mem, leaveOpen: true))
                    sw.Write(html);
                mem.Position = 0;

                return (mem, "text/html");
            }
            else if (File.Exists(localPath))
            {
                string extension = Path.GetExtension(localPath);
                MemoryStream mem = new MemoryStream();
                using (FileStream fs = File.OpenRead(localPath))
                    fs.CopyTo(mem);
                mem.Position = 0;
                string mimeType = MimeTypes.MimeTypeMap.GetMimeType(extension.ToLower().TrimStart("."));

                return (mem, mimeType);
            }
            else
                return (null, null);
        }

        private string createDirectoryListing(string _folderPath)
        {
            Helper.DirectoryListing directoryListing = new Helper.DirectoryListing();
            directoryListing.BaseUrl = "/repository";
            directoryListing.RootFolderPath = Config.Current.RepositoryFolderPath;
            directoryListing.FolderPath = _folderPath;
            return directoryListing.TransformText();
        }
    }
}
