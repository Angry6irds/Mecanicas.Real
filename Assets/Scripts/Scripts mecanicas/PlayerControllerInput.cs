using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerController3P))]
public class PlayerControllerInput : MonoBehaviour
{
    private PlayerController3P controller;
    

    void Awake()
    {
        controller = GetComponent<PlayerController3P>();
        
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        controller.SetMoveInput(ctx.ReadValue<Vector2>());
    }

    public void OnThrow(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) controller.TryThrow();
    }

    public void OnCycleLeft(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) controller.CycleTarget(-1);
    }

    public void OnCycleRight(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) controller.CycleTarget(+1);
    }

    // Solo si usarás parry con sartén
    public void OnParry(InputAction.CallbackContext ctx)
    {

    }
}