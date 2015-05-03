using System.Collections.Generic;
using System.Linq;

using TaskWatcher.Common;

namespace TaskWatcher.Import.OldInfra
{
    class OldInfraImporter : ImporterImpl<Task>
    {
        public override void ConvertTasks(ICollection<Task> oldTasks, TaskManager taskManager)
        {
            foreach (Task oldTask in oldTasks.OrderBy(t => t.Index))
            {
                TaskItem newItem = taskManager.Create(name: oldTask.Text, priority: oldTask.Priority);
                if (oldTask.Done)
                {
                    taskManager.AddTag(newItem.Index, TaskTag.Done);
                }

                foreach (SubTask subTask in oldTask.SubTasks.OrderBy(st => st.Index))
                {
                    TaskItem newSubItem = taskManager.Create(subTask.Text);
                    if (subTask.Done)
                    {
                        taskManager.AddTag(newItem.Index, TaskTag.Done);
                    }
                    taskManager.AttachTo(newSubItem.Index, newItem.Index);
                }
            }
        }
    }
}
