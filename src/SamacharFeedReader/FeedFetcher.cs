using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace SamacharFeedReader
{
    class FeedFetcher
    {
        DownloadedFeeds downloadedFeeds = null;
        DownloadedFeedsWindow downloadedFeedsWnd = null;
        public FeedFetcher(DownloadedFeeds pDownloadedFeeds, DownloadedFeedsWindow pDownloadedFeedsWnd)
        {
            downloadedFeeds = pDownloadedFeeds;
            downloadedFeedsWnd = pDownloadedFeedsWnd;
        }

        public async Task run()
        {
            //NOTE: To connect to server without valid SSL cert in demo env.
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            //NOTE: To connect to server with older SSL/TLS protocols.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | 
                SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            var client = new HttpClient();
            //NOTE: If this is not added, some RSS sites return 403 Forbidden HTTP code.
            client.DefaultRequestHeaders.Add("User-Agent", "a");

            SubscribedFeeds feedSources= SubscribedFeeds.DeSerialize();
            DownloadedFeeds currDownloadedFeeds = new DownloadedFeeds();

            foreach (var feedSrc in feedSources.FeedList)
            {
                try
                {
                    var result = await client.GetStreamAsync(feedSrc.Link);

                    using (var xmlReader = XmlReader.Create(result))
                    {
                        SyndicationFeed feed = SyndicationFeed.Load(xmlReader);

                        if (feed != null)
                        {
                            foreach (var item in feed.Items)
                            {
                                currDownloadedFeeds.DownloadedFeedItemList.Add(
                                    new DownloadedFeedItem { FeedName=feedSrc.Name, 
                                        Link=item.Links.First<SyndicationLink>().Uri.ToString(), NewEntry=true, 
                                        PublishedDate=item.PublishDate.UtcDateTime, Title=item.Title.Text, 
                                        FeedSrcLink=feedSrc.Link});
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //NOTE: Eat exception if error in any 1 feed. Continue to others.
                    int u = 9;
                }
            }

            downloadedFeeds.mergeWith(currDownloadedFeeds);
            downloadedFeeds.cleanUpOlderEntries();

            if (downloadedFeeds.isAnyNewEntryPresent())
            {
                await System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    downloadedFeedsWnd.showActivated();
                }));
            }
            downloadedFeeds.Serialize();
        }
    }
}
