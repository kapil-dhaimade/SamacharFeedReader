using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace SamacharFeedReader
{
    public class SubscribedFeed
    {
        public string Link
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }
    public class SubscribedFeeds
    {
        private ObservableCollection<SubscribedFeed> feedList = new ObservableCollection<SubscribedFeed>();

        public ObservableCollection<SubscribedFeed> FeedList
        {
            get
            {
                return feedList;
            }
            set
            {
                feedList = value;
            }
        }

        public static SubscribedFeeds DeSerialize()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SubscribedFeeds));
                using (FileStream fileStream = new FileStream("subscribedFeeds.txt", FileMode.Open))
                {
                    return (SubscribedFeeds)serializer.Deserialize(fileStream);
                }
            }
            catch (Exception)
            {
                //NOTE: Return new blank if any error, like file does not exist, first time
                return new SubscribedFeeds();
            }
        }

        public void Serialize()
        {
            using (var writer = new System.IO.StreamWriter("subscribedFeeds.txt"))
            {
                new XmlSerializer(this.GetType()).Serialize(writer, this);
                writer.Flush();
            }
        }
    }
}
