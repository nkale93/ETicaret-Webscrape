using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TrendyolScraping
{
    internal sealed class TendyolProductScrape : IDisposable
    {
        private readonly string currentUrl;

        private List<string> images = new List<string>();

        private HtmlDocument htmlDocument;
        private HtmlWeb htmlWeb;

        public TendyolProductScrape(string url)
        {
            currentUrl = url;
        }

        public TendyolProductScrape Intiliaze()
        {
            bool connected = false;
            htmlWeb = new HtmlWeb()
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.131 Safari/537.36",
                BrowserDelay = new TimeSpan(0, 0, 10)
            };
            do
            {
                htmlDocument = htmlWeb.Load(currentUrl);
                if ((int)htmlWeb.StatusCode == 429)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(5000);
                    }).Wait(); ;
                }
                else
                {
                    connected = true;
                }
            } while (connected == false);
            return this;
        }

        public Task Scrape(Action<ScrapedProductModel> callback)
        {
            GetProductName();
            GetPrice();
            GetImages();
            ScrapedProductModel model = new ScrapedProductModel()
            {
                ProductName = productName,
                Price = price,
                ImageUrls = images,
            };
            callback.Invoke(model);
            Dispose();
            return Task.CompletedTask;
        }

        private Task GetImages()
        {
            var containerNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='product-slide-container']");
            if (containerNode == null)
            {
                containerNode = htmlDocument.DocumentNode.SelectSingleNode("//img[@class='detail-section-img']");
                var url = containerNode.GetAttributeValue("src", string.Empty);
                images.Add(url);
                return Task.CompletedTask;
            }
            var nodes = containerNode.SelectNodes(".//img[@loading='lazy']");
            foreach (var item in nodes)
            {
                var url = CorrentImageUrl(item.GetAttributeValue("src", string.Empty));
                images.Add(url);
            }
            return Task.CompletedTask;
        }

        private string CorrentImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            var httpremoved = url.Substring(8, url.Length - 8);
            var splitted = httpremoved.Split('/').ToList();
            splitted.RemoveAt(3);
            splitted.RemoveAt(2);
            splitted.RemoveAt(1);
            var imageUrl = "https://" + string.Join("/", splitted.ToArray());
            return imageUrl;
        }

        private string price;

        private Task GetPrice()
        {
            var prc = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='prc-slg']");
            if (prc != null)
            {
                price = prc.InnerText;
                return Task.CompletedTask;
            }
            prc = htmlDocument.DocumentNode.SelectSingleNode("//span[@class='prc-slg prc-slg-w-dsc']");
            if (prc != null)
            {
                price = prc.InnerText;
                return Task.CompletedTask;
            }
            return Task.CompletedTask;
        }

        private string productName;

        private void GetProductName()
        {
            var brand = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='pr-new-br']//a");
            if (brand != null)
            {
                var brandName = brand.InnerText;
                var product = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='pr-new-br']//span").InnerText;
                productName = $"{brandName} {product}";
                return;
            }
            brand = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='pr-new-br']");
            if (brand != null)
            {
                var brandName = brand.InnerText;
                var product = htmlDocument.DocumentNode.SelectSingleNode("//h1[@class='pr-new-br']//span").InnerText;
                productName = $"{brandName} {product}";
                return;
            }
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

                htmlWeb = null;
                htmlDocument = null;
                images = null;

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