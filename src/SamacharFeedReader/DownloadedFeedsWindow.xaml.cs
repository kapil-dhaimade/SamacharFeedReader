using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SamacharFeedReader
{
    /// <summary>
    /// Interaction logic for DownloadedFeedsWindow.xaml
    /// </summary>
    public partial class DownloadedFeedsWindow : Window
    {
        public DownloadedFeedsWindow(DownloadedFeeds downloadedFeeds)
        {
            InitializeComponent();
            DataContext = downloadedFeeds;
        }

        private void ContentControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(
                ((DownloadedFeedItem)((ContentControl)sender).DataContext).Link);
        }

        public void showActivated()
        {
            if (Visibility != Visibility.Visible)
            {
                Show();
            }
            else
            {
                Activate();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //NOTE: Once a window is closed, you cannot show it again. Hence, we need to hide it 
            //on close.
            e.Cancel = true;
            Visibility = Visibility.Hidden;
            ((DownloadedFeeds)DataContext).switchNewEntriesToOld();
            ((DownloadedFeeds)DataContext).Serialize();
        }
    }

    [ValueConversion(typeof(DateTime), typeof(String))]
    public class DateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - ((DateTime)value).Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
                return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";

            if (delta < 2 * MINUTE)
                return "a minute ago";

            if (delta < 45 * MINUTE)
                return ts.Minutes + " minutes ago";

            if (delta < 90 * MINUTE)
                return "an hour ago";

            if (delta < 24 * HOUR)
                return ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                return "yesterday";

            if (delta < 30 * DAY)
                return ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = (int)Math.Floor((double)ts.Days / 30);
                return months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = (int)(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception();
        }
    }

    [ValueConversion(typeof(bool), typeof(String))]
    public class NewEntryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) == true ? "NEW" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception();
        }
    }
}
