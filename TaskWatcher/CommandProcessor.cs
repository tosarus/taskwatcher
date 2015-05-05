﻿using System;
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
        private readonly TextWriter _output;
        private readonly List<object> _commandObjects;
        private readonly Dictionary<string, Command> _commands;
        private readonly string _ident;

        public CommandProcessor(TaskManager taskManager, RepositoryManager repoManager)
            : this(taskManager, repoManager, System.Console.Out)
        {
            _repoManager = repoManager;
        }

        public CommandProcessor(TaskManager taskManager, RepositoryManager repoManager, TextWriter output)
        {
            _taskManager = taskManager;
            _repoManager = repoManager;
            _output = output;
            _commandObjects = new List<object>();
            _commands = new Dictionary<string, Command>(StringComparer.InvariantCultureIgnoreCase);
            _ident = "    ";
        }

        public void AddCommand(Command cmd)
        {
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

        private void PrintCommandUsage(Command command)
        {
            _output.WriteLine("{0} - {1}", command.Name, command.Help);
            _output.WriteLine("{0}Usage: <tw> {1} {2}", _ident, command.Name, command.UsageArgs);
        }

        public void PrintUsage()
        {
            _output.WriteLine("Usage: tw <cmd> [args], Where cmds:");

            foreach (Command cmd in _commands.Values.OrderBy(cmd => cmd.Name))
            {
                _output.WriteLine("{0}{1,-8} - {2}", _ident, cmd.Name, cmd.Help);
            }
        }
    }
}
