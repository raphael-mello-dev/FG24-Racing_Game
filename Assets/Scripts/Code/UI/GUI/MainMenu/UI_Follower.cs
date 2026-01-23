using Unity.VisualScripting;
using UnityEngine;

public class UI_Follower : MonoBehaviour
{
    protected static RectTransform _visualContent;
    private RectTransform _target;
    private RectTransform _rectTransform;

    public float linearSpeed = 100;
    public float exponentialSpeed = 6;

    bool _activeLastFrame = false;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_visualContent == null)
            _visualContent = GameObject.Find("VisualContent").GetComponent<RectTransform>();

        _ = Init();
    }

    private async Awaitable Init()
    {
        _target = transform.parent.GetComponent<RectTransform>();

        await Awaitable.NextFrameAsync();
        _rectTransform.SetParent(_visualContent, true);

        _rectTransform.position = _target.position;
        _rectTransform.rotation = _target.rotation;
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

            Vector3 cur = _rectTransform.localPosition;
            _rectTransform.localPosition = new Vector3(cur.x, cur.y, 0);

            _rectTransform.rotation = _target.rotation;
        }
        var active = _target.gameObject.activeInHierarchy;
        if(_activeLastFrame != active)
        {
            transform.localScale = active ? Vector3.one : Vector3.zero;
            if(active)
            {
                _rectTransform.position = _target.position;
                _rectTransform.rotation = _target.rotation;
            }
            _activeLastFrame = active;

        }
        
    }
}