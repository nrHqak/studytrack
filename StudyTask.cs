namespace StudyTrack
{
    /// <summary>
    /// Модель учебной задачи (тема) для трекинга прогресса.
    /// </summary>
    public class StudyTask
    {
        public string Title { get; set; }
        public bool IsCompleted { get; set; }
        public string Subject { get; set; }

        /// <summary>
        /// Создаёт экземпляр учебной задачи.
        /// </summary>
        public StudyTask(string title, bool isCompleted, string subject)
        {
            Title = title;
            IsCompleted = isCompleted;
            Subject = subject;
        }

        /// <summary>
        /// Возвращает строку вида "Предмет: Тема" для отображения в интерфейсе.
        /// </summary>
        public override string ToString()
        {
            return Subject + ": " + Title;
        }
    }
}
