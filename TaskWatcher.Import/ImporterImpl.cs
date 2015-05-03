using System;
using System.Collections.Generic;

using TaskWatcher.Common;

namespace TaskWatcher.Import
{
    abstract class ImporterImpl<T> : IImporter
    {
        public void ImportFromFileToTaskManager(string fileName, TaskManager taskManager)
        {
            if (String.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName can't be null or empty", "fileName");
            }
            if (taskManager == null)
            {
                throw new ArgumentNullException("taskManager");
            }

            var oldTasks = TaskStore.LoadFromFile<List<T>>(fileName);
            if (oldTasks == null)
            {
                string message = String.Format("Can't load old tasks from '{0}'", fileName);
                throw new InvalidOperationException(message);
            }

            ConvertTasks(oldTasks, taskManager);
        }

        public abstract void ConvertTasks(ICollection<T> oldTasks, TaskManager taskManager);
    }
}
