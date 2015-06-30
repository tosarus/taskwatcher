using System;

namespace TaskWatcher.Console
{
    enum CommandType
    {
        Repository,
        Task,
        State,
        Other,
        //Queue,
    }

    class CommandAttribute : Attribute
    {
        public CommandAttribute(CommandType type, string name)
        {
            Type = type;
            Name = name;
        }

        public CommandType Type { get; set; }
        public string Name { get; set; }
        public string Help { get; set; }
    }

    class Command
    {
        public CommandType Type { get; set; }
        public string Name { get; set; }
        public string Help { get; set; }
        public string UsageArgs { get; set; }

        public Action<string[]> Proc { get; set; }
    }
}
