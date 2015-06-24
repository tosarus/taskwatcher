using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskWatcher.Common
{
    public class StateManager
    {
        private readonly Dictionary<string, State> _states;

        public StateManager(IEnumerable<State> states)
        {
            _states = new Dictionary<string, State>((states ?? Enumerable.Empty<State>()).ToDictionary(s => s.Name),
                                                    StringComparer.OrdinalIgnoreCase);
        }

        public ICollection<State> States
        {
            get
            {
                return _states.Values;
            }
        }

        public State OpenState
        {
            get
            {
                return _states["open"];
            }
        }

        public State GetState(string stateName)
        {
            if (String.IsNullOrEmpty(stateName))
            {
                throw new ArgumentNullException("stateName");
            }
            State state;
            if (!_states.TryGetValue(stateName, out state))
            {
                string message = String.Format("Unknown state '{0}'", stateName);
                throw new ArgumentException(message);
            }
            return state;
        }

        public State MoveToNext(string stateName, string nextName)
        {
            State state = GetState(stateName);
            State nextState = GetState(nextName);

            if (!state.NextStates.Contains(nextState.Name))
            {
                string message = String.Format("Can move only to one of {0}", String.Join(",", state.NextStates));
                throw new InvalidOperationException(message);
            }

            return nextState;
        }

        public State AddState(string stateName, List<string> nextList)
        {
            nextList = nextList ?? new List<string>();
            if (_states.ContainsKey(stateName))
            {
                string message = String.Format("State '{0}' already defined", stateName);
                throw new ArgumentException(message);
            }

            foreach (string next in nextList)
            {
                GetState(next);
            }

            _states.Add(stateName, new State(stateName, nextList));
            return _states[stateName];
        }

        public State AddNext(string stateName, string nextName)
        {
            State state = GetState(stateName);
            State nextState = GetState(nextName);

            return InternalAddNext(state, nextState);
        }

        public State AddNewNext(string stateName, string nextName)
        {
            State state = GetState(stateName);
            State nextState = AddState(nextName, null);

            return InternalAddNext(state, nextState);
        }

        State InternalAddNext(State state, State nextState)
        {
            _states[state.Name] = new State(state.Name, new HashSet<string>(state.NextStates) { nextState.Name });
            return _states[state.Name];
        }
    }
}
