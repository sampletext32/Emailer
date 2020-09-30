using System;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace ParserKzn
{
    class Program
    {
        private static HtmlDocument LoadDoc(string url)
        {
            return new HtmlWeb().Load(url);
        }

        static void Main(string[] args)
        {
            string[] groups =
            {
                "https://edu.tatar.ru/aviastroit/type/1",
                "http://edu.tatar.ru/vahit/type/1",
                "https://edu.tatar.ru/kirov/type/1",
                "https://edu.tatar.ru/moskow/type/1",
                "https://edu.tatar.ru/nsav/type/1",
                "http://edu.tatar.ru/priv/type/1",
                "https://edu.tatar.ru/sovetcki/type/1"
            };
            foreach (var group in groups)
            {
                Console.WriteLine("\tGROUP - {0}", group);
                var document = LoadDoc(group);
                var aNodes = document.DocumentNode.SelectNodes("//ul[@class='edu-list col-md-4']/li/a");
                var links = aNodes.Select(a => "https://edu.tatar.ru" + a.Attributes["href"].Value).ToList();
                var emails = links.Select(link =>
                {
                    Console.WriteLine("\t\tSCHOOL - {0}", link);
                    return LoadDoc(link).DocumentNode.SelectSingleNode("//tr/td/strong[text()='E-Mail:']/../../td[2]")
                        .InnerText;
                }).ToList();
                Console.WriteLine("FOUND - {0}", string.Join(", ", emails));
                File.AppendAllLines("emails.txt", emails);
            }

            Console.WriteLine("END");
        }
    }
}
