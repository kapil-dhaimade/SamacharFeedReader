using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SamacharFeedReader
{
    public class DownloadedFeedItem
    {
        public string FeedName
        {
            get; set;
        }
        public string FeedSrcLink
        {
            get; set;
        }
        public string Link
        {
            get; set;
        }

        public string Title
        {
            get; set;
        }

        public DateTime PublishedDate
        {
            get; set;
        }

        public bool NewEntry
        {
            get; set;
        }
    }

    public class DownloadedFeeds : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int numOfDaysOldEntriesToCleanup = 30;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task cleanupFeeds()
        {
            //NOTE: Remove items whose feed sources have been removed from subscribed feeds list.
            var subscribedFeeds = SubscribedFeeds.DeSerialize();
            var feedSrcLinksToKeep = subscribedFeeds.FeedList.Select(x => x.Link).Distinct();
            var keepList = DownloadedFeedItemList.
                Where(x => feedSrcLinksToKeep.Contains(x.FeedSrcLink)).ToList();
            DownloadedFeedItemList = new ObservableCollection<DownloadedFeedItem>(keepList);

            cleanUpOlderEntries();
        }

        public void cleanUpOlderEntries()
        {
            //NOTE: Remove old items.
            var newerEntries = DownloadedFeedItemList.
                Where(x => (DateTime.UtcNow - x.PublishedDate).Days <= numOfDaysOldEntriesToCleanup).ToList();
            DownloadedFeedItemList = new ObservableCollection<DownloadedFeedItem>(newerEntries);
        }

        private ObservableCollection<DownloadedFeedItem> downloadedFeedItemList = 
            new ObservableCollection<DownloadedFeedItem>();

        public ObservableCollection<DownloadedFeedItem> DownloadedFeedItemList
        {
            get
            {
                return downloadedFeedItemList;
            }
            set
            {
                downloadedFeedItemList = value;
                OnPropertyChanged("DownloadedFeedItemList");
            }
        }

        public static DownloadedFeeds DeSerialize()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DownloadedFeeds));
                using (FileStream fileStream = new FileStream("downloadedFeeds.xml", FileMode.Open))
                {
                    return (DownloadedFeeds)serializer.Deserialize(fileStream);
                }
            }
            catch (Exception)
            {
                //NOTE: Return new blank if any error, like file does not exist, first time
                return new DownloadedFeeds();
            }
        }

        public void Serialize()
        {
            using (var writer = new System.IO.StreamWriter("downloadedFeeds.xml"))
            {
                new XmlSerializer(this.GetType()).Serialize(writer, this);
                writer.Flush();
            }
        }

        public void mergeWith(DownloadedFeeds otherDownloadedFeeds)
        {
            try
            {
                var mergedList = new ObservableCollection<DownloadedFeedItem>();
                foreach (DownloadedFeedItem item in otherDownloadedFeeds.DownloadedFeedItemList)
                {
                    if (DownloadedFeedItemList.Count(x => x.Link == item.Link) == 0)
                    {
                        mergedList.Add(item);
                    }
                }
                foreach (var item in DownloadedFeedItemList)
                {
                    mergedList.Add(item);
                }
                DownloadedFeedItemList = new ObservableCollection<DownloadedFeedItem>(
                    mergedList.OrderByDescending(x => x.PublishedDate));
            }
            catch (Exception ex)
            {
                int u = 9;
                throw;
            }
        }

        public bool isAnyNewEntryPresent()
        {
            return DownloadedFeedItemList.Any(x => x.NewEntry == true);
        }

        public void switchNewEntriesToOld()
        {
            foreach(var item in DownloadedFeedItemList)
            {
                item.NewEntry = false;
            }
        }
    }
}
