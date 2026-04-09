using System;
using System.Collections.Generic;
using UnityEngine;

namespace RigidBodyPlayer.PlayerStates
{
    public class StateMachine<Estate> : MonoBehaviour where Estate : Enum
    {
        protected Dictionary<Estate, BaseState<Estate>> States = new Dictionary<Estate, BaseState<Estate>>();
        protected BaseState<Estate> CurrentState { get; private set; }
        public Estate CurrentStateKey => CurrentState.StateKey;
        private Estate queuedState;
        private bool isTransitionQueued = false;
        private bool isTransitioning = false;
        
        protected void InitializeState(Estate initialState)
        {
            CurrentState = States[initialState];
        }

        
        protected void Start()
        {
            CurrentState?.EnterState();
            if (isTransitionQueued && !isTransitioning)
            {
                ExecuteTransition();
            }
        }

        protected void Update()
        {
            if (isTransitionQueued && !isTransitioning)
            {
                ExecuteTransition();
            }
            
            CurrentState?.UpdateState();
        }
        
        protected void FixedUpdate()
        {
            CurrentState?.FixedUpdateState();
        }

        public void QueueNextState(Estate nextStateKey)
        {
            if (CurrentState != null && nextStateKey.Equals(CurrentState.StateKey)) return;
            
            queuedState = nextStateKey;
            isTransitionQueued = true;
        }    
        
        private void ExecuteTransition()
        {
            isTransitioning = true;
        
            CurrentState?.ExitState();
            CurrentState = States[queuedState];
            CurrentState?.EnterState();
        
            isTransitionQueued = false;
            isTransitioning = false;
        }
    }
}