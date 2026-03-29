// ============================================================
//  EnemyHealth.cs
// ============================================================
using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    public float MaxHealth     => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public float HealthPercent => _currentHealth / _maxHealth;
    public bool  IsDead        => _currentHealth <= 0f;

    public event Action<float, float> OnHealthChanged;   // (current, max)
    public event Action               OnDeath;

    private EnemyAI _ai;

    private void Awake()
    {
        _currentHealth = _maxHealth;
        _ai = GetComponent<EnemyAI>();
    }

    public void TakeDamage(float amount)
    {
        if (IsDead) return;

        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (IsDead)
        {
            OnDeath?.Invoke();
            _ai?.OnDeath();
            // Simple death: disable renderer + collider, destroy after delay
            Destroy(gameObject, 2f);
        }
    }
}
