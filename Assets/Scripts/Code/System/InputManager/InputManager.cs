using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum InputMap
{
    Menu,
    Gameplay,
    Pause
}

public class InputManager
{
    private Controls controls;

    #region Car inputs

    public float Move => controls.Gameplay.Move.ReadValue<float>();
    public float Steer => controls.Gameplay.Steer.ReadValue<float>();
    public float Boost => controls.Gameplay.Boost.ReadValue<float>();
    public float HandBrake => controls.Gameplay.HandBrake.ReadValue<float>();

    #endregion

    public event Action OnPause;
    public event Action OnUnpause;

    public InputManager()
    {
        controls = new Controls();
        controls.Gameplay.Enable();

        controls.Gameplay.Pause.performed += PausePerformed;
        controls.Pause.Unpause.performed += UnpausePerformed;
    }

    // Switch between input action maps
    public void SwitchInputMap(InputMap map)
    {
        // Disabling all input maps
        controls.Menu.Disable();
        controls.Gameplay.Disable();
        controls.Pause.Disable();

        // Activating only chosen input map
        switch (map)
        {
            case InputMap.Menu: controls.Menu.Enable(); break;

            case InputMap.Gameplay: controls.Gameplay.Enable(); break;

            case InputMap.Pause: controls.Pause.Enable(); break;
        }
    }

    private void PausePerformed(InputAction.CallbackContext context)
    {
        GameManager.Instance.StateManager.SwitchState<PauseState>();
        SwitchInputMap(InputMap.Pause);
        //OnPause?.Invoke();
    }

    private void UnpausePerformed(InputAction.CallbackContext context)
    {
        GameManager.Instance.StateManager.SwitchState<GameplayState>();
        SwitchInputMap(InputMap.Gameplay);
        //OnUnpause?.Invoke();
    }
}