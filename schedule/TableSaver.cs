﻿using System;
using HtmlAgilityPack;
using System.Collections.Generic;
using System.Linq;

namespace schedule
{
	public static class TableSaver
	{
		public static void DownloadAll(string path)
		{
			// Адрес сайта, откуда будем парсить данные.
			string WebAddress = path;

			// Создаём экземляры классов веб-страницы и веб-документа
			HtmlWeb WebGet = new HtmlWeb();

			// Загружаем html-документ с указанного сайта.
			HtmlDocument htmlDoc = WebGet.Load(WebAddress);

			// Парсим название клуба (удаляя символ возрата каретки)
			//		MatchesFC.NameFC = htmlDoc.DocumentNode.
			//			SelectSingleNode(".//*[@class='titleH1']").
			//			FirstChild.InnerText.Replace("\r\n", "");


			// Находим в этом документе таблицу с датами матчей с помощью XPath-выражений.
			HtmlNode Table = htmlDoc.DocumentNode.SelectSingleNode(".//*[@class='stat-table']/tbody");
			// Из полученной таблицы выделяем все элементы-строки с тегом "tr".
			IEnumerable<HtmlNode> refs = Table.Descendants().Where(x => x.Name == "a");

			//		foreach (var row in rows)
			//		{
			//			// Создаём коллекцию из ячеек каждой строки.
			//			HtmlNodeCollection cells = row.ChildNodes;
			//			// Создаём экземпляр класса SingleMatch, чтобы затем добавить его в лист.
			//			SingleMatch match = new SingleMatch();
			//
			//			// Парсим дату, предварительно убирая из строки символ черточки "|",
			//			// иначе наш метод TryParse не сможет правильно обработать.
			//			DateTime time;
			//			DateTime.TryParse(cells[1].InnerText.Replace("|", " "), out time);
			//			match.StartTime = time;
			//
			//			// Остальные поля просто заполняем, зная нужный нам индекс.
			//			match.Tournament = cells[3].InnerText;
			//			// В ячейке "Соперник" нужно удалить символ неразрывного пробела ("&nbsp")
			//			match.Rival = cells[5].InnerText.Replace("&nbsp;", "");
			//			match.Place = cells[6].InnerText;
			//
			//			// Добавляем одиночный матч в лист матчей.
			//			MatchesFC.ListMatches.Add(match);
			//		}
		}
	}
}
