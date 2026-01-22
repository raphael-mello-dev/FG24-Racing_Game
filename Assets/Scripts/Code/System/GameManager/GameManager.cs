using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Game manager singleton reference
    public static GameManager Instance { get; private set; }

    // FSM instance for Game flow
    public StateMachine StateManager { get; private set; }

    // TODO - RACE SETTINGS

    // Game manager setup
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        StateManager = new StateMachine();
        StateManager.SwitchState<MenuState>();

        // Game manager getting able to travel through scenes
        DontDestroyOnLoad(gameObject);
    }

    void Update() => StateManager.OnUpdate();
}