﻿using System;
using System.IO;
using System.Linq;
using HtmlAgilityPack;

namespace ParserNNovg
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
                var page = i != 0 ? $"page{i + 1}/" : "";
                string group = $"https://asktel.ru/nizhnij_novgorod/shkola/{page}";
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
