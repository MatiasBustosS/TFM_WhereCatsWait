using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class PlayerMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference movement;
    [SerializeField] private InputActionReference jump;
    [SerializeField] private InputActionReference interact;
    
    [Header("Movement")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float moveSpeed = 100f;
    
    [Header("Visual")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Animator animator;
    
    
    private Vector2 _movement;
    private float _axis;
    private bool _jump;
    private bool _canJump;
    
    private float actualSpeed;
    private float verticalSpeed;
    
    private CharacterController _controller;
    
    private bool canMove = true;
    
    
    void Start()
    {
        _controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }
    
    
    private void OnEnable()
    {
        movement.action.Enable();
        jump.action.Enable();
        interact.action.Enable();
        
        movement.action.performed += OnMove;
        jump.action.performed += OnJump;
        interact.action.performed += OnInteract;
        
        movement.action.canceled += OnMove;
        jump.action.canceled += OnJump;
        interact.action.canceled += OnInteract;
    }
    
    private void OnDisable()
    {
        movement.action.performed -= OnMove;
        jump.action.performed -= OnJump;
        interact.action.performed -= OnInteract;
        
        movement.action.canceled -= OnMove;
        jump.action.canceled -= OnJump;
        interact.action.canceled -= OnInteract;
        
        movement.action.Disable();
        jump.action.Disable();
        interact.action.Disable();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        //_movement = context.ReadValue<Vector2>();
        _axis = context.ReadValue<float>();
        print(_axis);
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        if (context.performed)
        {
            _jump = true;
        }
    }
    RaycastHit _hit;
    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!canMove) return;
        if(HudManager.Instance.IsTarget) return;
        
        animator?.SetTrigger("Interact");
        
        if (_hit.collider !=null && _hit.collider.CompareTag("Puzzle") && _hit.collider.GetComponent<PuzzleManager>() != null)
        {
            if (_hit.collider.GetComponent<PuzzleManager>().solved)
            {
                print("Was solved");
                return;
            }
            
            animator?.SetFloat("HSpeed", 0);
            HudManager.Instance.OpenPuzzle(_hit.collider.GetComponent<PuzzleManager>().puzzleType);
            _hit.collider.GetComponent<PuzzleManager>().Initialize();
        }
    }


    private float _currentXRotation;
    private float _currentYRotation;
    
    private float _gravity = -20f;

    private Vector3 velocity;
    
    void Update()
    {
        if (!canMove) return;
        
        Move();
        HandleGravity();
        HandleJump();
        
        animator?.SetBool("isGround", _controller.isGrounded);
    }
    

    private float _animSpeed;

    private void Move()
    {
        if (HudManager.Instance.IsTarget)
        {
            return;
        }

        //Vector3 move = transform.right * _movement.x / 2 + transform.forward * _movement.y;
        //Vector3 move = transform.forward * _movement.x;
        Vector3 move = transform.forward * _axis;
        

        _controller.Move(move * (moveSpeed * Time.fixedDeltaTime));

        float targetSpeed = move.magnitude > 0.1f ? 1f : 0f;
        _animSpeed = Mathf.Lerp(_animSpeed, targetSpeed, 10f * Time.deltaTime);
        animator?.SetFloat("HSpeed", _animSpeed);
    }

    
    private void HandleGravity()
    {
        
        if (_controller.isGrounded && velocity.y < 0) velocity.y = 0f;
        
        velocity.y += _gravity * Time.deltaTime;

        _controller.Move(velocity * Time.deltaTime);
        animator?.SetFloat("VSpeed", _controller.isGrounded ? 0f : velocity.y);
        
    }
    
    private void HandleJump()
    {
        if (!_jump) return;

        if (_controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * _gravity);
            animator?.SetTrigger("Jump");
        }
        
        _jump = false;
    }
    
    
    private void HandleInteract()
    {
        if(HudManager.Instance.IsTarget) return;

        //HudManager.Instance.OnTarget(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out _hit, 5f) && _hit.collider.CompareTag("Puzzle"));
        //HudManager.Instance.OnTarget(Physics.SphereCast(transform.position, 2f, Vector3.one, out _hit, 5f) && _hit.collider.CompareTag("Puzzle"));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward*5);
        
        Gizmos.DrawWireSphere(transform.position, 2f);
    }

    public void SetCanMove(bool move)
    {
        canMove = move;
        if (!move)
        {
            animator?.SetFloat("HSpeed", 0f);
        }
    }
}
