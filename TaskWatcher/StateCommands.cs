using TaskWatcher.Common;

namespace TaskWatcher.Console
{
    class StateCommands
    {
        private readonly StateManager _manager;

        public StateCommands(StateManager manager)
        {
            _manager = manager;
        }

        [Command(CommandType.State, "stadd", Help = "Adds new state")]
        public void AddNewState(string stateName)
        {
            _manager.AddState(stateName, null);
        }

        [Command(CommandType.State, "stsetnext", Help = "Sets next existing state")]
        public void AddNextState(string stateName, string nextName)
        {
            _manager.AddNext(stateName, nextName);
        }

        [Command(CommandType.State, "staddnext", Help = "Adds new next state")]
        public void AddNewState(string stateName, string nextName)
        {
            _manager.AddNewNext(stateName, nextName);
        }
    }
}
