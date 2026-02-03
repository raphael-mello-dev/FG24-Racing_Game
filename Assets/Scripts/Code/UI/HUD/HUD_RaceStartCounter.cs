using System.Collections;
using UnityEngine;
using TMPro;

public class HUD_RaceStartCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI startText;
    private float time = 3f;

    void Update()
    {
        if (RaceSettingsManager.Instance.CurrentMode == RaceMode.Training)
        {
            gameObject.SetActive(false);
            return;
        }

        if (time <= 0.1)
        {
            startText.text = "Start";
            GameManager.Instance.StateManager.SwitchState<GameplayState>();
            StartCoroutine("RaceStart");
        }
        else
        {
            startText.text = $"{(int)(time + 1)}";
            time -= Time.deltaTime;
        }
    }

    private IEnumerator RaceStart()
    {
        yield return new WaitForSecondsRealtime(0.3f);
        gameObject.SetActive(false);
    }
}