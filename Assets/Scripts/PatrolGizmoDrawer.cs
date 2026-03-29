// ============================================================
//  PatrolGizmoDrawer.cs
//  Draws the patrol route as a connected loop in the Scene view.
//  Attach to the Waypoints parent GameObject.
//  Zero runtime cost — editor-only.
// ============================================================
using UnityEngine;

public class PatrolGizmoDrawer : MonoBehaviour
{
    [SerializeField] private Color _lineColor   = new Color(0f, 1f, 0.5f, 0.8f);
    [SerializeField] private Color _nodeColor   = new Color(0f, 1f, 0.5f, 1f);
    [SerializeField] private float _nodeRadius  = 0.3f;
    [SerializeField] private bool  _showLabels  = true;

    private void OnDrawGizmos()
    {
        int count = transform.childCount;
        if (count == 0) return;

        Gizmos.color = _lineColor;

        for (int i = 0; i < count; i++)
        {
            Transform a = transform.GetChild(i);
            Transform b = transform.GetChild((i + 1) % count);   // loop back to 0

            // Line between waypoints
            Gizmos.DrawLine(a.position, b.position);

            // Node sphere
            Gizmos.color = _nodeColor;
            Gizmos.DrawSphere(a.position, _nodeRadius);
            Gizmos.color = _lineColor;

#if UNITY_EDITOR
            // Label
            if (_showLabels)
                UnityEditor.Handles.Label(
                    a.position + Vector3.up * 0.5f,
                    $"WP {i}");
#endif
        }
    }
}
