using System;
using System.Collections.Generic;
using System.IO;

namespace StudyTrack
{
    /// <summary>
    /// Работа с файловым хранилищем задач (tasks.txt).
    /// </summary>
    public class FileManager
    {
        private readonly string _path;

        /// <summary>
        /// Создаёт менеджер файлов для заданного пути.
        /// </summary>
        public FileManager(string path)
        {
            _path = path;
        }

        /// <summary>
        /// Сохраняет задачи в текстовый файл через StreamWriter.
        /// </summary>
        public void SaveTasks(List<StudyTask> tasks)
        {
            using (StreamWriter writer = new StreamWriter(_path, false))
            {
                foreach (StudyTask task in tasks)
                {
                    writer.WriteLine(task.Subject + "|" + task.Title + "|" + task.IsCompleted);
                }
            }
        }

        /// <summary>
        /// Загружает задачи из файла через StreamReader.
        /// Обрабатывает отсутствие файла через FileNotFoundException.
        /// </summary>
        public List<StudyTask> LoadTasks()
        {
            List<StudyTask> tasks = new List<StudyTask>();

            try
            {
                using (StreamReader reader = new StreamReader(_path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        string[] parts = line.Split('|');
                        if (parts.Length >= 3)
                        {
                            bool isCompleted = false;
                            bool.TryParse(parts[2], out isCompleted);
                            tasks.Add(new StudyTask(parts[1], isCompleted, parts[0]));
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                // Файл ещё не создан — это нормальная ситуация при первом запуске.
                return tasks;
            }

            return tasks;
        }
    }
}
