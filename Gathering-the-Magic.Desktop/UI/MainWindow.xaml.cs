using System.Diagnostics;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using Celestial;
using Celestial.Components;
using Gathering_the_Magic.DeckEdit.Data;
using Lemon;
using Lemon.Text.Matching;
using Lemon.Threading;
using Microsoft.Web.WebView2.Core;

namespace Gathering_the_Magic.DeckEdit.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Current = this;
            InitializeComponent();
        }

        static public MainWindow Current { get; private set; }

        private void mainWindow_Loaded(object _sender, RoutedEventArgs _e)
        {
            initializeTitle();

            Delay.Start(10, () =>
            {
                if(StartUp.UI == null)
                {
                    StartupDialog startupDialog = new StartupDialog();
                    startupDialog.Show();
                }
                else
                    MainWindow.Current.Start();
            });
        }

        #region Title
        private TextBlock openFileTextBlock;
        private void initializeTitle()
        {
            DependencyObject titleBar = GetTemplateChild("PART_TitleBar");
            openFileTextBlock = titleBar.FindChild<TextBlock>(x => x.Name == "openFileTextBlock");
        }
        #endregion

        public void Start()
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
            }

            if (Debugger.IsAttached)
                webView.Source = new Uri("http://localhost:5414/" + page);
            else
            {
                webView.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
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
            string uri = _e.Request.Uri;
            if (uri.StartsWith(virtualHost))
            {
                string path = uri.Substring(virtualHost.Length);
                string decodedPath = WebUtility.UrlDecode(path);

                string fullPath = Path.MakeRooted(Path.Combine(StartUp.WebFolderPath, decodedPath));
                if (!fullPath.StartsWith(StartUp.WebFolderPath)) return;

                CoreWebView2WebResourceContext resourceContext = _e.ResourceContext;
                if (File.Exists(fullPath))
                {
                    string extension = Path.GetExtension(fullPath);
                    FileStream fs = File.OpenRead(fullPath);
                    string mimeType = MimeTypes.MimeTypeMap.GetMimeType(extension.ToLower().TrimStart("."));
                    CoreWebView2WebResourceResponse response = webView.CoreWebView2.Environment.CreateWebResourceResponse(fs, 200, "OK", "Content-Type: " + mimeType);
                    _e.Response = response;
                }
            }
        }

        private SaveFileDialog saveFileDialog = new SaveFileDialog()
        {
            Filters =
            {
                new FilterViewModel("YAML Deck", "YAML Deck format", new WildcardMatcher("*.yaml") | new WildcardMatcher("*.yml"), true),
                new FilterViewModel("JSON Deck", "JSON Deck format", new WildcardMatcher("*.json")),
                new FilterViewModel("DEC Deck", "DEC Deck format", new WildcardMatcher("*.dec")),
                new FilterViewModel("TXT Deck", "TXT Deck format", new WildcardMatcher("*.txt")),
                new FilterViewModel("COD Deck", "Cockatrice Deck format", new WildcardMatcher("*.cod")),
            }
        };
        public string SaveDeck()
        {
            if (!string.IsNullOrEmpty(Config.Current.RepositoryFolderPath))
                saveFileDialog.InitialFolderPath = Config.Current.RepositoryFolderPath;

            string result = null;
            webView.Visibility = Visibility.Hidden;
            if (!string.IsNullOrEmpty(saveFileDialog.ShowDialog()))
            {
                result = saveFileDialog.SelectedFilePath;
                if (saveFileDialog.SelectedFilter.Name == "*" && !result.ToLower().EndsWithAny(".yaml", ".yml")) result += ".yaml";
                if (saveFileDialog.SelectedFilter.Name == "YAML Deck" && !result.ToLower().EndsWithAny(".yaml", ".yml")) result += ".yaml";
                if (saveFileDialog.SelectedFilter.Name == "JSON Deck" && !result.ToLower().EndsWithAny(".json")) result += ".json";
                if (saveFileDialog.SelectedFilter.Name == "DEC Deck" && !result.ToLower().EndsWithAny(".dec")) result += ".dec";
                if (saveFileDialog.SelectedFilter.Name == "TXT Deck" && !result.ToLower().EndsWithAny(".txt")) result += ".txt";
                if (saveFileDialog.SelectedFilter.Name == "COD Deck" && !result.ToLower().EndsWithAny(".cod")) result += ".cod";
            }
            webView.Visibility = Visibility.Visible;

            return result;
        }

        private OpenFileDialog openDeckFileDialog = new OpenFileDialog()
        {
            Filters = {
                new FilterViewModel("YAML Deck", "YAML Deck format", new WildcardMatcher("*.yaml") | new WildcardMatcher("*.yml"), true),
                new FilterViewModel("JSON Deck", "JSON Deck format", new WildcardMatcher("*.json")),
                new FilterViewModel("DEC Deck", "DEC Deck format", new WildcardMatcher("*.dec")),
                new FilterViewModel("TXT Deck", "TXT Deck format", new WildcardMatcher("*.txt")),
                new FilterViewModel("COD Deck", "Cockatrice Deck format", new WildcardMatcher("*.cod")),
            },
        };
        public string LoadDeck()
        {
            if (!string.IsNullOrEmpty(Config.Current.RepositoryFolderPath))
                openDeckFileDialog.InitialFolderPath = Config.Current.RepositoryFolderPath;

            string result = default;
            webView.Visibility = Visibility.Hidden;
            if (!string.IsNullOrEmpty(openDeckFileDialog.ShowDialog()))
            {
                string filePath = openDeckFileDialog.SelectedFilePath;
                result = openDeckFileDialog.SelectedFilePath;
            }
            webView.Visibility = Visibility.Visible;

            return result;
        }

        private OpenMultipleFilesDialog openCollectionFileDialog = new OpenMultipleFilesDialog()
        {
            Filters = {
                new FilterViewModel("CSV Collection", "CSV Collection File Format", new WildcardMatcher("*.csv"), true),
            },
        };
        public IEnumerable<string> LoadCollections()
        {
            if (!string.IsNullOrEmpty(Config.Current.RepositoryFolderPath))
                openCollectionFileDialog.InitialFolderPath = Config.Current.RepositoryFolderPath;
            List<string> result = default;
            webView.Visibility = Visibility.Hidden;
            if (!string.IsNullOrEmpty(openCollectionFileDialog.ShowDialog()))
            {
                result = openCollectionFileDialog.SelectedFilePaths.ToList();
            }
            webView.Visibility = Visibility.Visible;

            return result;
        }
    }
}