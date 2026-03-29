// ============================================================
//  EnemyAIDebugOverlay.cs
//  Renders a world-space label above the enemy showing current
//  AI state and learning fill.  Visible in Game view — great
//  for recording portfolio demos without needing extra UI.
//
//  Requires: GameObject with GUIStyle drawn via OnGUI.
//  Attach to the Enemy prefab root.
// ============================================================
using UnityEngine;

public class EnemyAIDebugOverlay : MonoBehaviour
{
    [SerializeField] private bool  _enabled      = true;
    [SerializeField] private float _labelOffsetY = 2.4f;     // above capsule

    private EnemyAI   _ai;
    private GUIStyle  _style;
    private Camera    _cam;

    private void Awake()
    {
        _ai  = GetComponent<EnemyAI>();
        _cam = Camera.main;
    }

    private void OnGUI()
    {
        if (!_enabled || _ai == null || _cam == null) return;

        // Lazy-init style (can't do in Awake for GUI)
        if (_style == null)
        {
            _style = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize  = 14,
            };
            _style.normal.textColor = Color.white;
        }

        // Project world pos to screen pos
        Vector3 worldPos  = transform.position + Vector3.up * _labelOffsetY;
        Vector3 screenPos = _cam.WorldToScreenPoint(worldPos);

        // Don't draw if behind camera
        if (screenPos.z < 0f) return;

        // Flip Y (GUI origin is top-left, screen origin is bottom-left)
        screenPos.y = Screen.height - screenPos.y;

        // State color
        _style.normal.textColor = _ai.CurrentStateName switch
        {
            "Patrol" => new Color(0f, 1f, 0.7f),
            "Chase"  => new Color(1f, 0.85f, 0f),
            "Attack" => new Color(1f, 0.25f, 0.25f),
            _        => Color.white,
        };

        string stateLine    = $"[ {_ai.CurrentStateName.ToUpper()} ]";
        string learningLine = "";

        var learning = _ai.Context?.Learning;
        if (learning != null)
        {
            int filled   = learning.MemoryCount;
            int capacity = learning.MemoryCapacity;
            string bar   = "[" + new string('█', filled) + new string('░', capacity - filled) + "]";
            learningLine = $"\nMem {bar}";
        }

        Rect rect = new Rect(screenPos.x - 80f, screenPos.y - 30f, 160f, 60f);
        GUI.Label(rect, stateLine + learningLine, _style);
    }
}
