using UnityEngine;


//------------------------------------------------------------------------
//
//  This script was created by Milo. If you have questions or problems, ask her. 
//
//------------------------------------------------------------------------

public class SceneOnlySingleton<T> : MonoBehaviour where T : SceneOnlySingleton<T>
{
    public static T instance { get; protected set; }

    protected virtual void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            throw new System.Exception("An instance of this singleton already exists.");
        }
        else
        {
            instance = (T)this;
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
