using System.Collections.Generic;

namespace StudyTrack
{
    /// <summary>
    /// Сервис расчёта прогресса выполнения задач.
    /// </summary>
    public class ProgressCalculator
    {
        /// <summary>
        /// Считает процент выполненных задач в списке.
        /// </summary>
        public double Calculate(List<StudyTask> tasks)
        {
            if (tasks == null || tasks.Count == 0)
            {
                return 0.0;
            }

            int completed = 0;
            foreach (StudyTask task in tasks)
            {
                if (task.IsCompleted)
                {
                    completed++;
                }
            }

            double value = (double)completed / tasks.Count * 100.0;
            return value;
        }
    }
}
