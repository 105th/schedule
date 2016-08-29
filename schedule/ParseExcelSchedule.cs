using OfficeOpenXml;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

namespace schedule
{
	public static class ParseExcelSchedule
	{
		//TODO Либо находить нужные клетки из таблицы, либо рассмотреть все варианты таблиц
		/// <summary>
		/// Парсит расписание из таблицы и возвращает список из рабочих дней с расписанием.
		/// ВАЖНО!!! Здесь индексы подобраны из тестового образца, так что могут не совпадать с реальными данными.
		/// </summary>
		/// <param name="filePath">Путь к таблице с расписанием.</param>
		public static List<WorkDay> Parse(string filePath)
		{
			// Список будней, куда будем сохранять расписание
			List<WorkDay> schedule = new List<WorkDay>();

			// Создаем файловый поток из нашего файла.
			FileStream scheduleDoc = new FileStream(filePath, FileMode.Open);

			// "Распаковываем" таблицу.
			using (ExcelPackage package = new ExcelPackage(scheduleDoc))
			{
				// Извлекаем оттуда рабочую книгу и рабочую таблицу.
				ExcelWorkbook scheduleWorkbook = package.Workbook;
				ExcelWorksheets scheduleWorksheets = scheduleWorkbook.Worksheets;
				ExcelWorksheet sheet = scheduleWorksheets[1];

				// Разбиваем объединённые клетки, чтобы было проще обрабатывать их.
				BreakMergedCells(sheet);

				// Флаг, который сохраняет номер текущего дня
				int presentDay = -1;

				// Пробегаемся в цикле по всем строчкам 
				// и сохраняем элементы таблицы в список рабочих дней.
				for (int i = 1; i < sheet.Dimension.Rows; i++)
				{
					// Если есть название дня, если есть пара и это не "Самостоятельная работа",
					// тогда обрабатываем это занятие.
					if (GetIntNumberFromDayWeek(sheet.Cells[i,1].Text) != 0 &&
					    sheet.Cells[i,16].Text != "Самостоятельная работа" &&
					    !(string.IsNullOrWhiteSpace(sheet.Cells[i,16].Text)))
					{
						WorkDay tmp = new WorkDay();

						// Если мы сменили день, значит это первая пара за этот день.
						if (GetIntNumberFromDayWeek(sheet.Cells[i,1].Text) != presentDay
						    || (i != 1 && sheet.Cells[i,6].Text == sheet.Cells[i - 1,6].Text))
						{
							presentDay = GetIntNumberFromDayWeek(sheet.Cells[i,1].Text);
							tmp.isFirstClassesOfADay = true;
						}

						// Сохраняем номер дня неделя
						tmp.dayNumber = GetIntNumberFromDayWeek(sheet.Cells[i,1].Text);

						// Разбиваем время на две строки (начало и конец пары),
						// чтобы в дальнейшем было удобней использовать.
						tmp.timeClassStart = TimeSpan.Parse(sheet.Cells[i,6].Text.Split('-')[0].Replace('.', ':'));
						tmp.timeClassEnd = TimeSpan.Parse(sheet.Cells[i,6].Text.Split('-')[1].Replace('.', ':'));

						// Выделяем из столбца названия предмета ТОЛЬКО название,
						// отсекая цифру 2 (для предметов, идущих второй семестр),
						// и отсекая имя преподавателя.
						if (Regex.IsMatch(sheet.Cells[i,16].Text, @"\("))
							tmp.nameSubject = sheet.Cells[i,16].Text.
								Substring(0, Regex.Match(sheet.Cells[i,16].Text, @"\(").Index);
						else
							tmp.nameSubject = sheet.Cells[i,16].Text;

						//Находим в названии предмета имя преподавателя и убираем оттуда скобки 
						tmp.nameLecturer = Regex.Match(sheet.Cells[i,16].Text, @"\([^0-9]+\)").
							ToString().Replace("(", "").Replace(")", "");
						tmp.typeClass = (sheet.Cells[i,53].Text == "л") ? "Лекция" : "Семинар";

						// Разбиваем строку на целые значения - номера недель.
						string repeatAt = sheet.Cells[i,57].Text;
						foreach (string weekNumber in repeatAt.Split(','))
						{
							if (weekNumber.Contains("-"))
							{
								// Обрабатываем период недель С какой-то ПО какую-то.
								string[] numberPeriod = weekNumber.Split('-');
								for (int j = int.Parse(numberPeriod[0]); j <= int.Parse(numberPeriod[1]); j++)
								{
									tmp.repeatAt.Add(j);
								}
							}
							else
							{
								// Обрабатываем единичные недели.
								tmp.repeatAt.Add(int.Parse(weekNumber));
							}
						}

						tmp.place = sheet.Cells[i,62].Text;


						schedule.Add(tmp);
					}
				}
			}

			return schedule;
		}

		/// <summary>
		/// Разбиваем объединённые клетки.
		/// </summary>
		/// <param name="sheet">Таблица</param>
		public static void BreakMergedCells(ExcelWorksheet sheet)
		{
			// В обратном цикле пробегаемся по всем клеткам, которые объединeны.
			for (int i = sheet.MergedCells.Count - 1; i >= 0; i--)
			{
				// Выбираем промежуток объединённых клеток.
				sheet.Select(sheet.MergedCells[i]);
				// Меняем статус объединения.
				sheet.SelectedRange.Merge = false;
				// Сохраняем стартовую позицию.
				ExcelRange start = sheet.Cells[sheet.SelectedRange.Start.Address];
				// Пробегаемся по промежутку объединённых клеток.
				foreach (var item in sheet.SelectedRange)
				{
					// Занося в каждую значение из стартовой клетки промежутка.
					sheet.SetValue(item.Address, start.Text);
				}
			}
		}


		/// <summary>
		/// Преобразуем стринговое значение дня недели в порядковый номер.
		/// </summary>
		/// <returns>Номер дня недели.</returns>
		/// <param name="dayWeek">День недели.</param>
		public static int GetIntNumberFromDayWeek(string dayWeek)
		{
			switch (dayWeek.ToLower())
			{
				case "monday":
				case "понедельник":
					return 1;
				case "tuesday":
				case "вторник":
					return 2;
				case "wednesday":
				case "среда":
					return 3;
				case "thursday":
				case "четверг":
					return 4;
				case "friday":
				case "пятница":
					return 5;
				case "saturday":
				case"суббота":
					return 6;
				case "sunday":
				case "воскресенье":
					return 7;
				default:
					return 0;
			}
		}
	}
}

