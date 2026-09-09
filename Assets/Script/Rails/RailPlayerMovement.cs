using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class RailPlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference movement;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference interact;

    [Header("Riel Actual")]
    [SerializeField] private TrackRail currentRail;
    [Range(0f, 1f)] [SerializeField] private float currentT = 0f;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float switchSpeed = 10f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float gravity = -20f;
    
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
    private bool _isTransitioning = false;
    private Vector3 _targetTransitionPos;

    private float _animSpeed;
    private RaycastHit _hit;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        if (animator == null) animator = GetComponent<Animator>();

        
        if (currentRail != null)
        {
            transform.position = currentRail.GetPositionAt(currentT);
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

        interact.action.performed += OnInteract;
        interact.action.canceled += OnInteract;
    }

    private void OnDisable()
    {
        movement.action.performed -= OnMove;
        movement.action.canceled -= OnMove;

        jump.action.performed -= OnJump;
        jump.action.canceled -= OnJump;

        interact.action.performed -= OnInteract;
        interact.action.canceled -= OnInteract;

        movement.action.Disable();
        jump.action.Disable();
        interact.action.Disable();
    }

    private bool isTurning;
    private bool sign = true;
    public void OnMove(InputAction.CallbackContext context)
    {
        
        if (!canMove) return;
        _movementInput = context.ReadValue<Vector2>();
        
        if (_movementInput.x != 0 && _controller.isGrounded)
        {
            bool desiredDirection = Mathf.Sign(_movementInput.x) > 0;

            if (desiredDirection != sign && !isTurning)
            {
                StartCoroutine(TurnMesh(desiredDirection));
            }
        }

    }

    private IEnumerator TurnMesh(bool right)
    {
        isTurning = true;

        animator.SetTrigger("Turn");

        yield return new WaitForSeconds(.33f);

        playerModel.transform.localRotation = right ? Quaternion.Euler(0f, 0f, 0f) : Quaternion.Euler(0f, 180f, 0f);

        sign = right;
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
        if (!canMove) return;
        if (HudManager.Instance != null && HudManager.Instance.IsTarget) return;

        animator?.SetTrigger("Interact");

        if (_hit.collider != null && _hit.collider.CompareTag("Puzzle"))
        {
            var puzzleManager = _hit.collider.GetComponent<PuzzleManager>();
            if (puzzleManager != null)
            {
                if (puzzleManager.solved) return;

                animator?.SetFloat("HSpeed", 0);
                HudManager.Instance.OpenPuzzle(puzzleManager.puzzleType);
                puzzleManager.Initialize();
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
        if (currentRail == null) return;

        if (_isTransitioning)
        {
            Vector3 nextPos = Vector3.MoveTowards(transform.position, _targetTransitionPos, switchSpeed * Time.deltaTime);
            _controller.Move(nextPos - transform.position);
            RotateTowardsRailForward(currentT);
            
            if (Vector3.Distance(transform.position, _targetTransitionPos) < 2f)
            {
                _isTransitioning = false;
            }
            return;
        }

        float inputX = _movementInput.x;
        float candidateT = currentT;

        if (Mathf.Abs(inputX) > 0.01f)
        {
            float railLength = currentRail.GetLength();
            if (railLength > 0f)
            {
                float deltaT = (inputX * moveSpeed * Time.deltaTime) / railLength;
                candidateT = Mathf.Clamp01(currentT + deltaT);
            }
        }

        float targetSpeed = Mathf.Abs(inputX) > 0.1f ? 1f : 0f;
        _animSpeed = Mathf.Lerp(_animSpeed, targetSpeed, 10f * Time.deltaTime);
        animator?.SetFloat("HSpeed", _animSpeed);

        Vector3 candidatePos = currentRail.GetPositionAt(candidateT);
        Vector3 currentPos = transform.position;
        Vector3 moveVector = candidatePos - currentPos;
        moveVector.y = 0f;
        float moveDistance = moveVector.magnitude;

        if (moveDistance > 0.0001f && IsPathBlocked(currentPos, moveVector.normalized, moveDistance)) return;

        currentT = candidateT;

        Vector3 horizontalMove = candidatePos - transform.position;
        horizontalMove.y = 0;
        _controller.Move(horizontalMove);

        RotateTowardsRailForward(currentT);
    }
    
    private bool IsPathBlocked(Vector3 fromPosition, Vector3 direction, float distance)
    {
        float radius = _controller.radius * obstacleCheckRadiusMultiplier;
        Vector3 center = fromPosition + _controller.center;
        Vector3 bottom = center + Vector3.up * (radius - _controller.height / 2f);
        Vector3 top = center + Vector3.up * (_controller.height / 2f - radius);

        return Physics.CapsuleCast(bottom, top, radius, direction, distance, obstacleLayer);
    }
    
    private Vector3 GetRailForwardDirection(float t)
    {
        float tBehind = Mathf.Clamp01(t - tangentSampleDelta);
        float tAhead = Mathf.Clamp01(t + tangentSampleDelta);

        return currentRail.GetPositionAt(tAhead) - currentRail.GetPositionAt(tBehind);
    }

    private float rotationSpeed = 720f;
    private float tangentSampleDelta = 0.01f;
    
    private void RotateTowardsRailForward(float t)
    {
        Vector3 forward = GetRailForwardDirection(t);
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) return;

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

    public void SwitchToRail(TrackRail newRail, float newT)
    {
        if (newRail == null || newRail == currentRail) return;

        currentRail = newRail;
        currentT = newT;
        _targetTransitionPos = currentRail.GetPositionAt(currentT);
        _isTransitioning = true;
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