using UnityEngine;
using UnityEngine.UI;

public class UI_HoverIcon : MonoBehaviour
{
    public Image image;
    public Transform showBase;
    public float speed;

    private void Start()
    {
        SetImage(image.sprite);
    }
    void Update()
    {
        if(image.sprite != null)
            showBase.Rotate(Vector3.up, Time.deltaTime * speed, Space.Self);
    }

    public void SetImage(Sprite sprite)
    {
        image.sprite = sprite;
        if (image.sprite == null)
        {
            transform.localScale = Vector3.zero;
        }
        else
        {

            transform.localScale = Vector3.one;
        }
    }
}
