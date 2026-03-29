// ============================================================
//  ArenaGameManager.cs
//  Singleton game manager.  Tracks game state (Playing / Won /
//  Lost) and drives the UI.  Spawns additional enemies if you
//  need wave-based play later.
// ============================================================
using System;
using System.Collections.Generic;
using UnityEngine;

public class ArenaGameManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────
    public static ArenaGameManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private UIManager _uiManager;

    [Header("Enemy Spawn (optional — can place manually)")]
    [SerializeField] private GameObject   _enemyPrefab;
    [SerializeField] private Transform[]  _spawnPoints;

    // ── State ─────────────────────────────────────────────────
    public enum GameState { Playing, PlayerWon, PlayerLost }
    public GameState CurrentState { get; private set; } = GameState.Playing;

    private List<EnemyAI> _activeEnemies = new();

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Register any enemies already in the scene
        foreach (var enemy in FindObjectsOfType<EnemyAI>())
            RegisterEnemy(enemy);

        // Optionally spawn enemies from spawnPoints
        if (_enemyPrefab != null && _spawnPoints != null)
        {
            foreach (var sp in _spawnPoints)
            {
                var go    = Instantiate(_enemyPrefab, sp.position, sp.rotation);
                var ai    = go.GetComponent<EnemyAI>();
                if (ai) RegisterEnemy(ai);
            }
        }

        _uiManager?.UpdateEnemyCount(_activeEnemies.Count);
    }

    // ── Enemy Management ──────────────────────────────────────

    public void RegisterEnemy(EnemyAI enemy)
    {
        if (!_activeEnemies.Contains(enemy))
            _activeEnemies.Add(enemy);

        enemy.GetComponent<EnemyHealth>().OnDeath += () => OnEnemyDied(enemy);
    }

    private void OnEnemyDied(EnemyAI enemy)
    {
        _activeEnemies.Remove(enemy);
        _uiManager?.UpdateEnemyCount(_activeEnemies.Count);

        if (_activeEnemies.Count == 0)
            SetState(GameState.PlayerWon);
    }

    // ── Player Events ─────────────────────────────────────────

    public void OnPlayerDied() => SetState(GameState.PlayerLost);

    // ── State ─────────────────────────────────────────────────

    private void SetState(GameState newState)
    {
        if (CurrentState != GameState.Playing) return;  // only transition once
        CurrentState = newState;
        _uiManager?.ShowEndScreen(newState == GameState.PlayerWon);
        Debug.Log($"[GameManager] Game ended: {newState}");
    }

    public void RestartGame() => UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);

    internal void OnAllWavesCleared()
    {
        
    }
}
