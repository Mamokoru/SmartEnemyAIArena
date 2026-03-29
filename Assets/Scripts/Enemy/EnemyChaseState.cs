// ============================================================
//  EnemyChaseState.cs
// ============================================================
using UnityEngine;

public class EnemyChaseState : IState
{
    public string StateName => "Chase";

    public EnemyPatrolState PatrolState { get; set; }
    public EnemyAttackState AttackState { get; set; }

    private readonly EnemyContext _ctx;
    private readonly StateMachine _sm;

    private const float DestUpdateInterval = 0.15f;
    private float _lastDestUpdate;

    public EnemyChaseState(EnemyContext ctx, StateMachine sm)
    {
        _ctx = ctx;
        _sm  = sm;
    }

    public void OnEnter()
    {
        _ctx.Agent.isStopped        = false;      // always unlock the agent
        _ctx.Agent.speed            = _ctx.ChaseSpeed;
        _ctx.Agent.stoppingDistance = _ctx.AttackRange * 0.85f;

        // Immediately set destination so the agent starts moving on frame 1
        if (_ctx.PlayerTransform != null)
            _ctx.Agent.SetDestination(_ctx.PlayerTransform.position);

        _lastDestUpdate = Time.time;
    }

    public void OnUpdate()
    {
        if (_ctx.PlayerOutOfLoseRange)   { _sm.ChangeState(PatrolState); return; }
        if (_ctx.PlayerInAttackRange)    { _sm.ChangeState(AttackState); return; }

        if (Time.time >= _lastDestUpdate + DestUpdateInterval)
        {
            _lastDestUpdate = Time.time;
            _ctx.Agent.SetDestination(_ctx.PlayerTransform.position);
        }
    }

    public void OnFixedUpdate() { }

    public void OnExit() => _ctx.Agent.ResetPath();
}
