using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using GodaiLibrary.GodaiQuest;

namespace GodaiQuestServer
{
    public class RSSReaderWorker
    {
        private ServerWorker mParent;
        private bool mTerminate;

        public RSSReaderWorker(ServerWorker parent_)
        {
            this.mParent = parent_;
        }

        public void terminate()
        {
            this.mTerminate = true;
        }

        public void Run()
        {
            Thread.Sleep(20 * 1000);

            RssReader reader = new RssReader();

            while (!this.mTerminate)
            {
                Thread.Sleep(20 * 1000);

                ObjectAttrInfo objectinfo;
                if ( this.mParent.getObjectAttrInfo(out objectinfo) != EServerResult.SUCCESS )
                    continue;

                ItemInfo iteminfo;
                if ( this.mParent.getItemInfo(out iteminfo) != EServerResult.SUCCESS )
                    continue;

                foreach (var obj in objectinfo)
                {
                    if (this.mTerminate)
                        break;

                    int nItemID = obj.getItemID();
                    if ( nItemID == 0 )
                        continue;

                    var item = iteminfo.getAItem(nItemID);
                    String strHeader = item.getHeaderString();
                    if ( strHeader == null || strHeader.Length == 0 )
                        continue;

                    // タグを探す
                    int nIndexRSS = strHeader.ToUpper().IndexOf("RSS://");
                    if (nIndexRSS < 0)
                        continue;

                    StringBuilder strURL = new StringBuilder();
                    for (int ic = nIndexRSS+6; ic < strHeader.Length; ++ic)
                    {
                        if ( strHeader[ic] < 0x20 )
                            break;
                        strURL.Append(strHeader[ic]);
                    }

                    // 最終更新日付を得る
                    DateTime lastupdatetime = this.mParent.getLastUpdateTimeOfRSS(nItemID, DateTime.Now.AddDays(-1));

                    List<RSSItem> listItem;
                    try
                    {
                        // アイテムを読み込む
                        String strURL2 = strURL.ToString();
                        listItem = reader.GetRssData(strURL2, lastupdatetime.ToLocalTime());
                    }
                    catch (Exception)
                    {
                        // 例外は握りつぶす
                        continue;
                    }

                    // 最終更新日付を更新する
                    this.mParent.setLastUpdateTimeOfRSS(nItemID, DateTime.Now);

                    // 記事に書き込む
                    listItem.Reverse();
                    foreach(var rssitem in listItem) {
                        string strContent = rssitem.Title + "\r\n" + rssitem.Link + "\r\n";

                        ItemArticle article = new ItemArticle(nItemID, 0, 0/*system*/, strContent, rssitem.PubDate);
                        this.mParent.setItemAritcle(article);
                    }

                    // 2秒休みを入れる
                    Thread.Sleep(2 * 1000);
                }

                // 30分の休みを入れる
                for (int iti = 0; iti < 30 * 60; ++iti)
                {
                    if (this.mTerminate)
                        break;
                    Thread.Sleep(1000);
                }
            }
        }
    }

    //http://d.hatena.ne.jp/pongeponge/20110608/1307504187を真似しました。ありがとうございます。
    class RssReader
    {
        /// RSSのURLからタイトル、リンク、タイムスタンプを読み込む
        public List<RSSItem> GetRssData(string url, DateTime LastUpdateTime)
        {
            //ドキュメント読み込み
            XmlDocument Doc = new System.Xml.XmlDocument();
            Doc.Load(url);

            XmlElement rssElem = Doc.DocumentElement;
            List<RSSItem> RSSItems = new List<RSSItem>();

            if (rssElem.Name == "rdf:RDF")
            {
                RSSItems = GetRss10Item(Doc, LastUpdateTime);
            }
            else if (rssElem.Name == "rss")
            {
                RSSItems = GetRss20Item(Doc, LastUpdateTime);
            }

            return RSSItems;
        }

        /// RSS2.0からタイトル、リンク、タイムスタンプを抜く
        private List<RSSItem> GetRss20Item(XmlDocument Doc, DateTime LastUpdateTime)
        {
            List<RSSItem> items = new List<RSSItem>();
            XmlNodeList NodeList = Doc.SelectNodes("/rss/channel/item");

            foreach (XmlNode Node in NodeList)
            {
                RSSItem item = new RSSItem();

                //データの取得
                foreach (XmlNode Data in Node)
                {
                    switch (Data.Name)
                    {
                        case "title":
                            item.Title = Data.InnerText;
                            break;
                        case "link":
                            item.Link = Data.InnerText;
                            break;
                        case "pubDate":
                            item.PubDate = DateTime.Parse(Data.InnerText);
                            break;
                    }
                }

                //最終更新より過去のデータは追加しない
                if (item.PubDate > LastUpdateTime) items.Add(item);
            }

            return items;
        }

        /// RSS1.0からタイトル、リンク、タイムスタンプを抜く
        private List<RSSItem> GetRss10Item(XmlDocument Doc, DateTime LastUpdateTime)
        {
            List<RSSItem> items = new List<RSSItem>();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(Doc.NameTable);

            //名前空間の設定
            nsmgr.AddNamespace(Doc.DocumentElement.Prefix, Doc.DocumentElement.NamespaceURI);
            nsmgr.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
            nsmgr.AddNamespace("r", "http://purl.org/rss/1.0/");
            XmlNodeList NodeList = Doc.SelectNodes("/rdf:RDF/r:item", nsmgr);

            //データの取得
            foreach (XmlNode node in NodeList)
            {
                RSSItem item = new RSSItem();
                item.Title = node.SelectSingleNode("r:title", nsmgr).InnerText;
                item.Link = node.SelectSingleNode("r:link", nsmgr).InnerText;
                item.PubDate = DateTime.Parse(node.SelectSingleNode("dc:date", nsmgr).InnerText);

                //最終更新より過去のデータは追加しない
                if (item.PubDate > LastUpdateTime) items.Add(item);
            }

            return items;
        }
    }

    class RSSItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public DateTime PubDate { get; set; }
    }
}
