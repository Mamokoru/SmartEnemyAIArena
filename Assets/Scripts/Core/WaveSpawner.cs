// ============================================================
//  WaveSpawner.cs
//  Spawns enemies in numbered waves.  Each wave waits for all
//  enemies to be cleared before starting the next.
//  Plugs into ArenaGameManager via events.
//
//  Portfolio value: shows you can think beyond "one enemy" and
//  design systems that scale.
// ============================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────
    [System.Serializable]
    public class Wave
    {
        public string WaveName       = "Wave 1";
        public int    EnemyCount     = 2;
        [Tooltip("Seconds between each enemy spawn in this wave")]
        public float  SpawnInterval  = 0.8f;
        [Tooltip("Seconds before this wave starts after the previous one ends")]
        public float  PreWaveDelay   = 3f;
        [Tooltip("Override enemy speed for difficulty ramp — 0 = use prefab default")]
        public float  ChaseSpeedOverride = 0f;
    }

    [Header("Waves")]
    [SerializeField] private List<Wave>   _waves;
    [SerializeField] private GameObject   _enemyPrefab;
    [SerializeField] private Transform[]  _spawnPoints;

    [Header("UI")]
    [SerializeField] private UIManager    _uiManager;

    // ── Private ───────────────────────────────────────────────
    private int          _currentWaveIndex = -1;
    private int          _aliveEnemies;
    private bool         _spawning;

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Start()
    {
        StartCoroutine(RunWaves());
    }

    // ── Wave Loop ────────────────────────────────────────────

    private IEnumerator RunWaves()
    {
        while (_currentWaveIndex < _waves.Count - 1)
        {
            _currentWaveIndex++;
            Wave wave = _waves[_currentWaveIndex];

            // Pre-wave countdown
            yield return new WaitForSeconds(wave.PreWaveDelay);

            // Spawn all enemies for this wave
            yield return StartCoroutine(SpawnWave(wave));

            // Wait until all enemies are dead
            yield return new WaitUntil(() => _aliveEnemies <= 0);

            Debug.Log($"[WaveSpawner] Wave {_currentWaveIndex + 1} cleared!");
        }

        // All waves cleared — signal win
        ArenaGameManager.Instance?.OnAllWavesCleared();
    }

    private IEnumerator SpawnWave(Wave wave)
    {
        _spawning = true;
        Debug.Log($"[WaveSpawner] Starting {wave.WaveName} — {wave.EnemyCount} enemies");

        for (int i = 0; i < wave.EnemyCount; i++)
        {
            SpawnEnemy(wave);
            yield return new WaitForSeconds(wave.SpawnInterval);
        }

        _spawning = false;
    }

    private void SpawnEnemy(Wave wave)
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0) return;

        // Pick a random spawn point
        Transform sp  = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        GameObject go = Instantiate(_enemyPrefab, sp.position, sp.rotation);

        var ai = go.GetComponent<EnemyAI>();
        if (ai == null) return;

        // Optional speed ramp-up per wave
        if (wave.ChaseSpeedOverride > 0f)
            go.GetComponent<EnemyAI>().SetChaseSpeed(wave.ChaseSpeedOverride);

        // Track alive count
        _aliveEnemies++;
        go.GetComponent<EnemyHealth>().OnDeath += () =>
        {
            _aliveEnemies--;
            _uiManager?.UpdateEnemyCount(_aliveEnemies);
        };

        // Register with game manager
        ArenaGameManager.Instance?.RegisterEnemy(ai);
        _uiManager?.UpdateEnemyCount(_aliveEnemies);
    }

    // ── Public ────────────────────────────────────────────────
    public int  CurrentWave  => _currentWaveIndex + 1;
    public int  TotalWaves   => _waves.Count;
    public int  AliveEnemies => _aliveEnemies;
}
