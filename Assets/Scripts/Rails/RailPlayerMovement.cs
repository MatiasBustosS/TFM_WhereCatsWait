using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class RailPlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference movement;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference interact;

    [Header("Riel (un solo contenedor con varias splines)")]
    [SerializeField] private TrackRail rail;
    [SerializeField] private int currentSpline = 0;
    [Range(0f, 1f)] [SerializeField] private float currentT = 0f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = -20f;

    [Header("Selección de rama")]
    [Tooltip("Qué tan alineado debe estar el input con una rama (0-1) para poder tomarla.")]
    [Range(0f, 1f)] [SerializeField] private float junctionInputThreshold = 0.5f;
    [Tooltip("Bonus para seguir en la spline actual y evitar cambios accidentales.")]
    [Range(0f, 0.5f)] [SerializeField] private float currentSplineBias = 0.1f;

    [Header("Colisión en el Riel")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleCheckRadiusMultiplier = 0.95f;

    [Header("Visual & Interaction")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject playerModel;

    private Vector2 _movementInput;
    public Vector2 MovementInput => _movementInput;

    private bool _jumpRequested;
    private bool canMove = true;

    private CharacterController _controller;
    private Vector3 _verticalVelocity;

    private float _animSpeed;
    private RaycastHit _hit;

    private readonly List<TrackRail.Branch> _branches = new();
    private float _moveSign = 1f; // +1 avanza en t, -1 retrocede

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        if (rail != null)
        {
            // Desactivamos el controller un instante para poder teletransportar sin conflictos
            _controller.enabled = false;
            transform.position = rail.GetPosition(currentSpline, currentT);
            _controller.enabled = true;
        }
    }

    private void OnEnable()
    {
        movement.action.Enable();
        jump.action.Enable();
        interact.action.Enable();

        movement.action.performed += OnMove;
        movement.action.canceled += OnMove;

        jump.action.performed += OnJump;
        jump.action.canceled += OnJump;

        interact.action.started += OnInteract;
    }

    private void OnDisable()
    {
        movement.action.performed -= OnMove;
        movement.action.canceled -= OnMove;

        jump.action.performed -= OnJump;
        jump.action.canceled -= OnJump;

        interact.action.started -= OnInteract;

        movement.action.Disable();
        jump.action.Disable();
        interact.action.Disable();
    }

    private bool isTurning;
    private Vector3 _lastFacingDir = Vector3.zero;

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        _movementInput = context.ReadValue<Vector2>();
    }

    // El root ya rota hacia la dirección real de avance, así que el modelo
    // NO se gira 180° aparte (eso causaba el doble giro). Solo se dispara la animación.
    private IEnumerator TurnAnimCooldown()
    {
        isTurning = true;
        animator?.SetTrigger("Turn");
        yield return new WaitForSeconds(.33f);
        isTurning = false;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        if (context.performed)
        {
            _jumpRequested = true;
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (npc != null)
        {
            GameManager.instance.SetNpc(npc);
            SetCanMove(false);

            if (GameManager.instance.IsTalking)
            {
                GameManager.instance.DialogueManager.NextDialogue();
            }
            else
            {
                npc.Talk();
                GameManager.instance.ChangeTalking(true);
            }
        }
    }

    private void Update()
    {
        if (!canMove) return;

        if (HudManager.Instance != null && HudManager.Instance.IsTarget)
        {
            return;
        }

        HandleRailMovement();
        HandleGravity();
        HandleJump();
        HandleInteract();

        animator?.SetBool("IsGround", _controller.isGrounded);
    }

    private void HandleRailMovement()
    {
        if (rail == null) return;

        bool moved = false;
        Vector2 input = _movementInput;

        if (input.sqrMagnitude > 0.01f)
        {
            // 1) Ramas posibles desde la posición actual
            rail.GetBranchesAt(currentSpline, currentT, _branches);

            Vector3 inputWorld = InputToWorld(input);

            if (inputWorld.sqrMagnitude > 0.0001f)
            {
                inputWorld.Normalize();

                // 2) Elegimos la rama que mejor coincide con el input
                bool found = false;
                TrackRail.Branch best = default;
                float bestScore = junctionInputThreshold;

                foreach (var b in _branches)
                {
                    float score = Vector3.Dot(b.direction, inputWorld);
                    if (b.spline == currentSpline) score += currentSplineBias;

                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = b;
                        found = true;
                    }
                }

                // 3) Si no hay rama en esa dirección, el movimiento queda restringido
                if (found)
                {
                    // Cambio de spline sin salto: misma posición, otra spline
                    if (best.spline != currentSpline)
                    {
                        currentSpline = best.spline;
                        currentT = best.t;
                    }

                    Vector3 tangent = rail.GetTangent(currentSpline, currentT);
                    _moveSign = Vector3.Dot(best.direction, tangent) >= 0f ? 1f : -1f;

                    float length = rail.GetLength(currentSpline);
                    if (length > 0f)
                    {
                        float deltaT = (_moveSign * moveSpeed * Time.deltaTime) / length;
                        float candidateT = Mathf.Clamp01(currentT + deltaT);

                        Vector3 candidatePos = rail.GetPosition(currentSpline, candidateT);
                        Vector3 move = candidatePos - transform.position;
                        move.y = 0f;

                        float moveDistance = move.magnitude;
                        bool blocked = moveDistance > 0.0001f &&
                                       IsPathBlocked(transform.position, move / moveDistance, moveDistance);

                        if (!blocked)
                        {
                            currentT = candidateT;
                            _controller.Move(move);
                            moved = moveDistance > 0.0001f;
                        }
                    }
                }
            }
        }

        float targetSpeed = moved ? 1f : 0f;
        _animSpeed = Mathf.MoveTowards(_animSpeed, targetSpeed, 10f * Time.deltaTime);
        animator?.SetFloat("HSpeed", _animSpeed);

        // Solo giramos mientras realmente avanzamos; si no, conservamos la orientación
        if (moved) RotateTowardsRailForward(currentT);
    }

    // Input 2D -> dirección en mundo (relativa a la cámara, aplanada al plano XZ)
    private Vector3 InputToWorld(Vector2 input)
    {
        Transform cam = playerCamera != null ? playerCamera.transform : transform;
        Vector3 right = Vector3.ProjectOnPlane(cam.right, Vector3.up).normalized;
        Vector3 fwd = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        return right * input.x + fwd * input.y;
    }

    private bool IsPathBlocked(Vector3 fromPosition, Vector3 direction, float distance)
    {
        float radius = _controller.radius * obstacleCheckRadiusMultiplier;
        Vector3 center = fromPosition + _controller.center;
        Vector3 bottom = center + Vector3.up * (radius - _controller.height / 2f);
        Vector3 top = center + Vector3.up * (_controller.height / 2f - radius);

        return Physics.CapsuleCast(bottom, top, radius, direction, distance, obstacleLayer);
    }

    private float rotationSpeed = 720f;

    private void RotateTowardsRailForward(float t)
    {
        // Dirección real de avance = tangente firmada según el sentido en que nos movemos
        Vector3 forward = rail.GetTangent(currentSpline, t) * _moveSign;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) return;
        forward.Normalize();

        // Inversión de sentido -> animación de giro
        if (_lastFacingDir != Vector3.zero &&
            Vector3.Dot(_lastFacingDir, forward) < -0.5f &&
            _controller.isGrounded && !isTurning)
        {
            StartCoroutine(TurnAnimCooldown());
        }
        _lastFacingDir = forward;

        Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (_controller.isGrounded && _verticalVelocity.y < 0)
        {
            _verticalVelocity.y = -2f;
        }

        _verticalVelocity.y += gravity * Time.deltaTime;
        _controller.Move(_verticalVelocity * Time.deltaTime);

        animator?.SetFloat("VSpeed", _controller.isGrounded ? 0f : _verticalVelocity.y);
    }

    private void HandleJump()
    {
        if (!_jumpRequested) return;

        if (_controller.isGrounded)
        {
            _verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator?.SetTrigger("Jump");
        }

        _jumpRequested = false;
    }

    private void HandleInteract()
    {
        if (HudManager.Instance == null || HudManager.Instance.IsTarget) return;

        Physics.SphereCast(transform.position, 2f, Vector3.one, out _hit, 5f);
    }

    private NPCDialogue npc;

    public void SetNPC(NPCDialogue npc)
    {
        this.npc = npc;
    }

    public void SetCanMove(bool move)
    {
        canMove = move;
        if (!move)
        {
            animator?.SetFloat("HSpeed", 0f);
            _movementInput = Vector2.zero;
        }
    }

    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 5f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
}