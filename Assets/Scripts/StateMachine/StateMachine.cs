// ============================================================
//  StateMachine.cs
//  Generic, reusable state machine.
//  Fires an event on every transition — great for interviews:
//  "I decouple state logic from transition logic via events."
// ============================================================
using System;
using UnityEngine;

public class StateMachine
{
    // ── Public ────────────────────────────────────────────────
    public IState CurrentState  { get; private set; }
    public string CurrentStateName => CurrentState?.StateName ?? "None";

    /// <summary>Fires AFTER the transition completes: (from, to)</summary>
    public event Action<IState, IState> OnStateChanged;

    // ── API ───────────────────────────────────────────────────
    public void Initialize(IState startState)
    {
        CurrentState = startState;
        CurrentState.OnEnter();
    }

    public void ChangeState(IState newState)
    {
        if (newState == null || newState == CurrentState) return;

        var previous = CurrentState;
        CurrentState.OnExit();
        CurrentState = newState;
        CurrentState.OnEnter();

        OnStateChanged?.Invoke(previous, CurrentState);

        Debug.Log($"[StateMachine] {previous?.StateName} → {CurrentState.StateName}");
    }

    /// <summary>Call from MonoBehaviour.Update()</summary>
    public void Update()      => CurrentState?.OnUpdate();

    /// <summary>Call from MonoBehaviour.FixedUpdate()</summary>
    public void FixedUpdate() => CurrentState?.OnFixedUpdate();
}
