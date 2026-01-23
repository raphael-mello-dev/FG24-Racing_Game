using TMPro;
using UnityEngine;
using static Debug_AttributeDisplay;

public class HUD_Player : MonoBehaviour
{
    public GameObject textPrefab;
    public Rigidbody rigidbody;
    private TextMeshProUGUI _speed_text;

    private CheckpointManager.Racer _racer;
    private TextMeshProUGUI _position_text;
    private TextMeshProUGUI _lap_text;

    private void Start()
    {
        _speed_text = CreateText("Speed");
        _position_text = CreateText("Position");
        _lap_text = CreateText("Lap");
    
        _racer = CheckpointManager.instance.DEBUG_FOCUSED_RACER_INFO;

        UpdateText();
    }


    private void Update()
    {
        UpdateText();
    }
    public void UpdateText()
    {
        _speed_text.text = $"{rigidbody.linearVelocity.magnitude} M/S";
        _position_text.text = $"Position {_racer.racePosition} / ?";
        _lap_text.text = $"Lap {_racer.lapCount} / ?";
    }

    public TextMeshProUGUI CreateText(string name)
    {
        var g = Instantiate(textPrefab, transform);
        g.SetActive(true);
        g.name = name;

        return g.GetComponent<TextMeshProUGUI>();
    }
}
