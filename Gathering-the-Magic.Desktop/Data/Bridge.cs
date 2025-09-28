using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Gathering_the_Magic.DeckEdit.UI;
using WinCopies.Util;

namespace Gathering_the_Magic.DeckEdit.Data
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    sealed public class Bridge
    {
        public string ShowSaveDeck()
        {
            string filePath = MainWindow.Current.SaveDeck();
            filePath = filePath?.Replace("\\", "/");
            return filePath;
        }

        public string ShowOpenDeck()
        {
            string filePath = MainWindow.Current.LoadDeck();
            filePath = filePath?.Replace("\\", "/");
            return filePath;
        }

        public void DoSaveDeck(string _filePath, string _text)
        {
            _filePath = _filePath.Replace("/", "\\");
            File.WriteAllText(_filePath, _text);
        }

        public string DoOpenDeck(string _filePath)
        {
            _filePath = _filePath.Replace("/", "\\");
            return File.ReadAllText(_filePath);
        }

        //TODO: refactor
        public LoadResult[] LoadCollections()
        {
            IEnumerable<string> filePaths = MainWindow.Current.LoadCollections();
            if (filePaths == null) return null;
            return filePaths.Select(filePath => new LoadResult(filePath)).ToArray();
        }

        public LoadResult[] LoadDefaultCollections()
        {
            string collectionsFolderPath = Path.Combine(Config.Current.RepositoryFolderPath, "Collections");
            if (!Directory.Exists(collectionsFolderPath)) return null;

            IEnumerable<string> filePaths = Directory.GetFiles(collectionsFolderPath, "csv");
            return filePaths.Select(filePath => new LoadResult(filePath)).ToArray();
        }

        private string configFilePath = Path.Combine(Directory.Current, "web.config.user");
        public string LoadConfig()
        {
            return File.Exists(configFilePath) ? File.ReadAllText(configFilePath) : null;
        }

        public void SaveConfig(string _text)
        {
            File.WriteAllText(configFilePath, _text);
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class SaveResult
    {
        public SaveResult(string _filePath)
        {
            fullPath = _filePath;
            Name = Path.GetFileNameWithoutExtension(fullPath);
            Extension = Path.GetExtension(fullPath).TrimStart(".");
            Exists = File.Exists(fullPath);
        }

        private string fullPath;
        public string FilePath { get { return fullPath; } }
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public bool Exists { get; private set; }

        public void Save(string _text)
        {
            File.WriteAllText(fullPath, _text);
        }
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class LoadResult
    {
        public LoadResult(string _filePath)
        {
            fullPath = _filePath;
            Name = Path.GetFileNameWithoutExtension(fullPath);
            Extension = Path.GetExtension(fullPath).TrimStart(".").ToLower();
            LastModified = File.GetInfo(fullPath).LastWriteTimeUtc.ToString("o");
        }

        private string fullPath;
        public string FilePath { get { return fullPath; } }
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public string LastModified { get; private set; }

        public string Load()
        {
            return File.ReadAllText(fullPath);
        }
    }
}