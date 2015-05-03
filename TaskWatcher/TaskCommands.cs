using TaskWatcher.Common;

namespace TaskWatcher.Console
{
    class TaskCommands
    {
        public TaskManager Manager { get; set; }

        public TaskCommands(TaskManager manager)
        {
            Manager = manager;
        }

        [Command(CommandType.Task, "add", Help = "Creates task with default priority")]
        public void CreateTask(string name)
        {
            Manager.Create(name);
        }

        [Command(CommandType.Task, "addp", Help = "Creates task with specified priority")]
        public void CreatePrioritizedTask(string name, int priority)
        {
            Manager.Create(name, priority);
        }

        [Command(CommandType.Task, "addsub", Help = "Creates sub task with default priority")]
        public void CreateSubTask(int toIndex, string name)
        {
            TaskItem task = Manager.Create(name);
            Manager.AttachTo(task.Index, toIndex);
        }

        [Command(CommandType.Task, "addtop", Help = "Creates task with top priority")]
        public void CreateTopTask(string name)
        {
            Manager.Create(name, TaskPriority.Top);
        }

        [Command(CommandType.Task, "addlast", Help = "Creates task with last priority")]
        public void CreateLastTask(string name)
        {
            Manager.Create(name, TaskPriority.Last);
        }

        [Command(CommandType.Task, "detach", Help = "Detaches task")]
        public void DetachTask(int taskIndex)
        {
            Manager.Detach(taskIndex);
        }

        [Command(CommandType.Task, "attach", Help = "Attaches task to another one")]
        public void AttachTaskToParent(int taskIndex, int parentIndex)
        {
            Manager.AttachTo(taskIndex, parentIndex);
        }

        [Command(CommandType.Task, "delete", Help = "Deletes task")]
        public void DeleteTask(int taskIndex)
        {
            Manager.Delete(taskIndex);
        }

        [Command(CommandType.Task, "name", Help = "Edits name for task")]
        public void EditTaskName(int taskIndex, string name)
        {
            Manager.SetName(taskIndex, name);
        }

        [Command(CommandType.Task, "done", Help = "Marks task as done")]
        public void AddDoneTagToTask(int taskIndex)
        {
            TaskItem item = Manager.AddTag(taskIndex, TaskTag.Done);
            foreach (TaskItem subTask in item.SubTasks)
            {
                AddDoneTagToTask(subTask.Index);
            }
        }

        [Command(CommandType.Task, "undone", Help = "Removes done mark from task")]
        public void RemoveDoneTagFromTask(int taskIndex)
        {
            Manager.RemoveTag(taskIndex, TaskTag.Done);
        }

        [Command(CommandType.Task, "tag+", Help = "Adds custom tag to task")]
        public void AddTagToTask(int taskIndex, string tag)
        {
            Manager.AddTag(taskIndex, tag);
        }

        [Command(CommandType.Task, "tag-", Help = "Removes tag from task")]
        public void RemoveTagFromTask(int taskIndex, string tag)
        {
            Manager.RemoveTag(taskIndex, tag);
        }

        [Command(CommandType.Task, "p+", Help = "Rises task priority")]
        public void RiseTaskPriority(int taskIndex)
        {
            Manager.RisePriority(taskIndex);
        }

        [Command(CommandType.Task, "p-", Help = "Drops task priority")]
        public void DropTaskPriority(int taskIndex)
        {
            Manager.DropPriority(taskIndex);
        }
    }
}
