using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskWatcher.Common
{
    public class TaskPrinter
    {
        public TaskPrinter()
            : this(Console.Out)
        {
        }

        public TextWriter Output { get; private set; }
        public string Ident { get; private set; }

        public TaskPrinter(TextWriter output, string ident = "    ")
        {
            Output = output;
            Ident = ident;
        }

        public void PrintAllByPriority(IEnumerable<TaskItem> tasks, Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> filter)
        {
            PrintInternal(tasks.AllSorted(), filter, "");
        }

        public void PrintInternal(TaskItem task, Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> filter, string ident = "")
        {
            Output.WriteLine("{0}{1}", ident, ToString(task));
            PrintInternal(task.SubTasks.AllSorted(), filter, ident + Ident);
        }

        private void PrintInternal(IEnumerable<TaskItem> tasks, Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> filter, string ident)
        {
            foreach (TaskItem taskItem in filter(tasks))
            {
                PrintInternal(taskItem, filter, ident);
            }
        }

        private static string ToString(TaskItem task)
        {
            bool isDone = task.Tags.Contains(TaskTag.Done);
            return String.Format("<{0}>{1}{2,2} {3}", task.Priority, isDone ? "+" : " ", task.Index, task.Name);
        }
    }
}
