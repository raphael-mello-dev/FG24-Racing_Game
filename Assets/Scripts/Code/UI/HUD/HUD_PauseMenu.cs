using System.Collections;
using UnityEngine;

public class HUD_PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject playerHUD;

    private void OnEnable()
    {
        GameManager.Instance.InputManager.OnPause += PauseMenuActivation;
        GameManager.Instance.InputManager.OnUnpause += PauseMenuActivation;
    }
    
    private void OnDisable()
    {
        GameManager.Instance.InputManager.OnPause -= PauseMenuActivation;
        GameManager.Instance.InputManager.OnUnpause -= PauseMenuActivation;
    }

    private void Start() => StartCoroutine("GetPlayerHUD");

    private IEnumerator GetPlayerHUD()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        playerHUD = FindFirstObjectByType<HUD_Player>().gameObject;
    }

    public void PauseMenuActivation()
    {
        panel.SetActive(!panel.activeSelf);
        playerHUD.SetActive(!playerHUD.activeSelf);
    }

    public void ToMainMenu()
    {
        GameManager.Instance.InputManager.SwitchInputMap(InputMap.Menu);
        GameManager.Instance.LoadScene(Scenes.MainMenu);
    }

    public void Resume()
    {
        GameManager.Instance.InputManager.SwitchInputMap(InputMap.Gameplay);
        GameManager.Instance.StateManager.SwitchState<GameplayState>();
        Time.timeScale = 1.0f;
    }

    public void ClickRetry()
    {
        RaceSettingsManager.Instance.SetOponentsAmount();
        GameManager.Instance.LoadScene(Scenes.Gameplay);
    }

    public void ClickExit()
    {
        // Quit the game if you are playing it in Unity
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;

        // Quit the game of you are playing it in the game application
        #endif
            Application.Quit();
    }
}