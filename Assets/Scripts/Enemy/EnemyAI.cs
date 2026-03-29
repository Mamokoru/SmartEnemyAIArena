// ============================================================
//  EnemyAI.cs
//  The MonoBehaviour brain.  Owns the StateMachine, wires up
//  all states, and drives updates.  Think of it as the
//  "Director" — it delegates all behaviour to states.
// ============================================================
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyLearningModule))]
public class EnemyAI : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────
    [Header("Detection & Combat")]
    [SerializeField] private float _detectRange     = 12f;
    [SerializeField] private float _attackRange     = 2.5f;
    [SerializeField] private float _loseTargetRange = 18f;
    [SerializeField] private float _attackCooldown  = 1.5f;
    [SerializeField] private float _attackDamage    = 10f;

    [Header("Movement")]
    [SerializeField] private float _patrolSpeed = 2f;
    [SerializeField] private float _chaseSpeed  = 4.5f;

    [Header("Patrol Waypoints")]
    [SerializeField] private Transform[] _patrolPoints;

    [Header("Debug")]
    [SerializeField] private bool _showDebugGizmos = true;

    // ── Private ───────────────────────────────────────────────
    private StateMachine   _stateMachine;
    private EnemyContext   _ctx;

    // Expose for UIManager / external systems
    public string CurrentStateName => _stateMachine?.CurrentStateName ?? "None";
    public EnemyContext Context    => _ctx;

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Awake()
    {
        BuildContext();
        BuildStateMachine();
    }

    private void Start()
    {
        // Find player — done in Start so PlayerController can register first
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[EnemyAI] No GameObject tagged 'Player' found!");
            enabled = false;
            return;
        }

        _ctx.PlayerTransform = player.transform;
        _ctx.PlayerHealth    = player.GetComponent<PlayerHealth>();
    }

    private void Update()      => _stateMachine.Update();
    private void FixedUpdate() => _stateMachine.FixedUpdate();

    // ── Setup ─────────────────────────────────────────────────

    private void BuildContext()
    {
        _ctx = new EnemyContext
        {
            Self             = transform,
            Agent            = GetComponent<NavMeshAgent>(),
            Health           = GetComponent<EnemyHealth>(),
            Learning         = GetComponent<EnemyLearningModule>(),
            AI               = this,
            DetectRange      = _detectRange,
            AttackRange      = _attackRange,
            LoseTargetRange  = _loseTargetRange,
            AttackCooldown   = _attackCooldown,
            AttackDamage     = _attackDamage,
            PatrolSpeed      = _patrolSpeed,
            ChaseSpeed       = _chaseSpeed,
            PatrolPoints     = _patrolPoints,
        };
    }

    private void BuildStateMachine()
    {
        _stateMachine = new StateMachine();

        // ── Step 1: construct all states (no cross-refs yet) ──
        var patrol = new EnemyPatrolState(_ctx, _stateMachine);
        var chase  = new EnemyChaseState (_ctx, _stateMachine);
        var attack = new EnemyAttackState(_ctx, _stateMachine);

        // ── Step 2: setter injection — wire transitions ────────
        // This is the correct way to break circular ctor dependencies.
        // Every state is fully constructed before any cross-reference is set.
        patrol.ChaseState  = chase;

        chase.PatrolState  = patrol;
        chase.AttackState  = attack;

        attack.ChaseState  = chase;

        // ── Step 3: boot ──────────────────────────────────────
        _stateMachine.Initialize(patrol);
    }

    // ── Public ────────────────────────────────────────────────

    /// <summary>Called by EnemyHealth when the enemy dies.</summary>
    public void OnDeath()
    {
        enabled = false;
        _ctx.Agent.isStopped = true;
        Debug.Log("[EnemyAI] Enemy died.");
    }

    /// <summary>Called by WaveSpawner to ramp up difficulty per wave.</summary>
    public void SetChaseSpeed(float speed)
    {
        _chaseSpeed        = speed;
        _ctx.ChaseSpeed    = speed;
    }

    // ── Gizmos ────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (!_showDebugGizmos) return;

        // Detect range — cyan
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _detectRange);

        // Attack range — red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        // Lose target range — grey
        Gizmos.color = new Color(1, 1, 1, 0.2f);
        Gizmos.DrawWireSphere(transform.position, _loseTargetRange);
    }
}
