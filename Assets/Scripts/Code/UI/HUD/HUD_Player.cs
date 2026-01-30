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

    private int _playerCount;
    private int _lapCount;

    private void Start()
    {
        _speed_text = CreateText("Speed");
        _position_text = CreateText("Position");
        _lap_text = CreateText("Lap");
    
        _racer = CheckpointManager.instance.DEBUG_FOCUSED_RACER_INFO;
        if(_racer == null)
            _racer = CheckpointManager.GetRacerInfo(FindFirstObjectByType<CarController>().transform);

        rigidbody = _racer.transform.GetComponent<Rigidbody>();

        UpdateText();

        _playerCount = RaceManager.instance.racerCount;
        _lapCount = RaceManager.instance.lapCount;
    }


    private void Update()
    {
        UpdateText();
    }
    public void UpdateText()
    {
        _speed_text.text = string.Format("{0:N0} M/S", rigidbody.linearVelocity.magnitude);
        _position_text.text = string.Format("Position {0} / {1}", _racer.racePosition, _playerCount);
        _lap_text.text = string.Format("Lap {0} / {1}", _racer.lapCount, _lapCount);
    }

    public TextMeshProUGUI CreateText(string name)
    {
        var g = Instantiate(textPrefab, transform);
        g.SetActive(true);
        g.name = name;

        return g.GetComponent<TextMeshProUGUI>();
    }
}
