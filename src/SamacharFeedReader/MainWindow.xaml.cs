using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Win32;

namespace SamacharFeedReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Task mFeedReadTask;
        private Task mFeedCleanupTask;
        CancellationTokenSource quitTokenSrc = new CancellationTokenSource();
        FeedFetcher feedFetchTask = null;
        int millisecDelayForNextFeedRead;
        int millisecDelayForNextFeedCleanup = 60 * 60 * 1000;
        DownloadedFeeds downloadedFeeds = null;
        public DownloadedFeedsWindow downloadedFeedsWnd = null;
        public MainWindow()
        {
            //NOTE: Needed because working dir is system32 when launched as startup item.
            SetWorkingDirectory();

            //NOTE: Can't do this in member declaration list but only after SetWorkingDirectory() call 
            //because correct feeds serialized file should be found.
            downloadedFeeds = DownloadedFeeds.DeSerialize();

            InitializeComponent();

            setAsStartupItemIfUserChooses();

            downloadedFeedsWnd = new DownloadedFeedsWindow(downloadedFeeds);
            feedFetchTask = new FeedFetcher(downloadedFeeds, downloadedFeedsWnd);
            millisecDelayForNextFeedRead = 
                int.Parse(ConfigurationManager.AppSettings["feedFetchIntervalMins"] ?? (15 * 60 * 1000).ToString());
            if(ConfigurationManager.AppSettings["feedFetchIntervalMins"] == null)
            {
                AddOrUpdateAppSettings("feedFetchIntervalMins",
                    (millisecDelayForNextFeedRead / 60000).ToString());
            }

            //SubscribedFeeds obj = SubscribedFeeds.DeSerialize();
            //obj.FeedList.Add(new RegisteredFeed { Name = "a", Link = "b" });
            //obj.Serialize();
            //Feeds obj = new Feeds();
            //obj.FeedLinkVsNameList.Add(new Feed { Link = "a", Name = "b" });
            //obj.Serialize();

            mFeedReadTask = Task.Run(async () =>
            {
                while (quitTokenSrc.Token.IsCancellationRequested == false)
                {
                    await feedFetchTask.run();
                    await Task.Delay(millisecDelayForNextFeedRead, quitTokenSrc.Token);
                }
            }, quitTokenSrc.Token);

            mFeedCleanupTask = Task.Run(async () =>
            {
                while (quitTokenSrc.Token.IsCancellationRequested == false)
                {
                    await downloadedFeeds.cleanupFeeds();
                    downloadedFeeds.Serialize();
                    await Task.Delay(millisecDelayForNextFeedCleanup, quitTokenSrc.Token);
                }
            }, quitTokenSrc.Token);

            //during init of your application bind to this event  
            SystemEvents.SessionEnding +=
               new SessionEndingEventHandler(SystemEvents_SessionEnding);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            quitTokenSrc.Cancel();
            //NOTE: Try-catch needed because wait on canceled task throws exception.
            try
            {
                mFeedReadTask.Wait();
            }
            catch (Exception)
            {
            }
            try
            {
                mFeedCleanupTask.Wait();
            }
            catch (Exception)
            {
            }
            myNotifyIcon.Dispose();
            base.OnClosing(e);
        }

        private void AddOrUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings");
            }
        }

        private void ShowAll_Clicked(object sender, RoutedEventArgs e)
        {
            downloadedFeedsWnd.showActivated();
        }

        private void ManageFeeds_Clicked(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("subscribedFeeds.txt");
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            new Commands.QuitCommand().Execute(null);
        }

        private void setAsStartupItemIfUserChooses()
        {
            if(bool.Parse((ConfigurationManager.AppSettings["startupItemPromptShown"]) ?? "false") == true)
            {
                return;
            }
            AddOrUpdateAppSettings("startupItemPromptShown", true.ToString());

            var result = MessageBox.Show("Automatically run Samachar Feed Reader on machine startup?",
                "Samachar Feed Reader",
                MessageBoxButton.YesNo);
            if(result == MessageBoxResult.No)
            {
                return;
            }
            try
            {
                // Get the name and path of the current process
                string name = Assembly.GetEntryAssembly().GetName().Name;
                string path = Assembly.GetEntryAssembly().Location;

                // Add the startup item to the Registry
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(
                    "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                {
                    key.SetValue(name, path);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Sorry. Failed to set Samachar Feed Reader to automatically run on machine startup.");
            }
        }

        private void SetWorkingDirectory()
        {
            var appPath = Assembly.GetEntryAssembly().Location;
            var fileInfo = new FileInfo(Assembly.GetEntryAssembly().Location);
            Directory.SetCurrentDirectory(fileInfo.Directory.FullName);
        }
    }
}
