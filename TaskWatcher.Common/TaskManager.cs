using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskWatcher.Common
{
    public class TaskManager
    {
        private readonly Dictionary<int, TaskItem> _tasks;
        private int _nextIndex;

        public TaskManager(IEnumerable<TaskItem> tasks, string repoName)
        {
            RepositoryName = repoName;
            _tasks = (tasks ?? Enumerable.Empty<TaskItem>()).ToDictionary(t => t.Index);
            _nextIndex = GetMaxIndex(Tasks) + 1;
        }

        private int GetMaxIndex(IEnumerable<TaskItem> tasks)
        {
            int maxIndex = 0;
            foreach (TaskItem task in tasks)
            {
                maxIndex = Math.Max(maxIndex, task.Index);
                maxIndex = Math.Max(maxIndex, GetMaxIndex(task.SubTasks));
            }
            return maxIndex;
        }

        public ICollection<TaskItem> Tasks
        {
            get
            {
                return _tasks.Values;
            }
        }

        public string RepositoryName { get; private set; }

        public TaskItem Create(string name, int priority = TaskPriority.Default)
        {
            var timeStamp = DateTime.Now;
            var task = new TaskItem {
                                        Index = _nextIndex++,
                                        Name = name,
                                        Priority = priority,
                                        SubTasks = new List<TaskItem>(),
                                        Tags = new Dictionary<string, DateTime>(StringComparer.InvariantCultureIgnoreCase),
                                        Created = timeStamp,
                                        LastEdited = timeStamp
                                    };
            _tasks.Add(task.Index, task);
            return task;
        }

        public TaskItem Delete(int index)
        {
            TaskItem task = Detach(index);
            _tasks.Remove(index);
            return task;
        }

        public TaskItem GetByIndex(int index)
        {
            TaskItem task = FindInternal(Tasks, t => t.Index == index);
            if (task == null)
            {
                string message = String.Format("No task with index {0}", index);
                throw new KeyNotFoundException(message);
            }
            return task;
        }

        public TaskItem Find(Func<TaskItem, bool> comparer)
        {
            return FindInternal(Tasks, comparer);
        }

        private static TaskItem FindInternal(IEnumerable<TaskItem> tasks, Func<TaskItem, bool> comparer)
        {
            foreach (TaskItem task in tasks)
            {
                if (comparer(task))
                {
                    return task;
                }

                TaskItem subTask = FindInternal(task.SubTasks, comparer);
                if (subTask != null)
                {
                    return subTask;
                }
            }
            return null;
        }

        public TaskItem Edit(int index, Action<TaskItem> editor)
        {
            TaskItem task = GetByIndex(index);
            EditInternal(task, editor);
            return task;
        }

        public void EditInternal(TaskItem task, Action<TaskItem> editor)
        {
            editor(task);
            task.LastEdited = DateTime.Now;
        }

        public TaskItem AttachTo(int index, int indexTo)
        {
            TaskItem task = GetByIndex(index);
            TaskItem parentTask = GetByIndex(indexTo);
            if (task.Index == parentTask.Index)
            {
                throw new InvalidOperationException("Can't attach task to itself");
            }

            if (null != FindInternal(task.SubTasks, t => t.Index == parentTask.Index))
            {
                throw new InvalidOperationException("Can't attach task to subtask");
            }

            DetachInternal(task);
            EditInternal(parentTask, t => t.SubTasks.Add(task));
            _tasks.Remove(task.Index);
            return parentTask;
        }

        public TaskItem Detach(int index)
        {
            TaskItem task = GetByIndex(index);
            DetachInternal(task);
            return task;
        }

        private void DetachInternal(TaskItem task)
        {
            TaskItem parentTask = FindInternal(Tasks, t => t.SubTasks.Contains(task));
            if (parentTask != null)
            {
                EditInternal(parentTask, t => t.SubTasks.Remove(task));
                _tasks[task.Index] = task;
            }
        }

        public void Import(TaskItem newTask)
        {
            ImportInternal(new[] { newTask }, null);
        }

        public void ImportInternal(IEnumerable<TaskItem> tasks, TaskItem parentTask)
        {
            foreach (TaskItem newTask in tasks)
            {
                TaskItem task = Create(newTask.Name, newTask.Priority);
                EditInternal(task, t => t.Tags = newTask.Tags);
                ImportInternal(newTask.SubTasks, task);
                if (parentTask != null)
                {
                    EditInternal(parentTask, t => t.SubTasks.Add(task));
                    _tasks.Remove(task.Index);
                }
            }
        }
    }
}
