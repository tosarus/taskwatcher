using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using TaskWatcher.Common;

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
                        PrintTasks();
                        break;

                    case CommandType.Repository:
                        PrintRepositories();
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

        [Command(CommandType.Usage, "help", Help = "Prints command usage or list of all commands")]
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

        private void PrintTasks()
        {
            _output.WriteLine("'{0}' repository", _taskManager.RepositoryName);
            var printer = new TaskPrinter(_output, _ident);
            printer.PrintAllByPriority(_taskManager.Tasks, e => e.ExcludeByTag(TaskTag.Done));
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
            _output.WriteLine("{0}Usage: tw <{1}> {2}", _ident, command.Name, command.UsageArgs);
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
