using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using TaskWatcher.Common;
using TaskWatcher.Import;

namespace TaskWatcher.Console
{
    class CommandProcessor
    {
        private readonly TaskManager _taskManager;
        private readonly RepositoryManager _repoManager;
        private readonly StateManager _stateManager;
        private readonly TextWriter _output;
        private readonly List<object> _commandObjects;
        private readonly Dictionary<string, Command> _commands;
        private readonly string _ident;

        public CommandProcessor(TaskManager taskManager, RepositoryManager repoManager, StateManager stateManager)
            : this(taskManager, repoManager, stateManager, System.Console.Out)
        {
        }

        public CommandProcessor(TaskManager taskManager, RepositoryManager repoManager, StateManager stateManager, TextWriter output)
        {
            _taskManager = taskManager;
            _repoManager = repoManager;
            _stateManager = stateManager;
            _output = output;
            _commandObjects = new List<object>();
            _commands = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);
            _ident = "    ";
        }

        public void AddCommand(Command cmd)
        {
            if (_commands.ContainsKey(cmd.Name))
            {
                string message = String.Format("Command '{0}' is already defined as '{1}'", cmd.Name, cmd.Help);
                throw new InvalidOperationException(message);
            }
            _commands[cmd.Name] = cmd;
        }

        public void AddCommands(IEnumerable<Command> cmds)
        {
            foreach (Command command in cmds)
            {
                AddCommand(command);
            }
        }

        public void AddCommandsObject(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            _commandObjects.Add(obj);
            AddCommands(CommandBuilder.ObjectToCommands(obj));
        }

        public bool Run(string[] args)
        {
            if (args.Length < 1)
            {
                PrintUsage();
                return false;
            }

            Command command;
            if (!_commands.TryGetValue(args[0], out command))
            {
                _output.WriteLine("Unknown command '{0}'", args[0]);
                PrintUsage();
                return false;
            }

            try
            {
                command.Proc(args.Skip(1).ToArray());
                switch (command.Type)
                {
                    case CommandType.Task:
                        ListNotDoneTasks();
                        if (_repoManager.HasRepository(_taskManager.RepositoryName))
                        {
                            Repository repository = _repoManager.GetRepository(_taskManager.RepositoryName);
                            TaskStore.SaveTaskManager(repository, _taskManager);
                        }
                        break;

                    case CommandType.Repository:
                        ListRepositories();
                        TaskStore.SaveRepositoryManager(_repoManager);
                        break;

                    case CommandType.State:
                        ListStates();
                        TaskStore.SaveStateManager(_stateManager);
                        break;
                }
                return true;
            }
            catch (Exception ex)
            {
                _output.WriteLine("Error executing '{0}':", command.Name);
                _output.WriteLine("{0}{1}", _ident, ex.Message);
                return false;
            }
        }

        [Command(CommandType.CmdProcessor, "help", Help = "Prints command usage or list of all commands")]
        public void CmdHelp(string cmd = null)
        {
            if (!String.IsNullOrEmpty(cmd))
            {
                Command command;
                if (_commands.TryGetValue(cmd, out command))
                {
                    PrintCommandUsage(command);
                    return;
                }
                _output.WriteLine("Unknown command '{0}'", cmd);
            }
            PrintUsage();
        }

        [Command(CommandType.CmdProcessor, "list", Help = "Lists a task, or all not done tasks")]
        public void ListNotDoneTasks(int taskIndex = 0)
        {
            if (taskIndex > 0)
            {
                TaskItem taskItem = _taskManager.GetByIndex(taskIndex);
                PrintTasks(new[] { taskItem }, e => e);
            }
            else
            {
                PrintTasks(_taskManager.Tasks, e => e.ExcludeByTag(TaskTag.Done));
            }
        }

        [Command(CommandType.CmdProcessor, "listall", Help = "Lists all tasks")]
        public void ListAllTasks()
        {
            PrintTasks(_taskManager.Tasks, e => e);
        }

        [Command(CommandType.CmdProcessor, "repos", Help = "Lists available repositories")]
        public void ListRepositories()
        {
            PrintRepositories();
        }

        [Command(CommandType.CmdProcessor, "states", Help = "Lists states")]
        public void ListStates()
        {
            PrintStates();
        }

        [Command(CommandType.CmdProcessor, "hist", Help = "Lists task state history")]
        public void ListStateHistory(int taskIndex)
        {
            TaskItem task = _taskManager.GetByIndex(taskIndex);
            var printer = new TaskPrinter(_output, _ident);
            printer.PrintTaskStateHistory(task);
        }

        [Command(CommandType.CmdProcessor, "tags", Help = "Lists task tags")]
        public void ListTags(int taskIndex)
        {
            TaskItem task = _taskManager.GetByIndex(taskIndex);
            var printer = new TaskPrinter(_output, _ident);
            printer.PrintTaskTags(task);
        }

        [Command(CommandType.CmdProcessor, "whatnext", Help = "Lists available next states for task")]
        public void ListNextStatesHistory(int taskIndex)
        {
            TaskItem task = _taskManager.GetByIndex(taskIndex);
            var states = new List<string>();
            if (task.StatesHistory == null || task.StatesHistory.Count < 1)
            {
                states.Add(_stateManager.OpenState.Name);
            }
            else
            {
                State st = _stateManager.GetState(task.StatesHistory.OrderByDescending(s => s.TimeStamp).First().State);
                states.AddRange(st.NextStates);
            }
            var printer = new TaskPrinter(_output, _ident);
            printer.PrintTask(task, e => Enumerable.Empty<TaskItem>());
            _output.WriteLine();
            _output.WriteLine("Available next states - {0}", String.Join(", ", states));
        }

        [Command(CommandType.CmdProcessor, "repoconv1", Help = "Converts v1 repository")]
        public void ConvertVer1Repository(string repoName)
        {
            Repository repository = _repoManager.GetRepository(repoName);
            TaskManager taskManager = _taskManager.RepositoryName == repoName
                                          ? _taskManager
                                          : new TaskManager(Enumerable.Empty<TaskItem>(), repository.Name);

            var importerFactory = new ImporterFactory();
            IImporter importer = importerFactory.CreateVer1Importer();
            importer.ImportFromFileToTaskManager(repository.Path, taskManager);

            TaskStore.SaveTaskManager(repository, taskManager);

            _output.WriteLine("Repository '{0}' converted", repository.Name);
        }

        [Command(CommandType.CmdProcessor, "export", Help = "Exports task to target repository")]
        public void ExportTask(int taskIndex, string repoName)
        {
            TaskItem task = _taskManager.GetByIndex(taskIndex);
            Repository targetRepo = _repoManager.GetRepository(repoName);
            if (_taskManager.RepositoryName == targetRepo.Name)
            {
                throw new InvalidOperationException("Can't export task to the same repository");
            }

            TaskManager targetTaskManager = TaskStore.LoadTaskManager(targetRepo);
            targetTaskManager.Import(task);
            TaskStore.SaveTaskManager(targetRepo, targetTaskManager);

            _output.WriteLine("Task {0}:'{1}' exported", task.Index, task.Name);
        }

        private void PrintTasks(IEnumerable<TaskItem> tasks, Func<IEnumerable<TaskItem>, IEnumerable<TaskItem>> filter)
        {
            _output.WriteLine("'{0}' repository", _taskManager.RepositoryName);
            var printer = new TaskPrinter(_output, _ident);
            printer.PrintAllByPriority(tasks, filter);
        }

        private void PrintRepositories()
        {
            foreach (var repository in _repoManager.Repositories)
            {
                _output.WriteLine("{0,-10} - {1}", repository.Name, repository.Path);
            }
            _output.WriteLine();
            _output.WriteLine("Current repository: {0}", _repoManager.CurrentRepository.Name);
        }

        private void PrintStates()
        {
            _output.WriteLine("{0} state(s):", _stateManager.States.Count);
            foreach (State state in _stateManager.States)
            {
                _output.WriteLine("{0} -> {1}", state.Name, String.Join(",", state.NextStates));
            }
        }

        private void PrintCommandUsage(Command command)
        {
            _output.WriteLine("{0} - {1}", command.Name, command.Help);
            _output.WriteLine("{0}Usage: <tw> {1} {2}", _ident, command.Name, command.UsageArgs);
        }

        public void PrintUsage()
        {
            _output.WriteLine("Usage: tw <cmd> [args], where cmd:");

            foreach (Command cmd in _commands.Values.OrderBy(cmd => cmd.Name))
            {
                _output.WriteLine("{0}{1,-12} - {2}", _ident, cmd.Name, cmd.Help);
            }
        }
    }
}
