// ============================================================
//  EnemyAttackState.cs
//  Moves to the best learned attack angle, deals damage, and
//  records successful hits back to EnemyLearningModule.
//
//  Setter injection: ChaseState is set by EnemyAI after all
//  states are constructed, avoiding circular ctor deps.
// ============================================================
using UnityEngine;

public class EnemyAttackState : IState
{
    public string StateName => "Attack";

    // ── Setter injection ─────────────────────────────────────
    public EnemyChaseState ChaseState { get; set; }

    private readonly EnemyContext _ctx;
    private readonly StateMachine _sm;

    private bool    _repositioning;
    private Vector3 _targetAttackPos;
    private const float RepThreshold = 0.6f;

    public EnemyAttackState(EnemyContext ctx, StateMachine sm)
    {
        _ctx = ctx;
        _sm  = sm;
    }

    // ── IState ───────────────────────────────────────────────
    public void OnEnter()
    {
        _ctx.Agent.speed            = _ctx.ChaseSpeed * 0.8f;
        _ctx.Agent.stoppingDistance = RepThreshold;
        ChooseAttackPosition();
    }

    public void OnUpdate()
    {
        if (!_ctx.PlayerInAttackRange && !_repositioning)
        {
            _sm.ChangeState(ChaseState);
            return;
        }

        if (_repositioning) { HandleRepositioning(); return; }

        FacePlayer();

        if (_ctx.CanAttack)
            PerformAttack();
    }

    public void OnFixedUpdate() { }

    public void OnExit() => _ctx.Agent.ResetPath();

    // ── Attack ───────────────────────────────────────────────

    private void PerformAttack()
    {
        _ctx.LastAttackTime = Time.time;

        bool hit = _ctx.PlayerHealth.TakeDamage(_ctx.AttackDamage);

        if (hit)
        {
            _ctx.Learning.RecordSuccessfulAttack(_ctx.Self.position, _ctx.PlayerTransform);
            Debug.Log($"[Attack] Hit! Memory: {_ctx.Learning.MemoryCount}/{_ctx.Learning.MemoryCapacity}");
        }

        ChooseAttackPosition();   // reposition after every strike
    }

    // ── Repositioning ────────────────────────────────────────

    private void ChooseAttackPosition()
    {
        _targetAttackPos = _ctx.Learning.ChooseBestAttackPosition(
            _ctx.Self, _ctx.PlayerTransform, _ctx.AttackRange * 0.9f);

        _ctx.Agent.SetDestination(_targetAttackPos);
        _repositioning = true;
    }

    private void HandleRepositioning()
    {
        if (_ctx.Agent.pathPending) return;
        if (_ctx.Agent.remainingDistance <= RepThreshold)
        {
            _repositioning = false;
            _ctx.Agent.ResetPath();
        }
    }

    // ── Helpers ──────────────────────────────────────────────

    private void FacePlayer()
    {
        Vector3 dir = (_ctx.PlayerTransform.position - _ctx.Self.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;
        _ctx.Self.rotation = Quaternion.Slerp(
            _ctx.Self.rotation,
            Quaternion.LookRotation(dir),
            Time.deltaTime * 8f);
    }
}
