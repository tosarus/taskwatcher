using System.Collections.Generic;
using System.Linq;

namespace TaskWatcher.Common
{
    public static class TaskExtensions
    {
        public static TaskItem SetPriority(this TaskManager tm, int idex, int priority)
        {
            return tm.Edit(idex, t => t.Priority = priority);
        }

        public static TaskItem SetName(this TaskManager tm, int index, string name)
        {
            return tm.Edit(index, t => t.Name = name);
        }

        public static TaskItem AddTag(this TaskManager tm, int index, string tag)
        {
            return tm.Edit(index, t => t.Tags.Add(tag));
        }

        public static TaskItem RemoveTag(this TaskManager tm, int index, string tag)
        {
            return tm.Edit(index, t => t.Tags.Remove(tag));
        }

        public static TaskItem RisePriority(this TaskManager tm, int index)
        {
            return tm.Edit(index, t => --t.Priority);
        }

        public static TaskItem DropPriority(this TaskManager tm, int index)
        {
            return tm.Edit(index, t => ++t.Priority);
        }

        public static IEnumerable<TaskItem> AllSorted(this IEnumerable<TaskItem> tasks)
        {
            return tasks.OrderBy(t => t.Priority)
                        .ThenBy(t => t.Index);
        }

        public static IEnumerable<TaskItem> IncludeByTag(this IEnumerable<TaskItem> tasks, string tag)
        {
            return tasks.Where(t => t.Tags.Contains(tag));
        }

        public static IEnumerable<TaskItem> ExcludeByTag(this IEnumerable<TaskItem> tasks, string tag)
        {
            return tasks.Where(t => !t.Tags.Contains(tag));
        }
    }
}
