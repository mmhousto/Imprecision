using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputErrorWorkaround : MonoBehaviour
{
    public InputActionAsset actionAsset;
    private PlayerInput Input;

    private void Start()
    {
        Input = GetComponent<PlayerInput>();
        Input.actions = actionAsset;
    }

    private void OnDisable()
    {
        Input.actions = null;
    }
}