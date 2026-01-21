using TMPro;
using UnityEngine;

public class UI_Follower : MonoBehaviour
{
    protected static RectTransform _visualContent;
    private RectTransform _target;
    private RectTransform _rectTransform;

    public float linearSpeed = 100;
    public float exponentialSpeed = 6;

    void Start()
    {
        if(_visualContent == null)
            _visualContent = GameObject.Find("VisualContent").GetComponent<RectTransform>();

        _rectTransform = GetComponent<RectTransform>();

        _target = transform.parent.GetComponent<RectTransform>();
        _rectTransform.SetParent(_visualContent, true);

    }

    private void Update()
    {
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
    }


}
