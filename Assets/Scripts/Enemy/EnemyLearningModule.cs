// ============================================================
//  EnemyLearningModule.cs
//
//  THE TWIST — Simple Pattern Learning
//  ─────────────────────────────────────────────────────────
//  The enemy records the ANGLE (relative to the player's
//  forward) from which each successful hit was landed.
//  When choosing the next attack position, it scores N
//  candidate positions and PREFERS angles near historical
//  successes — so it gradually learns to approach from the
//  player's blind side or whichever angle landed before.
//
//  Interview talking point:
//  "It's not ML — it's a weighted scoring heuristic on a
//   ring buffer of successful angles.  Cheap, deterministic,
//   and surprisingly effective at feeling 'smart'."
// ============================================================
using System.Collections.Generic;
using UnityEngine;

public class EnemyLearningModule : MonoBehaviour
{
    // ── Inspector ────────────────────────────────────────────
    [Header("Learning")]
    [Tooltip("How many past successful angles we remember")]
    [SerializeField] private int   _memoryCapacity      = 8;
    [Tooltip("Higher = stronger preference for learned angles")]
    [SerializeField] private float _learningBias        = 0.6f;
    [Tooltip("Candidate positions evaluated when picking attack spot")]
    [SerializeField] private int   _candidateCount      = 12;

    [Header("Debug")]
    [SerializeField] private bool  _drawGizmos = true;

    // ── Private ──────────────────────────────────────────────
    // Ring buffer of angles (degrees, 0–360) relative to player forward
    private readonly Queue<float> _successAngles = new();
    private Vector3 _lastChosenPosition;

    // ── Public API ───────────────────────────────────────────

    /// <summary>
    /// Call this when an attack successfully damages the player.
    /// attackerPos: world position of the enemy at time of hit.
    /// playerTransform: the player.
    /// </summary>
    public void RecordSuccessfulAttack(Vector3 attackerPos, Transform playerTransform)
    {
        float angle = WorldToRelativeAngle(attackerPos, playerTransform);

        if (_successAngles.Count >= _memoryCapacity)
            _successAngles.Dequeue();

        _successAngles.Enqueue(angle);
        Debug.Log($"[Learning] Recorded success angle: {angle:F1}°  Memory: {_successAngles.Count}/{_memoryCapacity}");
    }

    /// <summary>
    /// Returns the best world-space position from which to attack,
    /// biased toward historically successful angles.
    /// </summary>
    public Vector3 ChooseBestAttackPosition(Transform self, Transform player, float attackRadius)
    {
        // If no memory yet, return a naive approach position
        if (_successAngles.Count == 0)
        {
            _lastChosenPosition = GetPositionAtAngle(player, attackRadius,
                                                     Random.Range(0f, 360f));
            return _lastChosenPosition;
        }

        Vector3 bestPos   = self.position;
        float   bestScore = float.MinValue;

        for (int i = 0; i < _candidateCount; i++)
        {
            float candidateAngle = (360f / _candidateCount) * i;
            Vector3 candidate    = GetPositionAtAngle(player, attackRadius, candidateAngle);

            float score = ScoreCandidate(candidate, candidateAngle, self.position, player);

            if (score > bestScore)
            {
                bestScore = score;
                bestPos   = candidate;
            }
        }

        _lastChosenPosition = bestPos;
        return bestPos;
    }

    // ── Scoring ──────────────────────────────────────────────

    private float ScoreCandidate(Vector3 candidatePos, float candidateAngle,
                                  Vector3 selfPos, Transform player)
    {
        // 1. Base score: prefer positions closer to enemy (less travel)
        float distScore = 1f - Mathf.Clamp01(
            Vector3.Distance(candidatePos, selfPos) / 20f);

        // 2. Learning score: proximity of this angle to past successes
        float learnScore = 0f;
        foreach (float remembered in _successAngles)
        {
            float delta = Mathf.Abs(Mathf.DeltaAngle(candidateAngle, remembered));
            // Full bonus at 0°, falls off to 0 at 90°
            learnScore += Mathf.Max(0f, 1f - delta / 90f);
        }
        // Normalize by memory count so bonus stays in [0,1]
        learnScore /= _memoryCapacity;

        return distScore * (1f - _learningBias) + learnScore * _learningBias;
    }

    // ── Geometry Helpers ─────────────────────────────────────

    /// <summary>
    /// Returns a world position at 'radius' distance from the player,
    /// at 'angleDeg' relative to the player's forward vector.
    /// </summary>
    private Vector3 GetPositionAtAngle(Transform player, float radius, float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;

        // Use player's local axes so "0°" = directly in front of player
        Vector3 offset = player.forward * Mathf.Cos(rad) * radius
                       + player.right   * Mathf.Sin(rad) * radius;

        return player.position + offset;
    }

    /// <summary>
    /// Converts attacker world-position to an angle in [0,360)
    /// relative to the player's forward direction.
    /// </summary>
    private float WorldToRelativeAngle(Vector3 attackerPos, Transform player)
    {
        Vector3 dir = (attackerPos - player.position).normalized;
        float angle = Vector3.SignedAngle(player.forward, dir, Vector3.up);
        return (angle + 360f) % 360f;
    }

    // ── Public Accessors (for debug / UI) ────────────────────
    public int  MemoryCount => _successAngles.Count;
    public int  MemoryCapacity => _memoryCapacity;
    public float LearningBias  => _learningBias;

    // ── Gizmos ───────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        if (!_drawGizmos || _lastChosenPosition == Vector3.zero) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_lastChosenPosition, 0.3f);
        Gizmos.DrawLine(transform.position, _lastChosenPosition);
    }
}
