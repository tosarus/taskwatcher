using System.Collections.Generic;

using TaskWatcher.Common;

namespace TaskWatcher.Import.Ver1
{
    class Ver1Importer : ImporterImpl<Task>
    {
        public override void ConvertTasks(ICollection<Task> oldTasks, TaskManager taskManager)
        {
            ConvertTasks(null, oldTasks, taskManager);
        }

        void ConvertTasks(TaskItem parentTask, IEnumerable<Task> oldTasks, TaskManager taskManager)
        {
            foreach (Task oldTask in oldTasks)
            {
                TaskItem task = taskManager.Create(oldTask.Name, oldTask.Priority);
                foreach (string tag in oldTask.Tags)
                {
                    taskManager.AddTag(task.Index, tag);
                }
                if (oldTask.SubTasks.Count > 0)
                {
                    ConvertTasks(task, oldTask.SubTasks, taskManager);
                }
                if (parentTask != null)
                {
                    taskManager.AttachTo(task.Index, parentTask.Index);
                }
            }
        }
    }
}
