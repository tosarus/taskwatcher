using System;
using System.Collections.Generic;
using System.Linq;

using TaskWatcher.Common;

namespace TaskWatcher.Console
{
    class TaskCommands
    {
        private readonly TaskManager _manager;
        private readonly StateManager _stateManager;

        public TaskCommands(TaskManager manager, StateManager stateManager)
        {
            _manager = manager;
            _stateManager = stateManager;
        }

        [Command(CommandType.Task, "add", Help = "Creates task with default priority")]
        public void CreateTask(string name)
        {
            _manager.Create(name);
        }

        [Command(CommandType.Task, "addp", Help = "Creates task with specified priority")]
        public void CreatePrioritizedTask(string name, int priority)
        {
            _manager.Create(name, priority);
        }

        [Command(CommandType.Task, "addsub", Help = "Creates sub task with default priority")]
        public void CreateSubTask(int toIndex, string name)
        {
            TaskItem task = _manager.Create(name);
            _manager.AttachTo(task.Index, toIndex);
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

        [Command(CommandType.Task, "openst", Help = "Adds 'open' state")]
        public void AddOpenState(int taskIndex, string notes = null)
        {
            TaskItem task = _manager.GetByIndex(taskIndex);
            if (task.StatesHistory != null && task.StatesHistory.Count > 0)
            {
                throw new InvalidOperationException("Task already has a state");
            }

            State openState = _stateManager.OpenState;
            task.StatesHistory = new List<StateItem> {
                                                         new StateItem {
                                                                           State = openState.Name,
                                                                           TimeStamp = DateTime.Now,
                                                                           Notes = notes
                                                                       }
                                                     };
        }

        [Command(CommandType.Task, "clearst", Help = "Clears state history")]
        public void ClearState(int taskIndex)
        {
            TaskItem task = _manager.GetByIndex(taskIndex);
            task.StatesHistory = null;
        }

        [Command(CommandType.Task, "notest", Help = "Sets note to current state")]
        public void AddStateNote(int taskIndex, string notes)
        {
            TaskItem task = _manager.GetByIndex(taskIndex);
            if (task.StatesHistory == null || task.StatesHistory.Count < 1)
            {
                throw new InvalidOperationException("Task doesn't have a state");
            }
            StateItem currentState = task.StatesHistory.OrderByDescending(s => s.TimeStamp).FirstOrDefault();
            currentState.Notes = notes;
        }

        [Command(CommandType.Task, "nextst", Help = "Sets next state")]
        public void MoveToNextState(int taskIndex, string nextState, string notes = null)
        {
            TaskItem task = _manager.GetByIndex(taskIndex);
            if (task.StatesHistory == null || task.StatesHistory.Count < 1)
            {
                throw new InvalidOperationException("Task doesn't have a state");
            }
            StateItem currentState = task.StatesHistory.OrderByDescending(s => s.TimeStamp).FirstOrDefault();
            State next = _stateManager.MoveToNext(currentState.State, nextState);
            task.StatesHistory.Add(new StateItem { State = next.Name, TimeStamp = DateTime.Now, Notes = notes });
        }
    }
}
