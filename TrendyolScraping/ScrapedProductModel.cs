using System.Collections.Generic;

namespace TrendyolScraping
{
    public class ScrapedProductModel
    {
        public ScrapedProductModel()
        {
            ImageUrls = new List<string>();
        }

        public string ProductName { get; set; }

        public string Price { get; set; }

        public List<string> ImageUrls { get; set; }
    }
}