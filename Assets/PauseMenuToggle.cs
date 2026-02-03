using UnityEngine;

public class PauseMenuToggle : MonoBehaviour
{
    public bool isPaused = false;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            ApplyPause();
        }
    }

    private void ApplyPause()
    {
        Time.timeScale = isPaused ? 0.0f : 1.0f;
        transform.GetChild(0).gameObject.SetActive(isPaused);
    }

    private void OnDestroy()
    {
        isPaused = false;
        ApplyPause();
    }
}
