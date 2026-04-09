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
                if (StartUp.UI == null)
                {
                    StartupDialog startupDialog = new StartupDialog();
                    startupDialog.ShowDialog();
                    Start();
                }
                else
                    Start();
            });
        }

        #region Title
        private void initializeTitle()
        {
            DependencyObject titleBar = GetTemplateChild("PART_TitleBar");
        }
        #endregion

        public void Start()
        {
            mainContent.Child = new WebHost();
        }
    }
}