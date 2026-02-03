using UnityEngine;
using UnityEngine.SceneManagement;

public enum Scenes
{
    MainMenu = 0,
    Gameplay = 1
}

public class GameManager : MonoBehaviour
{
    // Game manager singleton reference
    public static GameManager Instance { get; private set; }

    // FSM instance for Game flow
    public StateMachine StateManager { get; private set; }

    // 
    public InputManager InputManager { get; private set; }

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
        InputManager = new InputManager();

        // Game manager getting able to travel through scenes
        DontDestroyOnLoad(gameObject);
    }

    void Update() => StateManager.OnUpdate();

    // Function for loading desired scene
    public void LoadScene(Scenes scene) => SceneManager.LoadScene((int)scene);
}