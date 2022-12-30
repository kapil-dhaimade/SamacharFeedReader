using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
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
        DownloadedFeeds downloadedFeeds = DownloadedFeeds.DeSerialize();
        public DownloadedFeedsWindow downloadedFeedsWnd = null;
        public MainWindow()
        {
            InitializeComponent();
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
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            quitTokenSrc.Cancel();
            mFeedReadTask.Wait();
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
    }
}
