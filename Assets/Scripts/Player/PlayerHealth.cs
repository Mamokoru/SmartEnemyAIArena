// ============================================================
//  PlayerHealth.cs
// ============================================================
using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    public float MaxHealth     => _maxHealth;
    public float CurrentHealth => _currentHealth;
    public float HealthPercent => _currentHealth / _maxHealth;
    public bool  IsDead        => _currentHealth <= 0f;

    public event Action<float, float> OnHealthChanged;  // (current, max)
    public event Action               OnDeath;

    private void Awake() => _currentHealth = _maxHealth;

    /// <summary>
    /// Returns true if the damage actually connected (player was alive).
    /// EnemyAttackState uses the return value to decide whether to record a success.
    /// </summary>
    public bool TakeDamage(float amount)
    {
        if (IsDead) return false;

        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (IsDead)
        {
            OnDeath?.Invoke();
            ArenaGameManager.Instance?.OnPlayerDied();
        }

        return true;    // hit connected
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
}
