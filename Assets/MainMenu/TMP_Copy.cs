using TMPro;
using UnityEngine;

public class TMP_Copy : MonoBehaviour
{
    public GameObject copyFrom;
    private TextMeshProUGUI _text;

    private void Start()
    {
        if (_text == null)
            _text = GetComponent<TextMeshProUGUI>();
        if (copyFrom != null)
        {
            TextMeshProUGUI text = copyFrom.GetComponent<TextMeshProUGUI>();

            if (text != null)
                _text.text = text.text;
            else
                _text.text = copyFrom.name;
        }
    }

    private void OnValidate()
    {
        if(_text == null)
            _text = GetComponent<TextMeshProUGUI>();
        if(copyFrom != null)
        {
            TextMeshProUGUI text = copyFrom.GetComponent<TextMeshProUGUI>();

            if(text != null)
                _text.text = text.text;
            else 
                _text.text = copyFrom.name;
        }
    }
}
