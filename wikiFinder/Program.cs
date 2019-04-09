using HtmlAgilityPack; // uses NuGet package : Project > Manage NuGet Packages > browse for HtmlAgilityPack
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

class Program
{
    static void Main(string[] args)
    {
        Program p = new Program();
        string keyword = "Poland"; // splitstring will be used
        p.SetMaxDepthOfSearch(3);
        p.FindTheMan(keyword);
        Console.WriteLine("Press any key to exit...");
        Console.ReadLine();
    }

    private List<List<string>> addresses = new List<List<string>>();
    private int depthCounter = 0;
    private int maxDepth;

    public void SetMaxDepthOfSearch(int d)
    {
        this.maxDepth = d;
    }

    private int GetMaxDepthOfSearch()
    {
        return this.maxDepth;
    }

    private void AddCounter()
    {
        this.depthCounter++;
    }
    private int GetCounter()
    {
        return this.depthCounter;
    }
    private void SetAddresses(List<string> add)
    {
        this.addresses.Add(add);
    }
    private List<List<string>> GetAddresses()
    {
        return this.addresses;
    }

    private bool IsAddressPresent(string html)
    {
        foreach (List<string> l in GetAddresses())
        {
            if (l.Contains(html))
            {
                return true;
            }
        }
        return false;
    }

    public void FindTheMan(string keyWords)
    {
        MyWebClient wb = new MyWebClient();
        string html = "https://en.wikipedia.org/wiki/Special:Random";
        string[] names = keyWords.Split(' ');

        Console.WriteLine(wb.ReadPage(html));
        AddCounter();

        foreach (string s in names)
        {
            if (wb.ReadPage(html).Contains(s))
            {
                Console.WriteLine("found {0} at depth {1}", s, GetCounter());
                Console.WriteLine(html);
                return;
            }
        }

        SetAddresses(ExtractAllAHrefTags(html));

        int listCount = GetAddresses().Count;
        for (int i = 0; i < listCount; i++)
        {
            AddCounter();
            if (GetCounter() > GetMaxDepthOfSearch())
            {
                Console.WriteLine("Actual depth is bigger than max depth");
                return;
            }

            List<string> tmp = new List<string>();
            for (int j = 0; j < GetAddresses()[i].Count; j++)
            {
                html = "https://en.wikipedia.org" + GetAddresses()[i][j];
                Console.WriteLine(html);
                Console.WriteLine(wb.ReadPage(html));
                foreach (string s in names)
                {
                    if (wb.ReadPage(html).Contains(s))
                    {
                        Console.WriteLine("found {0} at depth {1}", s, GetCounter());
                        Console.WriteLine(html);
                        return;
                    }
                }
                tmp.AddRange(ExtractAllAHrefTags(html));
            }
            SetAddresses(tmp);

            listCount++;
        }
    }

    private List<string> ExtractAllAHrefTags(string url)
    {
        List<string> hrefTags = new List<string>();
        HtmlWeb hw = new HtmlWeb();
        HtmlDocument doc = hw.Load(url);
        foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            HtmlAttribute att = link.Attributes["href"];
            if (att.Value.StartsWith("/wiki/"))
            {

                if (GetAddresses().Count > 0)
                {
                    foreach (List<string> l in GetAddresses())
                    {
                        if (!l.Contains(att.Value))
                        {
                            hrefTags.Add(att.Value);
                        }
                    }
                }
                else
                {
                    hrefTags.Add(att.Value);
                }
            }
        }

        return hrefTags;
    }

    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }

        public string ReadPage(string url)
        {
            string html = "";
            Uri ur = new Uri(url);
            HttpWebRequest req = (HttpWebRequest)this.GetWebRequest(ur);

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                html = reader.ReadToEnd();
            }
            return html;
        }

        public void OldREader()
        {
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create("https://onet.pl");
            Request.UserAgent = "Client Cert Sample";
            Request.Method = "GET";
            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
        }
    }
}