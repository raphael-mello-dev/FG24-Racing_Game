using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Follower : MonoBehaviour
{
    protected static RectTransform _visualContent;
    private RectTransform _target;
    private Image _colorCopy;
    private RectTransform _rectTransform;
    private TextMeshProUGUI _text;


    public float linearSpeed = 100;
    public float exponentialSpeed = 6;

    void Start()
    {
        if(_visualContent == null)
            _visualContent = GameObject.Find("VisualContent").GetComponent<RectTransform>();

        _rectTransform = GetComponent<RectTransform>();
        _colorCopy = transform.parent.parent.GetComponent<Image>();
        _text = GetComponent<TextMeshProUGUI>();

        _ = Init();
    }
    private async Awaitable Init()
    {
        await Awaitable.NextFrameAsync();
        _target = transform.parent.GetComponent<RectTransform>();
        _rectTransform.SetParent(_visualContent, true);
    }
    

    private void Update()
    {
        if (_target == null)
            return;

        Vector3 target = _target.position;

        if(_rectTransform.position != target)
        {
            _rectTransform.position = Vector3.Lerp(
            _rectTransform.position, target,
            1 - Mathf.Pow(2, -Time.unscaledDeltaTime * exponentialSpeed));

            //Linear movement
            _rectTransform.position = Vector3.MoveTowards(
                _rectTransform.position, target,
                Time.unscaledDeltaTime * linearSpeed);
        }

        _text.color = _colorCopy.color;

        transform.localScale = _target.gameObject.activeInHierarchy ? Vector3.one : Vector3.zero;
    }


}
