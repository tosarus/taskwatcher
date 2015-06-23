using System;
using System.Collections.Generic;

namespace TaskWatcher.Common
{
    public class TaskItem
    {
        private int _priority;

        public int Index { get; set; }
        public int Priority
        {
            get
            {
                return _priority;
            }
            set
            {
                _priority = Math.Max(TaskPriority.Top, Math.Min(value, TaskPriority.Last));
            }
        }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastEdited { get; set; }
        public Dictionary<string, DateTime> Tags { get; set; }
        public List<TaskItem> SubTasks { get; set; }
        public List<StateItem> StatesHistory { get; set; }
    }
}
