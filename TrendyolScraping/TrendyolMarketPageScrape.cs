using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TrendyolScraping
{
    internal sealed class TrendyolMarketPageScrape : IDisposable
    {
        private readonly string trendyolUrl = "https://www.trendyol.com";
        private readonly string currentUrl;

        private List<string> productUrls;

        private HtmlDocument htmlDocument;
        private HtmlWeb htmlWeb;

        private List<TendyolProductScrape> productScrapes = new List<TendyolProductScrape>();

        public TrendyolMarketPageScrape(string url)
        {
            currentUrl = url;
        }

        public TrendyolMarketPageScrape Initiliaze()
        {
            htmlWeb = new HtmlWeb()
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36",
                BrowserDelay = new TimeSpan(0, 0, 10)
            };
            htmlDocument = htmlWeb.Load(currentUrl);
            productUrls = GetProductUrls();
            return this;
        }

        public async Task<Task> Scrape(Action<ScrapedProductModel> callback)
        {
            List<Task> tasks = new List<Task>();
            foreach (var item in productUrls)
            {
                var scraper = new TendyolProductScrape(item).Intiliaze();
                productScrapes.Add(scraper);
                tasks.Add(Task.Run(() =>
                {
                    scraper.Scrape(callback);
                }));
                await Task.Delay(100);
            }
            await Task.WhenAll(tasks);
            Dispose();
            return Task.CompletedTask;
        }

        private List<string> GetProductUrls()
        {
            List<string> productUrls = new List<string>();
            var nodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='p-card-chldrn-cntnr']//a");
            if (nodes == null)
            {
                return productUrls;
            }
            foreach (var node in nodes)
            {
                var href = trendyolUrl + node.GetAttributeValue("href", "");
                productUrls.Add(href);
            }
            return productUrls;
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

                htmlDocument = null;
                htmlWeb = null;
                productScrapes = null;

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