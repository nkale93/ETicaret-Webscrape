using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TrendyolScraping
{
    public sealed class TrendyolMarketPlaceScrape : IDisposable
    {
        private string baseMarketPlaceUrl;
        private readonly string currentUrl;

        private int totalProduct;
        private int totalProductAtOnePage;
        private int totalPage;

        private List<string> productPageUrls;
        private List<TrendyolMarketPageScrape> pages;

        private HtmlDocument htmlDocument;

        private HtmlWeb htmlWeb;
        private bool initiliazed = false;

        public int TotalProduct { get => totalProduct; }

        public TrendyolMarketPlaceScrape(string url)
        {
            currentUrl = CorrectMarketPlaceUrl(url);
        }

        public TrendyolMarketPlaceScrape Initiliaze()
        {
            initiliazed = true;
            htmlWeb = new HtmlWeb
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36"
            };
            htmlDocument = htmlWeb.Load(currentUrl);
            totalProduct = GetTotalProductCount();
            totalProductAtOnePage = GetTotalProductAtOnePage();
            totalPage = (int)Math.Ceiling(totalProduct / (double)totalProductAtOnePage);
            productPageUrls = GenerateProductPageUrls();
            pages = GeneratePages();
            return this;
        }

        public async Task StartScrape(Action<ScrapedProductModel> callback)
        {
            if (initiliazed == false)
            {
                return;
            }
            List<Task> tasks = new List<Task>();
            foreach (var item in pages)
            {
                var task = Task.Run(async () =>
                {
                    return await item.Scrape(callback);
                });
                tasks.Add(task);
            }
            await Task.WhenAll(tasks);
        }

        private string CorrectMarketPlaceUrl(string url)
        {
            string piString = "pi=";
            if (url.Contains(piString))
            {
                int indexOfPi = url.IndexOf(piString);
                baseMarketPlaceUrl = url.Substring(0, indexOfPi + piString.Length);
                url = url.Substring(0, indexOfPi + piString.Length) + "1";
            }
            else
            {
                baseMarketPlaceUrl = url + "&pi=";
                url += "&pi=1";
            }
            return url;
        }

        private int GetTotalProductCount()
        {
            var node = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='dscrptn']");
            if (node == null)
            {
                return 0;
            }
            var numberString = Regex.Match(node.InnerText, @"\d+").Value;
            var parsedInt = int.TryParse(numberString, out int totalProductCount);
            if (parsedInt)
            {
                return totalProductCount;
            }
            else
            {
                return 0;
            }
        }

        private int GetTotalProductAtOnePage()
        {
            var nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='p-card-wrppr']");
            if (nodes == null)
            {
                return 0;
            }
            else
            {
                return nodes.Count();
            }
        }

        private List<string> GenerateProductPageUrls()
        {
            List<string> result = new List<string>();
            if (totalPage > 0)
            {
                for (int i = 1; i <= totalPage; i++)
                {
                    result.Add(baseMarketPlaceUrl + i);
                }
            }
            return result;
        }

        private List<TrendyolMarketPageScrape> GeneratePages()
        {
            List<TrendyolMarketPageScrape> pages = new List<TrendyolMarketPageScrape>();
            for (int i = 0; i < productPageUrls.Count; i++)
            {
                pages.Add(new TrendyolMarketPageScrape(productPageUrls[i]).Initiliaze());
            }
            return pages;
        }

        private bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                pages = null;
                htmlDocument = null;
                htmlWeb = null;
                productPageUrls = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}