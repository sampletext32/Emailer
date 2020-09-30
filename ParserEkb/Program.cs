using System;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace ParserEkb
{
    class Program
    {
        private static HtmlDocument LoadDoc(string url)
        {
            return new HtmlWeb().Load(url);
        }

        private static void V1()
        {
            for (int i = 0; i < 6; i++)
            {
                var page = i != 0 ? $"page{i + 1}/" : "";
                string group = $"https://asktel.ru/ekaterinburg/shkoly/{page}";
                Console.WriteLine(group);
                var document = LoadDoc(@group);
                var aNodes = document.DocumentNode.SelectNodes("//div[@class='companyEmail']/a");
                var emails = aNodes.Select(a => a.InnerText);
                File.AppendAllLines("emails.txt", emails);
                Console.WriteLine(string.Join(", ", emails));
            }
        }

        private static void V2()
        {
            for (int i = 0; i < 11; i++)
            {
                string group = $"http://obrazovanie66.ru/shkoly/page/{i + 1}/";
                Console.WriteLine(group);
                var document = LoadDoc(group);
                var aNodes = document.DocumentNode.SelectNodes("//a[@class='school_link']");
                var links = aNodes.Select(a => a.Attributes["href"].Value);
                foreach (var link in links)
                {
                    Console.WriteLine("\t{0}", link);
                    var doc = LoadDoc(link);
                    var aNodeEmails =
                        doc.DocumentNode.SelectNodes("//tr/td[@class='txtTableTitle' and text()='Почта']/../td[2]/a");

                    var emails = aNodeEmails.Select(a => a.InnerText);
                    File.AppendAllLines("emails.txt", emails);
                    Console.WriteLine(string.Join(", ", emails));
                }
            }
        }

        static void Main(string[] args)
        {
        }
    }
}