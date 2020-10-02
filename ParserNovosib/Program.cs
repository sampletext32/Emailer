using System;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace ParserNovosib
{
    class Program
    {
        private static HtmlDocument LoadDoc(string url)
        {
            return new HtmlWeb().Load(url);
        }

        static void Main(string[] args)
        {
            for (int i = 0; i < 8; i++)
            {
                var page = i != 0 ? $"page{i + 1}/" : "";
                string group = $"https://asktel.ru/novosibirsk/shkola/{page}";
                Console.WriteLine(group);
                var document = LoadDoc(@group);
                var aNodes = document.DocumentNode.SelectNodes("//div[@class='companyEmail']/a");
                var emails = aNodes.Select(a => a.InnerText);
                File.AppendAllLines("emails.txt", emails);
                Console.WriteLine(string.Join(", ", emails));
            }
        }
    }
}
