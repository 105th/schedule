using System;
using System.Collections.Generic;
using DDay.iCal;
using DDay.iCal.Serialization.iCalendar;
using System.Linq;
using Emojione;

namespace schedule
{
	class MainClass
	{
		const string filename = @"rasp8sem";
		const string fileCalcName = @"Расписание_ДКО130_8сем";

		public static void Main (string[] args)
		{
			// Название нашей таблицы с расписанием.
			string filePath = filename + @".xlsx";

			// Парсим xlsx-таблицу
			List<WorkDay> week = ParseExcelSchedule.Parse(filePath);

			// Задаем дату начала семестра.
			iCalDateTime startStudy = new iCalDateTime(2016, 9, 1);

			// Создаём календарь, в который будем сохранять матчи.
			iCalendar CalForSchedule = new iCalendar
			{
				Method = "PUBLISH",
				Version = "2.0",
			};

			// Эти настройки нужны для календаря Mac, чтобы он был неотличим от 
			// оригинального календаря (т.е. созданного внутри Mac Calendar)
			CalForSchedule.AddProperty("CALSCALE", "GREGORIAN");
			CalForSchedule.AddProperty("X-WR-CALNAME", "Расписание");
			CalForSchedule.AddProperty("X-WR-TIMEZONE", "Europe/Moscow");
			CalForSchedule.AddLocalTimeZone();


			// Сохраняем дату начала первой учебной недели.
			//TODO тут какое-то говно с преобразованием iCalDateTime в IDateTime
			int numberOfFirstDayOfFirstStudyWeek = startStudy.DayOfYear - ParseExcelSchedule.GetIntNumberFromDayWeek(startStudy.DayOfWeek.ToString());
			iCalDateTime firstDayOfFirstStudyWeek = new iCalDateTime(startStudy.FirstDayOfYear.AddDays(numberOfFirstDayOfFirstStudyWeek));



			// Пробегаемся по всем учебным дням в неделе.
			foreach (WorkDay workDay in week)
			{
				// Информация для отладки.
				Console.WriteLine(workDay);

				// Плюсуем к понедельнику первой учебной недели номер нашего обрабатываемого дня
				iCalDateTime tmpDate = new iCalDateTime(firstDayOfFirstStudyWeek.AddDays(workDay.dayNumber - 1));

				// Плюсуем к временной дате (номер недели - 1, т.к. чтобы перейти
				// к первой неделе не нужно плюсовать неделю) * 7 дней) и
				// приводим к локальной временной зоне.
				//
				// FIXME
				// Для второго семестра приходится минусовать 24 недели
				//iCalDateTime StartClass = new iCalDateTime(tmpDate.AddDays((number - 1 - 23) * 7).Local);

				iCalDateTime StartClass = new iCalDateTime(tmpDate.AddDays((workDay.repeatAt[0] - 1) * 7).Local);



				// Если неделя первая (подразумевается, что она не полная)
				// и день занятий раньше для начала учебы, тогда не записываем его.
				//if ((1 == 1
				//	&& StartClass.LessThan(startStudy))
				//	||
				//	(StartClass.GreaterThanOrEqual(new iCalDateTime(startStudy.FirstDayOfYear.AddDays(363)))
				//	&& !(isLeapYear(StartClass.Year)))
				//	||
				//	(StartClass.GreaterThanOrEqual(new iCalDateTime(startStudy.FirstDayOfYear.AddDays(364)))
				//	&& isLeapYear(StartClass.Year)))
				//	continue;

				Event newClass = CalForSchedule.Create<Event>();



				newClass.DTStart = StartClass; // OLD: StartClass
				newClass.DTStart = newClass.DTStart.Add(workDay.timeClassStart);
				newClass.Duration = workDay.timeClassEnd - workDay.timeClassStart;
				if (workDay.typeClass == "Семинар")
					newClass.Summary = Emojione.Emojione.ShortnameToUnicode(":closed_book:");
				else
					newClass.Summary = Emojione.Emojione.ShortnameToUnicode(":green_book:");
				newClass.Summary += string.Format("{0}", workDay.nameSubject);
				newClass.Description = string.Format("Преподаватель: {0}", workDay.nameLecturer);
				newClass.Location = string.Format("{0}, {1}", workDay.typeClass, workDay.place);
				newClass.IsAllDay = false;
				RecurrencePattern rp = new RecurrencePattern("RRULE:FREQ=WEEKLY;INTERVAL=1;COUNT=" + (workDay.repeatAt.Max() - workDay.repeatAt.Min() + 1));
				newClass.RecurrenceRules.Add(rp);
				//event.RecurrenceRules.Add(rp);
				//newClass.AddProperty("RRULE", @"FREQ = WEEKLY; INTERVAL = 1; COUNT = " + ();



				// Добавим напоминание к парам, чтобы не забыть о них.
				Alarm alarm = new Alarm();
				alarm.Trigger = new Trigger(TimeSpan.FromMinutes(-5));
				alarm.Description = "Напоминание о событии";
				alarm.AddProperty("ACTION", "DISPLAY");
				newClass.Alarms.Add(alarm);



				// Если это первая пара за день, напоминаем о ней за 2,5 часа.
				//if (workDay.isFirstClassesOfADay)
				//{
				//	Alarm alarm2 = new Alarm();
				//	alarm2.Trigger = new Trigger(TimeSpan.FromMinutes(-150));
				//	alarm2.Description = "Напоминание о событии";
				//	alarm2.AddProperty("ACTION", "DISPLAY");
				//	newClass.Alarms.Add(alarm2);
				//}
			}


			// Сериализуем наш календарь.
			iCalendarSerializer serializer = new iCalendarSerializer();
			serializer.Serialize(CalForSchedule, fileCalcName + @".ics");
			Console.WriteLine("Календарь расписания сохранён успешно" + Environment.NewLine);
		}



		/// <summary>
		/// Високосный год или нет.
		/// </summary>
		/// <returns> Вернет true, если год високосный. </returns>
		/// <param name="yearNumber">Номер года.</param>
		public static bool isLeapYear (int yearNumber)
		{
			if (yearNumber % 400 == 0 || yearNumber % 4 == 0 && yearNumber % 100 != 0)
				return true;
			else
				return false;
		}
	}
}
