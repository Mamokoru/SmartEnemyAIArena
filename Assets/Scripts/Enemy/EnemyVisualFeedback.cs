// ============================================================
//  EnemyVisualFeedback.cs
//  Purely cosmetic — flashes the enemy red on hit, plays a
//  death animation if an Animator is present.
//  Attach alongside EnemyAI on the enemy prefab.
// ============================================================
using System.Collections;
using UnityEngine;

public class EnemyVisualFeedback : MonoBehaviour
{
    [Header("Hit Flash")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Color    _hitColor   = Color.red;
    [SerializeField] private float    _flashTime  = 0.12f;

    [Header("Animator (optional)")]
    [SerializeField] private Animator _animator;
    // Animator parameter names
    private static readonly int _stateHash  = Animator.StringToHash("State");
    private static readonly int _attackHash = Animator.StringToHash("Attack");
    private static readonly int _deathHash  = Animator.StringToHash("Death");

    private Color   _originalColor;
    private bool    _flashing;

    private EnemyHealth _health;
    private EnemyAI     _ai;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
        _ai     = GetComponent<EnemyAI>();

        if (_renderer)
            _originalColor = _renderer.material.color;
    }

    private void OnEnable()
    {
        if (_health) _health.OnHealthChanged += OnHit;
        if (_health) _health.OnDeath         += OnDeath;
    }

    private void OnDisable()
    {
        if (_health) _health.OnHealthChanged -= OnHit;
        if (_health) _health.OnDeath         -= OnDeath;
    }

    private void Update()
    {
        // Drive animator state int from the state machine
        if (_animator == null || _ai == null) return;
        int stateInt = _ai.CurrentStateName switch
        {
            "Patrol" => 0,
            "Chase"  => 1,
            "Attack" => 2,
            _        => 0
        };
        _animator.SetInteger(_stateHash, stateInt);
    }

    // ── Callbacks ────────────────────────────────────────────

    private void OnHit(float current, float max)
    {
        if (!_flashing) StartCoroutine(FlashRoutine());
    }

    private void OnDeath()
    {
        if (_animator == null || _ai == null) return;

        _animator?.SetTrigger(_deathHash);
    }

    // ── Coroutine ────────────────────────────────────────────

    private IEnumerator FlashRoutine()
    {
        _flashing = true;
        if (_renderer) _renderer.material.color = _hitColor;
        yield return new WaitForSeconds(_flashTime);
        if (_renderer) _renderer.material.color = _originalColor;
        _flashing = false;
    }
}
