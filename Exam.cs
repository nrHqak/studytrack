using System;

namespace StudyTrack
{
    /// <summary>
    /// Модель экзамена с названием, датой и предметом.
    /// </summary>
    public class Exam
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Subject { get; set; }

        /// <summary>
        /// Создаёт экземпляр экзамена.
        /// </summary>
        public Exam(string name, DateTime date, string subject)
        {
            Name = name;
            Date = date;
            Subject = subject;
        }
    }
}
