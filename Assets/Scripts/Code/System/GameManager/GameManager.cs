using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Game manager singleton reference
    public static GameManager Instance { get; private set; }

    // FSM instance for Game flow
    //TODO

    private void Awake()
    {
        // Singleton initialization
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Game manager getting able to travel through scenes
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        
    }
}