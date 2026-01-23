using UnityEngine;

using System.Collections.Generic;
public class UI_Selector : MonoBehaviour
{
    public List<GameObject> collection;

    int _selected = -1;
    public int Selected 
    { 
        get { return _selected; } 
        set { _selected = value; UpdateSelection(); } 
    }

    private void UpdateSelection()
    {
        for(int i=0;i<collection.Count; i++)
        {
            collection[i].SetActive(i == _selected);
        }
    }
    public void SetSelected(int index)
    {
        Selected = index;
    }

}
