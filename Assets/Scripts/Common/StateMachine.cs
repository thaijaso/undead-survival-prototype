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
        // CRITICAL: Check for null state before doing anything
        if (newState == null)
        {
            Debug.LogError($"[{ownerName}] CRITICAL: Attempted to transition to NULL state from {currentState?.GetType().Name ?? "None"}!");
            Debug.LogError($"[{ownerName}] Stack trace for null state transition:");
            Debug.LogError(System.Environment.StackTrace);
            return;
        }

        if (ReferenceEquals(currentState, newState))
        {
            Debug.Log($"[{ownerName}] State {newState.GetType().Name} is already active.");
            return;
        }

        string previousStateName = currentState?.GetType().Name ?? "None";
        string newStateName = newState.GetType().Name;
        
        Debug.Log($"[{ownerName}] State transition: {previousStateName} -> {newStateName}.");

        currentState?.Exit(newState);
        currentState = newState;
        currentState.Enter();

        Debug.Log($"[{ownerName}] Successfully entered {newStateName}.");
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


