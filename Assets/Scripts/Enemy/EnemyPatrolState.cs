// ============================================================
//  EnemyPatrolState.cs
//  Moves between waypoints until the player enters detect range.
// ============================================================
using UnityEngine;

public class EnemyPatrolState : IState
{
    public string StateName => "Patrol";

    // ── Setter injection — set AFTER construction to break circular deps ──
    public EnemyChaseState ChaseState { get; set; }

    private readonly EnemyContext _ctx;
    private readonly StateMachine _sm;
    private float _waypointReachThreshold = 0.5f;

    public EnemyPatrolState(EnemyContext ctx, StateMachine sm)
    {
        _ctx = ctx;
        _sm  = sm;
    }

    // ── IState ───────────────────────────────────────────────
    public void OnEnter()
    {
        _ctx.Agent.speed = _ctx.PatrolSpeed;
        _ctx.Agent.stoppingDistance = 0.2f;
        MoveToCurrentWaypoint();
    }

    public void OnUpdate()
    {
        // Transition: player spotted → Chase
        if (_ctx.PlayerInDetectRange)
        {
            _sm.ChangeState(ChaseState);
            return;
        }

        // Advance waypoint when close enough
        if (!_ctx.Agent.pathPending &&
            _ctx.Agent.remainingDistance <= _waypointReachThreshold)
        {
            AdvanceWaypoint();
        }
    }

    public void OnFixedUpdate() { }

    public void OnExit()
    {
        // nothing to clean up
    }

    // ── Helpers ──────────────────────────────────────────────
    private void MoveToCurrentWaypoint()
    {
        if (_ctx.PatrolPoints == null || _ctx.PatrolPoints.Length == 0) return;
        _ctx.Agent.SetDestination(_ctx.PatrolPoints[_ctx.CurrentPatrolIndex].position);
    }

    private void AdvanceWaypoint()
    {
        if (_ctx.PatrolPoints == null || _ctx.PatrolPoints.Length == 0) return;
        _ctx.CurrentPatrolIndex = (_ctx.CurrentPatrolIndex + 1) % _ctx.PatrolPoints.Length;
        MoveToCurrentWaypoint();
    }
}
