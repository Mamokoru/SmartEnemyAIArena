// ============================================================
//  EnemyAttackState.cs
// ============================================================
using UnityEngine;

public class EnemyAttackState : IState
{
    public string StateName => "Attack";

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

    public void OnEnter()
    {
        _ctx.Agent.isStopped        = false;     // unlock agent
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
        if (_ctx.CanAttack) PerformAttack();
    }

    public void OnFixedUpdate() { }
    public void OnExit() => _ctx.Agent.ResetPath();

    private void PerformAttack()
    {
        _ctx.LastAttackTime = Time.time;
        bool hit = _ctx.PlayerHealth.TakeDamage(_ctx.AttackDamage);
        if (hit)
        {
            _ctx.Learning.RecordSuccessfulAttack(_ctx.Self.position, _ctx.PlayerTransform);
            Debug.Log($"[Attack] Hit! Memory: {_ctx.Learning.MemoryCount}/{_ctx.Learning.MemoryCapacity}");
        }
        ChooseAttackPosition();
    }

    private void ChooseAttackPosition()
    {
        _targetAttackPos = _ctx.Learning.ChooseBestAttackPosition(
            _ctx.Self, _ctx.PlayerTransform, _ctx.AttackRange * 0.9f);
        _ctx.Agent.isStopped = false;
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
