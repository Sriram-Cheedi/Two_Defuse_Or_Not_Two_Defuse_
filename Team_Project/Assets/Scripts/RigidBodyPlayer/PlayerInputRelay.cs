using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputRelay : MonoBehaviour
{
    private PlayerInput input;
    
    public event Action<Vector2> Move;
    public event Action<Vector2> Look;
    public event Action JumpPressed;
    public event Action<bool> Crouch;
    public event Action<bool> InvertGravity;
    public event Action<int> NumberPressed;
    public event Action RespawnRequested;
    public event Action GuessSubmitted;
    private InputAction move, look, jump, crouch, invertGravity, enter;
    private InputAction keypressNumbers;

    public Vector2 MoveValue { get; private set; }
    public Vector2 LookValue { get; private set; }
    public bool CrouchValue { get; private set; }
    public bool InvertGravityValue { get; private set; }
    public bool IsJumpHeld => jump.IsPressed();

    void Awake()
    {
        input =  GetComponent<PlayerInput>();
        
        move = input.actions["Move"];
        look = input.actions["Look"];
        jump = input.actions["Jump"];
        crouch = input.actions["Crouch"];
        invertGravity = input.actions["InvertGravity"];
        keypressNumbers = input.actions["NumKey"];
        enter = input.actions["Enter"];
    }

    private void OnEnable()
    {
        move.performed += OnMove;
        move.canceled += OnMove;
        
        look.performed += OnLook;
        look.canceled += OnLook;
        
        jump.performed += OnJump;
        jump.canceled += OnJump;
        
        crouch.performed += OnCrouch;
        crouch.canceled += OnCrouch;
        
        invertGravity.performed += OnInvertGravity;
        invertGravity.canceled += OnInvertGravity;
        
        keypressNumbers.performed += OnNumberInput;
        
        enter.performed += OnEnter;
        enter.canceled += OnEnter;
    }

    private void OnDisable()
    {
        move.performed -= OnMove;
        move.canceled -= OnMove;
        
        look.performed -= OnLook;
        look.canceled -= OnLook;
        
        jump.performed -= OnJump;
        jump.canceled -= OnJump;
        
        crouch.performed -= OnCrouch;
        crouch.canceled -= OnCrouch;
        
        invertGravity.performed -= OnInvertGravity;
        invertGravity.canceled -= OnInvertGravity;
        
        keypressNumbers.performed -= OnNumberInput;
        
        enter.performed -= OnEnter;
        enter.canceled -= OnEnter;
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        MoveValue = ctx.ReadValue<Vector2>();
        Move?.Invoke(MoveValue);
    }

    private void OnLook(InputAction.CallbackContext ctx)
    {
        LookValue = ctx.ReadValue<Vector2>();
        Look?.Invoke(LookValue);
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) JumpPressed?.Invoke();
    }

    private void OnCrouch(InputAction.CallbackContext ctx)
    {
        CrouchValue = ctx.ReadValueAsButton();
        Crouch?.Invoke(CrouchValue);
    }

    private void OnInvertGravity(InputAction.CallbackContext ctx)
    {
        InvertGravityValue = ctx.ReadValueAsButton();
        InvertGravity?.Invoke(InvertGravityValue);
    }

    private void OnNumberInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.ReadValueAsButton()) return;
        string keyname = ctx.control.name;
        char lastChar = keyname[keyname.Length - 1];
        Debug.Log(lastChar);

        if (char.IsDigit(lastChar))
        {

            int digit = (int)char.GetNumericValue(lastChar);
            NumberPressed?.Invoke(digit);
        }
    }
    
    private void OnEnter(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) GuessSubmitted?.Invoke();
    }

    public void CallRepawn()
    {
        RespawnRequested?.Invoke();
    }

    
}
