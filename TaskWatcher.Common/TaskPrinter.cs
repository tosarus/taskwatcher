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

        private void PrintInternal(IEnumerable<TaskItem> tasks, Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> filter, string ident)
        {
            foreach (TaskItem taskItem in filter(tasks))
            {
                PrintInternal(taskItem, filter, ident);
            }
        }

        public void PrintInternal(TaskItem task, Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> filter, string ident = "")
        {
            Output.WriteLine("{0}{1}", ident, ToString(task));
            if (task.StatesHistory != null)
            {
                StateItem state = task.StatesHistory.OrderByDescending(s => s.TimeStamp).FirstOrDefault();
                if (state != null)
                {
                    Output.WriteLine("{0}    {1}", ident, ToString(state));
                }
            }
            PrintInternal(task.SubTasks.AllSorted(), filter, ident + Ident);
        }

        public void PrintTaskStateHistory(TaskItem task)
        {
            Output.WriteLine(ToString(task));
            if (task.StatesHistory != null)
            {
                foreach (StateItem stateItem in task.StatesHistory.OrderBy(s => s.TimeStamp))
                {
                    Output.WriteLine(ToString(stateItem));
                }
            }
        }

        public void PrintTaskTags(TaskItem task)
        {
            Output.WriteLine(ToString(task));
            if (task.Tags != null)
            {
                foreach (KeyValuePair<string, DateTime> tag in task.Tags)
                {
                    Output.WriteLine("{0} - {1}", tag.Value, tag.Key);
                }
            }
        }

        private static string ToString(TaskItem task)
        {
            bool isDone = task.Tags.ContainsKey(TaskTag.Done);
            return String.Format("<{0}>{1}{2,3} {3}", task.Priority, isDone ? "+" : " ", task.Index, task.Name);
        }

        private static string ToString(StateItem state)
        {
            return String.Format("{0:u}: {1,-10} {2}", state.TimeStamp, state.State, state.Notes);
        }
    }
}
