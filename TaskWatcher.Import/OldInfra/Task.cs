using System;
using System.Collections.Generic;

namespace TaskWatcher.Import.OldInfra
{
    class Task
    {
        public int Index { get; set; }
        public string Text { get; set; }
        public string Notes { get; set; }
        public int Priority { get; set; }
        public bool Done { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastEdited { get; set; }
        public List<SubTask> SubTasks { get; set; }
        public int QueueWeight { get; set; }
    }
}
