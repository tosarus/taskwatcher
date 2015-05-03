using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskWatcher.Import.Ver1
{
    class Task
    {
        public int Index { get; set; }
        public int Priority { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime LastEdited { get; set; }
        public HashSet<string> Tags { get; set; }
        public List<Task> SubTasks { get; set; }
    }
}
