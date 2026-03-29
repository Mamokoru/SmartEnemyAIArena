// ============================================================
//  EnemyContext.cs
//  Shared data bag injected into every enemy state.
//  States READ and WRITE here — no direct component coupling.
//  This is the "Blackboard" pattern, common in game AI.
// ============================================================
using System;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class EnemyContext
{
    // ── Component References ──────────────────────────────────
    [NonSerialized] public Transform        Self;
    [NonSerialized] public NavMeshAgent     Agent;
    [NonSerialized] public Animator         Animator;           // optional
    [NonSerialized] public EnemyAI          AI;
    [NonSerialized] public EnemyHealth      Health;
    [NonSerialized] public EnemyLearningModule Learning;

    // ── Target ───────────────────────────────────────────────
    [NonSerialized] public Transform        PlayerTransform;
    [NonSerialized] public PlayerHealth     PlayerHealth;

    // ── Tuning (set in Inspector via EnemyAI) ────────────────
    public float DetectRange        = 12f;
    public float AttackRange        = 3f;
    public float AttackCooldown     = 1.5f;
    public float PatrolSpeed        = 2f;
    public float ChaseSpeed         = 4.5f;
    public float AttackDamage       = 10f;
    public float LoseTargetRange    = 18f;

    // ── Patrol Waypoints ─────────────────────────────────────
    public Transform[]      PatrolPoints;
    public int              CurrentPatrolIndex;

    // ── Runtime State ─────────────────────────────────────────
    public float            LastAttackTime;
    public bool             CanAttack => Time.time >= LastAttackTime + AttackCooldown;

    // ── Convenience ──────────────────────────────────────────
    public float DistanceToPlayer =>
        PlayerTransform != null
            ? Vector3.Distance(Self.position, PlayerTransform.position)
            : float.MaxValue;

    public bool PlayerInDetectRange  => DistanceToPlayer <= DetectRange;
    public bool PlayerInAttackRange  => DistanceToPlayer <= AttackRange;
    public bool PlayerOutOfLoseRange => DistanceToPlayer > LoseTargetRange;
}
