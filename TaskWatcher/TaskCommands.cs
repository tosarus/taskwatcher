using TaskWatcher.Common;

namespace TaskWatcher.Console
{
    class TaskCommands
    {
        private readonly TaskManager _manager;

        public TaskCommands(TaskManager manager)
        {
            _manager = manager;
        }

        [Command(CommandType.Task, "add", Help = "Creates task with specified or default priority")]
        public void CreateTask(string name, int priority = TaskPriority.Default)
        {
            _manager.Create(name, priority);
        }

        [Command(CommandType.Task, "addsub", Help = "Creates sub task with default priority")]
        public void CreateSubTask(int toIndex, string name)
        {
            TaskItem task = _manager.Create(name);
            _manager.AttachTo(task.Index, toIndex);
        }

        [Command(CommandType.Task, "addtop", Help = "Creates task with top priority")]
        public void CreateTopTask(string name)
        {
            _manager.Create(name, TaskPriority.Top);
        }

        [Command(CommandType.Task, "addlast", Help = "Creates task with last priority")]
        public void CreateLastTask(string name)
        {
            _manager.Create(name, TaskPriority.Last);
        }

        [Command(CommandType.Task, "detach", Help = "Detaches task")]
        public void DetachTask(int taskIndex)
        {
            _manager.Detach(taskIndex);
        }

        [Command(CommandType.Task, "attach", Help = "Attaches task to another one")]
        public void AttachTaskToParent(int taskIndex, int parentIndex)
        {
            _manager.AttachTo(taskIndex, parentIndex);
        }

        [Command(CommandType.Task, "delete", Help = "Deletes task")]
        public void DeleteTask(int taskIndex)
        {
            _manager.Delete(taskIndex);
        }

        [Command(CommandType.Task, "name", Help = "Edits name for task")]
        public void EditTaskName(int taskIndex, string name)
        {
            _manager.SetName(taskIndex, name);
        }

        [Command(CommandType.Task, "done", Help = "Marks task as done")]
        public void AddDoneTagToTask(int taskIndex)
        {
            TaskItem item = _manager.AddTag(taskIndex, TaskTag.Done);
            foreach (TaskItem subTask in item.SubTasks)
            {
                AddDoneTagToTask(subTask.Index);
            }
        }

        [Command(CommandType.Task, "undone", Help = "Removes done mark from task")]
        public void RemoveDoneTagFromTask(int taskIndex)
        {
            _manager.RemoveTag(taskIndex, TaskTag.Done);
        }

        [Command(CommandType.Task, "tag+", Help = "Adds custom tag to task")]
        public void AddTagToTask(int taskIndex, string tag)
        {
            _manager.AddTag(taskIndex, tag);
        }

        [Command(CommandType.Task, "tag-", Help = "Removes tag from task")]
        public void RemoveTagFromTask(int taskIndex, string tag)
        {
            _manager.RemoveTag(taskIndex, tag);
        }

        [Command(CommandType.Task, "p+", Help = "Rises task priority")]
        public void RiseTaskPriority(int taskIndex)
        {
            _manager.RisePriority(taskIndex);
        }

        [Command(CommandType.Task, "p-", Help = "Drops task priority")]
        public void DropTaskPriority(int taskIndex)
        {
            _manager.DropPriority(taskIndex);
        }
    }
}
