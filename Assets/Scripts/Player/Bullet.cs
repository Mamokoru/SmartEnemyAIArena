// ============================================================
//  Bullet.cs
//  Attach to a Bullet prefab that has a Rigidbody + Collider.
//  Deals damage to EnemyHealth on collision.
// ============================================================
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float _damage      = 25f;
    [SerializeField] private float _lifetime    = 4f;
    [SerializeField] private GameObject _hitFxPrefab;   // optional particle

    private void Start() => Destroy(gameObject, _lifetime);

    private void OnCollisionEnter(Collision col)
    {
        // Spawn hit FX
        if (_hitFxPrefab)
            Instantiate(_hitFxPrefab, col.contacts[0].point, Quaternion.identity);

        // Damage enemy
        if (col.gameObject.TryGetComponent<EnemyHealth>(out var eh))
            eh.TakeDamage(_damage);

        Debug.Log($"Bullet hit {col.gameObject.name} for {_damage} damage.");
        Destroy(gameObject);
    }
}
