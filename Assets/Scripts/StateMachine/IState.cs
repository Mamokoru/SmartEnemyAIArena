// ============================================================
//  IState.cs
//  Base contract every concrete state must implement.
//  Keeping it thin gives maximum flexibility.
// ============================================================
public interface IState
{
    string StateName { get; }   // used by debug overlay & UI
    void OnEnter();
    void OnUpdate();
    void OnFixedUpdate();
    void OnExit();
}
