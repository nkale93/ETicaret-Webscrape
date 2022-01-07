using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TrendyolScraping;

namespace ScrapeConsoleApp
{
    internal class Program
    {
        private static readonly List<ScrapedProductModel> models = new List<ScrapedProductModel>();
        private static int count = 0;
        private static int total = 0;

        private static void Main(string[] args)
        {
            bool incorrectUrlFormat = true;
            string url = string.Empty;
            Console.WriteLine("Trendyol mağaza ürünlerinin adı, fiyat ve resimlerini alır.");
            Console.WriteLine("mağaza urlsi https://www.trendyol.com/sr?mid= kısmını içermelidir veya benzemelidir.");
            do
            {
                Console.Write("Mağaza urlsini giriniz: ");
                url = Console.ReadLine();
                if (url.Contains("https://www.trendyol.com/sr?mid="))
                {
                    incorrectUrlFormat = false;
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine("Hatalı url tekrar deniyiniz !");
                }
            } while (incorrectUrlFormat);
            TrendyolMarketPlaceScrape market = new TrendyolMarketPlaceScrape(url)
                .Initiliaze();
            total = market.TotalProduct;
            Console.WriteLine($"toplam {total} ürün bulundu.\nÜrünler alınmaya başlıyor...");
            var task = market.StartScrape(Callback);
            task.Wait();
            DateTime now = DateTime.Now;
            while (task.IsCompleted == false)
            {
                Console.WriteLine(DateTime.Now - now);
                Task.Run(() => { Task.Delay(1000); }).Wait();
            }
            using (StreamWriter file = File.CreateText("products.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, models);
            }
            Console.Write("Çıkış yapmak için herhangibir tuşa basınız...");
            Console.ReadKey();
        }

        private static void Callback(ScrapedProductModel obj)
        {
            count++;
            Console.WriteLine($"{count}\t/\t{total}");
            models.Add(obj);
        }
    }
}