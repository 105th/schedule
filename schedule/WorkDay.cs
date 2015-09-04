using System;
using System.Collections.Generic;

namespace schedule
{
	public class WorkDay
	{
		/// <summary>
		/// День недели.
		/// </summary>
		public int dayNumber
		{
			get;
			set;
		}

		/// <summary>
		/// Время начало учебной пары
		/// </summary>
		public TimeSpan timeClassStart
		{
			get;
			set;
		}

		/// <summary>
		/// Время начало учебной пары
		/// </summary>
		public TimeSpan timeClassEnd
		{
			get;
			set;
		}

		/// <summary>
		/// Название предмета
		/// </summary>
		public string nameSubject
		{
			get;
			set;
		}

		/// <summary>
		/// Первая ли пара в дне или нет.
		/// </summary>
		/// <value><c>true</c> если это первое занятие в этом дне; 
		/// иначе, <c>false</c>.</value>
		public bool isFirstClassesOfADay
		{
			get;
			set;
		}

		/// <summary>
		/// Преподаватель
		/// </summary>
		/// <value>Имя преподавателя.</value>
		public string nameLecturer
		{
			get;
			set;
		}

		/// <summary>
		/// Тип занятий (лекция, семинар)
		/// </summary>
		public string typeClass
		{
			get;
			set;
		}

		/// <summary>
		/// По каким неделям повторяется
		/// </summary>
		public List<int> repeatAt
		{
			get;
			set;
		}

		/// <summary>
		/// Место
		/// </summary>
		public string place
		{
			get;
			set;
		}

		/// <summary>
		/// Создает новый экземляр <see cref="schedule.WorkDay"/> class.
		/// </summary>
		public WorkDay()
		{
			repeatAt = new List<int>();
			isFirstClassesOfADay = false;
		}

		public override string ToString()
		{
			string weeksNumbers = "";
			repeatAt.ForEach(item =>
				{
					weeksNumbers += item + ", ";
				});
			
			string isFirstClasses = (isFirstClassesOfADay == true) ? "Да" : "Нет";

			return string.Format("{0}: Предмет - {1}, преподаватель - {2}, " +
				"время: {3} - {4}, {5}, первая пара за день: {6}" +
				" повторяется по неделям, под номерами: {7}, в {8}",
				dayNumber, nameSubject, nameLecturer, timeClassStart,
				timeClassEnd, typeClass, isFirstClasses, weeksNumbers, place);
		}
	}
}

