// ============================================================
//  EnemyPatrolState.cs
//  Moves between waypoints until the player enters detect range.
//
//  Bugs fixed vs original:
//   1. remainingDistance returns 0 on frame-1 before the path
//      is computed → guarded with agent.hasPath so we never
//      "arrive" before actually moving.
//   2. isStopped was never reset on OnEnter → enemy stays
//      frozen when re-entering Patrol from another state.
//   3. Clear Debug.LogError when waypoints are missing so the
//      developer immediately knows what's wrong.
// ============================================================
using UnityEngine;

public class EnemyPatrolState : IState
{
    public string StateName => "Patrol";

    // ── Setter injection ─────────────────────────────────────
    public EnemyChaseState ChaseState { get; set; }

    private readonly EnemyContext _ctx;
    private readonly StateMachine _sm;

    // How close the agent must be before we consider it "arrived"
    private const float ArrivalThreshold = 0.4f;

    // Small idle wait before moving to next waypoint (feels more natural)
    private float _waitTimer;
    private const float WaypointWaitTime = 0.5f;

    // Tracks whether we're actually waiting at a waypoint
    private bool _waiting;

    public EnemyPatrolState(EnemyContext ctx, StateMachine sm)
    {
        _ctx = ctx;
        _sm  = sm;
    }

    // ── IState ───────────────────────────────────────────────
    public void OnEnter()
    {
        // BUG FIX 1: always un-stop the agent when entering patrol
        _ctx.Agent.isStopped       = false;
        _ctx.Agent.speed           = _ctx.PatrolSpeed;
        _ctx.Agent.stoppingDistance = 0f;   // let arrival threshold handle stopping

        _waiting = false;
        _waitTimer = 0f;

        // Validate waypoints — loud error so the developer sees it immediately
        if (_ctx.PatrolPoints == null || _ctx.PatrolPoints.Length == 0)
        {
            Debug.LogError(
                $"[PatrolState] '{_ctx.Self.name}' has NO patrol waypoints assigned! " +
                "Assign at least 1 Transform in EnemyAI → Patrol Points.");
            return;
        }

        MoveToCurrentWaypoint();
    }

    public void OnUpdate()
    {
        if (_ctx.PatrolPoints == null || _ctx.PatrolPoints.Length == 0) return;

        // Transition: player spotted → Chase
        if (_ctx.PlayerInDetectRange)
        {
            _sm.ChangeState(ChaseState);
            return;
        }

        // ── BUG FIX 2: guard with hasPath so frame-1 remainingDistance==0
        //    doesn't count as "arrived". The agent only has a path once the
        //    NavMesh has finished computing it (usually 1-2 frames after
        //    SetDestination is called).
        bool pathReady  = _ctx.Agent.hasPath && !_ctx.Agent.pathPending;
        bool arrived    = pathReady && _ctx.Agent.remainingDistance <= ArrivalThreshold;

        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _waiting = false;
                AdvanceWaypoint();
            }
            return;
        }

        if (arrived)
        {
            _ctx.Agent.ResetPath();     // stop sliding
            _waiting   = true;
            _waitTimer = WaypointWaitTime;
        }
    }

    public void OnFixedUpdate() { }

    public void OnExit()
    {
        _waiting   = false;
        _waitTimer = 0f;
    }

    // ── Helpers ──────────────────────────────────────────────
    private void MoveToCurrentWaypoint()
    {
        Transform wp = _ctx.PatrolPoints[_ctx.CurrentPatrolIndex];
        if (wp == null)
        {
            Debug.LogWarning($"[PatrolState] Waypoint at index {_ctx.CurrentPatrolIndex} is null!");
            return;
        }

        _ctx.Agent.isStopped = false;
        bool ok = _ctx.Agent.SetDestination(wp.position);

        if (!ok)
            Debug.LogWarning(
                $"[PatrolState] SetDestination failed for waypoint '{wp.name}'. " +
                "Is the NavMesh baked? Is the waypoint on the NavMesh surface?");
    }

    private void AdvanceWaypoint()
    {
        _ctx.CurrentPatrolIndex = (_ctx.CurrentPatrolIndex + 1) % _ctx.PatrolPoints.Length;
        MoveToCurrentWaypoint();
    }
}
