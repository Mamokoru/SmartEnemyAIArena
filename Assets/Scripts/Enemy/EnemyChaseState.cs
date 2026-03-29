// ============================================================
//  EnemyChaseState.cs
//  Pursues the player.  Transitions to Attack when close enough,
//  back to Patrol when player escapes LoseTargetRange.
//
//  Uses setter injection to break the circular dependency:
//  Patrol <-> Chase <-> Attack all reference each other.
// ============================================================
using UnityEngine;

public class EnemyChaseState : IState
{
    public string StateName => "Chase";

    // ── Setter injection — wire AFTER construction ────────────
    public EnemyPatrolState PatrolState { get; set; }
    public EnemyAttackState AttackState { get; set; }

    private readonly EnemyContext _ctx;
    private readonly StateMachine _sm;

    // Throttle NavMesh destination updates (big perf win)
    private const float DestUpdateInterval = 0.15f;
    private float _lastDestUpdate;

    public EnemyChaseState(EnemyContext ctx, StateMachine sm)
    {
        _ctx = ctx;
        _sm  = sm;
    }

    // ── IState ───────────────────────────────────────────────
    public void OnEnter()
    {
        _ctx.Agent.speed            = _ctx.ChaseSpeed;
        _ctx.Agent.stoppingDistance = _ctx.AttackRange * 0.85f;
    }

    public void OnUpdate()
    {
        // Lost the player — back to patrol
        if (_ctx.PlayerOutOfLoseRange)
        {
            _sm.ChangeState(PatrolState);
            return;
        }

        // Close enough to attack
        if (_ctx.PlayerInAttackRange)
        {
            _sm.ChangeState(AttackState);
            return;
        }

        // Throttled destination — never call SetDestination 60x per second
        if (Time.time >= _lastDestUpdate + DestUpdateInterval)
        {
            _lastDestUpdate = Time.time;
            _ctx.Agent.SetDestination(_ctx.PlayerTransform.position);
        }
    }

    public void OnFixedUpdate() { }

    public void OnExit() => _ctx.Agent.ResetPath();
}
