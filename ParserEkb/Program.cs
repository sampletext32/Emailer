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

        static void Main(string[] args)
        {
            for (int i = 0; i < 6; i++)
            {
                string group = $"https://asktel.ru/ekaterinburg/shkola/page{i + 1}/";
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