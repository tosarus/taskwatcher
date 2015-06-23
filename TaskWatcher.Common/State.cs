using System.Collections.Generic;
using System.Linq;

namespace TaskWatcher.Common
{
    public class State
    {
        public State(string name, IEnumerable<string> nextStates)
        {
            Name = name;
            NextStates = new List<string>(nextStates ?? Enumerable.Empty<string>());
        }

        public string Name { get; private set; }
        public IReadOnlyCollection<string> NextStates { get; private set; }
    }
}
