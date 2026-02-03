using UnityEngine;
using UnityEngine.EventSystems;

public class UI_SetShowcaseIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite image;
    private UI_HoverIcon _hoverIcon;

    void Start()
    {
        _hoverIcon = FindFirstObjectByType<UI_HoverIcon>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverIcon.SetImage(image);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverIcon.SetImage(null);
    }
}
