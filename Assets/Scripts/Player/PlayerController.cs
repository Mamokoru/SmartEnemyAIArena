// ============================================================
//  PlayerController.cs
//  WASD movement, mouse-aim via ground raycast, LMB to shoot.
//  Uses Unity's legacy Input system — swap Input.GetAxis /
//  Input.GetButtonDown for the new Input System if preferred.
// ============================================================
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour, GameInputs.IPlayerActions
{
    // ── Inspector ─────────────────────────────────────────────
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _gravity = -15f;

    [Header("Shooting")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _fireRate = 0.25f;
    [SerializeField] private float _bulletForce = 25f;

    [Header("Aim")]
    [SerializeField] private LayerMask _groundLayer;

    // ── Private ───────────────────────────────────────────────
    private CharacterController _cc;
    private Vector3 _velocity;
    private float _nextFireTime;
    private bool _isShooting;
    private Vector2 _movementVector;
    private GameInputs _inputs;

    // ── Unity Lifecycle ───────────────────────────────────────
    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Confined;
    }
    private void Start()
    {
        _inputs = new GameInputs();
        _inputs.Player.AddCallbacks(this);
        _inputs.Enable();

    }
    private void OnDisable()
    {
        _inputs.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleAim();
        HandleShooting();
    }

    // ── Movement ─────────────────────────────────────────────

    private void HandleMovement()
    {
        float h = _movementVector.x;/* Input.GetAxisRaw("Horizontal");*/
        float v = _movementVector.y;/* Input.GetAxisRaw("Vertical");*/

        Vector3 moveDir = new Vector3(h, 0f, v).normalized;
        _cc.Move(moveDir * _moveSpeed * Time.deltaTime);

        // Gravity
        if (_cc.isGrounded && _velocity.y < 0f) _velocity.y = -2f;
        _velocity.y += _gravity * Time.deltaTime;
        _cc.Move(_velocity * Time.deltaTime);
    }

    // ── Aim ──────────────────────────────────────────────────

    private void HandleAim()
    {
        if (Mouse.current == null) { return; }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.value);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, _groundLayer))
        {
            Vector3 lookTarget = hit.point;
            lookTarget.y = transform.position.y;
            Vector3 dir = lookTarget - transform.position;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ── Shooting ─────────────────────────────────────────────

    private void HandleShooting()
    {
        if (!_isShooting) return;
        if (Time.time < _nextFireTime) return;
        if (_bulletPrefab == null) return;

        _nextFireTime = Time.time + _fireRate;

        Vector3 pos = _firePoint ? _firePoint.position : transform.position + transform.forward;
        Quaternion rot = _firePoint ? _firePoint.rotation : transform.rotation;

        GameObject bullet = Instantiate(_bulletPrefab, pos, rot);
        if (bullet.TryGetComponent<Rigidbody>(out var rb))
            rb.AddForce(rot * Vector3.forward * _bulletForce, ForceMode.Impulse);
    }

    // -InputHandler for new Input System would go here, if using that instead of legacy Input.

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        _movementVector = input;
        // Use input.x for horizontal and input.y for vertical movement
    }

    public void OnLook(InputAction.CallbackContext context)
    {
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        _isShooting = context.ReadValueAsButton();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
    }

    public void OnJump(InputAction.CallbackContext context)
    {
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
    }

    public void OnNext(InputAction.CallbackContext context)
    {
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
    }
}
