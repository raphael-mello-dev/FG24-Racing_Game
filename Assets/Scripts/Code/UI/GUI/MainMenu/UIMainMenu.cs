using UnityEngine;
using UnityEngine.UI;

public class UIMainMenu : MonoBehaviour
{
    public void StartRace()
    {
        RaceSettingsManager.Instance.SetOponentsAmount();
        GameManager.Instance.LoadScene(Scenes.Gameplay);
    }
    public void ToMainMenu()
    {
        GameManager.Instance.LoadScene(Scenes.MainMenu);
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