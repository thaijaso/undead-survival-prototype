using System;
using UnityEngine;

public interface IState<T>
{
    void Enter();
    void Exit(T nextState);
    void LogicUpdate();
    void PhysicsUpdate();
    void LateUpdate();
}

public class StateMachine<T> where T : IState<T>
{
    public string ownerName { get; private set; }
    public T currentState { get; private set; }

    public StateMachine(String owner)
    {
        ownerName = owner;
    }

    public void SetState(T newState)
    {
        if (ReferenceEquals(currentState, newState))
        {
            Debug.Log($"State {newState.GetType().Name} is already active.");
            return;
        }

        currentState?.Exit(newState);
        currentState = newState;
        currentState.Enter();

        Debug.Log($"{ownerName} transitioned to state: {newState.GetType().Name}");
    }

    public void LogicUpdate()
    {
        currentState?.LogicUpdate();
    }

    public void PhysicsUpdate()
    {
        currentState?.PhysicsUpdate();
    }

    public void LateUpdate()
    {
        currentState?.LateUpdate();
    }
}


