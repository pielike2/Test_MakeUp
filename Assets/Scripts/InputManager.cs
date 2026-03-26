using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, IInputManager
{
    public static IInputManager instance { get; private set; }
    private void Awake()
    {
        //Устанавлаиаем Синглтон
        if (instance != null && instance is InputManager manager)
            Destroy(manager);
        instance = this;
    }

    public Vector2 currentPosition { get; private set; }

    Mobile_Action inputActions;

    private void OnEnable()
    {
        inputActions = new Mobile_Action();
        inputActions.Touch.Enable();

        inputActions.Touch.Position.performed += UpdatePosition;
    }

    private void OnDisable()
    {
        inputActions.Touch.Position.performed -= UpdatePosition;
    }

    private void UpdatePosition(InputAction.CallbackContext context)
    {
        currentPosition = context.ReadValue<Vector2>();
    }
}
