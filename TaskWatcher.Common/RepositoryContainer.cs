using System.Collections.Generic;

namespace TaskWatcher.Common
{
    internal class RepositoryContainer
    {
        public List<Repository> Repositories { get; set; }
        public string CurrentRepository { get; set; }
    }
}
