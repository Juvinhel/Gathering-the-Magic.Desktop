using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Celestial;
using Celestial.Components;
using Gathering_the_Magic.Desktop.UI;
using Lemon.Text.Matching;
using Microsoft.Web.WebView2.Wpf;

namespace Gathering_the_Magic.Desktop.Data
{
    static public class SaveLoad
    {
        static private WebView2 webView
        {
            get { return MainWindow.Current.FindChild<WebView2>(); }
        }

        static private SaveFileDialog saveFileDialog = new SaveFileDialog()
        {
            Filters =
            {
                new FilterViewModel("IDEC Deck", "IDEC Deck format", new WildcardMatcher("*.idec")),
                new FilterViewModel("YAML Deck", "YAML Deck format", new WildcardMatcher("*.yaml") | new WildcardMatcher("*.yml")),
                new FilterViewModel("JSON Deck", "JSON Deck format", new WildcardMatcher("*.json")),
                new FilterViewModel("DEC Deck", "DEC Deck format", new WildcardMatcher("*.dec")),
                new FilterViewModel("TXT Deck", "TXT Deck format", new WildcardMatcher("*.txt")),
                new FilterViewModel("COD Deck", "Cockatrice Deck format", new WildcardMatcher("*.cod")),
            }
        };
        static public string SaveDeck()
        {
            string repositoryFolderPath = string.IsNullOrWhiteSpace(Config.Current.RepositoryFolderPath) ? Directory.Current : Config.Current.RepositoryFolderPath;
            saveFileDialog.InitialFolderPath = repositoryFolderPath;

            string result = null;
            if(webView != null) webView.Visibility = Visibility.Hidden;
            if (!string.IsNullOrEmpty(saveFileDialog.ShowDialog()))
            {
                result = saveFileDialog.SelectedFilePath;
                if (saveFileDialog.SelectedFilter.Name == "*" && !result.ToLower().EndsWithAny(".idec", ".yaml", ".yml", ".json", ".dec", ".txt", ".cod")) result += ".idec";
                if (saveFileDialog.SelectedFilter.Name == "IDEC Deck" && !result.ToLower().EndsWithAny(".idec")) result += ".idec";
                if (saveFileDialog.SelectedFilter.Name == "YAML Deck" && !result.ToLower().EndsWithAny(".yaml", ".yml")) result += ".yaml";
                if (saveFileDialog.SelectedFilter.Name == "JSON Deck" && !result.ToLower().EndsWithAny(".json")) result += ".json";
                if (saveFileDialog.SelectedFilter.Name == "DEC Deck" && !result.ToLower().EndsWithAny(".dec")) result += ".dec";
                if (saveFileDialog.SelectedFilter.Name == "TXT Deck" && !result.ToLower().EndsWithAny(".txt")) result += ".txt";
                if (saveFileDialog.SelectedFilter.Name == "COD Deck" && !result.ToLower().EndsWithAny(".cod")) result += ".cod";
            }
            if (webView != null) webView.Visibility = Visibility.Visible;

            return result;
        }

        static private OpenFileDialog openDeckFileDialog = new OpenFileDialog()
        {
            Filters = {
                new FilterViewModel("IDEC Deck", "IDEC Deck format", new WildcardMatcher("*.idec")),
                new FilterViewModel("YAML Deck", "YAML Deck format", new WildcardMatcher("*.yaml") | new WildcardMatcher("*.yml")),
                new FilterViewModel("JSON Deck", "JSON Deck format", new WildcardMatcher("*.json")),
                new FilterViewModel("DEC Deck", "DEC Deck format", new WildcardMatcher("*.dec")),
                new FilterViewModel("TXT Deck", "TXT Deck format", new WildcardMatcher("*.txt")),
                new FilterViewModel("COD Deck", "Cockatrice Deck format", new WildcardMatcher("*.cod")),
            },
        };
        static public string LoadDeck()
        {
            string repositoryFolderPath = string.IsNullOrWhiteSpace(Config.Current.RepositoryFolderPath) ? Directory.Current : Config.Current.RepositoryFolderPath;
            openDeckFileDialog.InitialFolderPath = repositoryFolderPath;

            string result = default;
            if (webView != null) webView.Visibility = Visibility.Hidden;
            if (!string.IsNullOrEmpty(openDeckFileDialog.ShowDialog()))
            {
                string filePath = openDeckFileDialog.SelectedFilePath;
                result = openDeckFileDialog.SelectedFilePath;
            }
            if (webView != null) webView.Visibility = Visibility.Visible;

            return result;
        }

        static private OpenMultipleFilesDialog openCollectionFileDialog = new OpenMultipleFilesDialog()
        {
            Filters = {
                new FilterViewModel("CSV Collection", "CSV Collection File Format", new WildcardMatcher("*.csv"), true),
            },
        };
        static public IEnumerable<string> LoadCollections()
        {
            string repositoryFolderPath = string.IsNullOrWhiteSpace(Config.Current.RepositoryFolderPath) ? Directory.Current : Config.Current.RepositoryFolderPath;
            openCollectionFileDialog.InitialFolderPath = repositoryFolderPath;
            List<string> result = default;
            if (webView != null) webView.Visibility = Visibility.Hidden;
            if (!string.IsNullOrEmpty(openCollectionFileDialog.ShowDialog()))
            {
                result = openCollectionFileDialog.SelectedFilePaths.ToList();
            }
            if (webView != null) webView.Visibility = Visibility.Visible;

            return result;
        }
    }
}
