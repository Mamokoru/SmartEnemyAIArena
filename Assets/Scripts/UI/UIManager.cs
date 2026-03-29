// ============================================================
//  UIManager.cs
//  Drives all HUD elements:
//    - Player health bar
//    - Enemy count
//    - Current AI state label (great for portfolio demos!)
//    - Learning module progress bar
//    - Win / Lose screen
// ============================================================
using UnityEngine;
using UnityEngine.UI;
using TMPro;                // requires TextMeshPro package

public class UIManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────
    [Header("Player Health")]
    [SerializeField] private Slider _healthSlider;
    [SerializeField] private TMP_Text _healthText;

    [Header("Enemy Info")]
    [SerializeField] private TMP_Text _enemyCountText;

    [Header("AI Debug Overlay (optional but impressive)")]
    [SerializeField] private TMP_Text _aiStateText;
    [SerializeField] private Slider _learningSlider;    // shows memory fill %
    [SerializeField] private TMP_Text _learningText;

    [Header("End Screen")]
    [SerializeField] private GameObject _endScreen;
    [SerializeField] private TMP_Text _endTitleText;
    [SerializeField] private Button _restartButton;

    // ── References ────────────────────────────────────────────
    private PlayerHealth _playerHealth;
    private EnemyAI _trackedEnemy;     // first enemy for debug overlay

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Start()
    {
        // Hook player
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerHealth = player.GetComponent<PlayerHealth>();
            _playerHealth.OnHealthChanged += UpdateHealthUI;
            UpdateHealthUI(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
        }

        // Hook restart button
        _restartButton?.onClick.AddListener(() => ArenaGameManager.Instance?.RestartGame());

        // Hide end screen
        _endScreen?.SetActive(false);

        // Find an enemy to track for the debug overlay
        _trackedEnemy = FindObjectOfType<EnemyAI>();
    }

    private void Update()
    {
        UpdateAIDebugOverlay();
    }

    // ── Health ───────────────────────────────────────────────

    private void UpdateHealthUI(float current, float max)
    {
        if (_healthSlider) _healthSlider.value = current / max;
        if (_healthText) _healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";
    }

    // ── Enemy Count ──────────────────────────────────────────

    public void UpdateEnemyCount(int count)
    {
        if (_enemyCountText) _enemyCountText.text = $"Enemies: {count}";
    }

    // ── AI Debug Overlay ─────────────────────────────────────

    private void UpdateAIDebugOverlay()
    {
        if (_trackedEnemy == null)
        {
            _trackedEnemy = FindObjectOfType<EnemyAI>();
            return;
        }

        // State label
        if (_aiStateText)
        {
            string state = _trackedEnemy.CurrentStateName;
            string color = state switch
            {
                "Patrol" => "#00FFAA",
                "Chase" => "#FFD700",
                "Attack" => "#FF4444",
                _ => "#FFFFFF"
            };
            _aiStateText.text = $"AI: <color={color}>{state}</color>";
        }

        // Learning progress
        var learning = _trackedEnemy.Context?.Learning;
        if (learning != null)
        {
            float fill = (float)learning.MemoryCount / learning.MemoryCapacity;
            if (_learningSlider) _learningSlider.value = fill;
            if (_learningText)
                _learningText.text = $"Memory: {learning.MemoryCount}/{learning.MemoryCapacity}  Bias: {learning.LearningBias:F2}";
        }
    }

    // ── End Screen ───────────────────────────────────────────

    public void ShowEndScreen(bool playerWon)
    {
        if (_endScreen) _endScreen.SetActive(true);
        if (_endTitleText) _endTitleText.text = playerWon ? "YOU WIN!" : "YOU DIED";
    }
}