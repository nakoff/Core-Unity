
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Fsm
{
    public class FsmState
    {
        public string Name { get; }
        public Func<bool> CanEnter { get; set; }
        public Func<bool> CanExit { get; set; }
        public Action<float> OnUpdate { get; set; }
        public Action OnEntered { get; set; }
        public Action OnExited { get; set; }

        public FsmState(string name)
        {
            Name = name;
        }
    }

    public class AnonimouseState
    {
        public FsmState State { get; }

        public AnonimouseState
        (
            string Name,
            Action onEntered = null,
            Action onExited = null,
            Action<float> onUpdate = null,
            Func<bool> canEnter = null,
            Func<bool> canExit = null
        )
        {
            var state = new FsmState(Name);
            state.CanEnter = canEnter;
            state.CanExit = canExit;
            state.OnEntered = onEntered;
            state.OnExited = onExited;
            state.OnUpdate = onUpdate;

            State = state;
        }
    }

    public class Fsm
    {
        public enum Mode 
        {
            AUTO,
            MANUAL,
            COMBINE,
        }

        public Mode RuleMode { get; set; } = Mode.COMBINE;

        private HashSet<FsmState> _states = new();
        protected FsmState curState;
        protected FsmState prevState;

        public Fsm(List<AnonimouseState> states)
        {
            foreach (var state in states)
                AddState(state.State);
            
            ChangeState(_states.FirstOrDefault());
        }

        public Fsm(ICollection<FsmState> states = null)
        {
            if (states == null)
                return;
            
            _states = states.ToHashSet<FsmState>();
            ChangeState(_states.FirstOrDefault());
        }

        public void AddState(FsmState state)
        {
            if (_states.Contains(state))
                return;

            if (_states.FirstOrDefault(s => s.Name == state.Name) != null)
            {
                Debug.LogError($"State {state.Name} already is exist");
                return;
            }

            _states.Add(state);
        }

        public void OnUpdate(float dt)
        {
            curState?.OnUpdate?.Invoke(dt);

            if (RuleMode == Mode.MANUAL)
                return;

            if (curState != null && curState.CanEnter != null && !curState.CanExit())
                return;
            
            foreach (var state in _states)
            {
                if (state.CanEnter == null || !state.CanEnter())
                    continue;
                
                ChangeState(state);
                break;
            }
        }

        public void ChangeState(string name)
        {
            if (RuleMode == Mode.AUTO)
                return;
            
            var newState = _states.FirstOrDefault(s => s.Name == name);
            if (newState == null)
            {
                Debug.Log($"State {name} not exist");
                return;
            }
            ChangeState(newState);
        }

        private void ChangeState(FsmState newState)
        {
            if (curState?.CanExit != null && !curState.CanExit())
                return;
            if (newState == null || curState == newState)
                return;
            
            curState?.OnExited?.Invoke();
            newState.OnEntered?.Invoke();
            prevState = curState;
            curState = newState;
        }
    }
}
