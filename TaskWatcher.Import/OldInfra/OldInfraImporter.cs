using System;
using System.Collections.Generic;
using System.Linq;

using TaskWatcher.Common;

namespace TaskWatcher.Import.OldInfra
{
    class OldInfraImporter : IImporter
    {
        public ICollection<TaskItem> ImportFromFile(string fileName)
        {
            var taskManager = new TaskManager(Enumerable.Empty<TaskItem>(), String.Empty);
            ImportFromFileToTaskManager(fileName, taskManager);
            return taskManager.Tasks;
        }

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

            var oldTasks = TaskStore.LoadFromFile<List<Task>>(fileName);
            if (oldTasks == null)
            {
                string message = String.Format("Can't load old tasks from '{0}'", fileName);
                throw new InvalidOperationException(message);
            }

           ConvertTasks(oldTasks, taskManager);
        }

        static void ConvertTasks(IEnumerable<Task> oldTasks, TaskManager taskManager)
        {
            foreach (Task oldTask in oldTasks.OrderBy(t => t.Index))
            {
                TaskItem newItem = taskManager.Create(name: oldTask.Text, priority: oldTask.Priority);
                if (oldTask.Done)
                {
                    newItem.Tags.Add(TaskTag.Done);
                }

                foreach (SubTask subTask in oldTask.SubTasks.OrderBy(st => st.Index))
                {
                    TaskItem newSubItem = taskManager.Create(subTask.Text);
                    if (subTask.Done)
                    {
                        newSubItem.Tags.Add(TaskTag.Done);
                    }
                    taskManager.AttachTo(newSubItem.Index, newItem.Index);
                }
            }
        }
    }
}
