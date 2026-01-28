using UnityEngine;


//------------------------------------------------------------------------
//
//  This script was created by Milo. If you have questions or problems, ask her. 
//
//------------------------------------------------------------------------

public class SceneOnlySingleton<T> : MonoBehaviour where T : SceneOnlySingleton<T>
{
    public static T _instance;
    public static T instance { 
        get
        {
            if(_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                    throw new System.Exception("No instance of type exists.");

                if (!_instance.hasInit)
                    _instance.Init();
            }
            return _instance;
        }
        protected set => _instance = value; }

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
        if (!hasInit)
            Init();

    }

    protected bool hasInit = false;
    protected virtual void Init()
    {
        if (hasInit)
            return;

        hasInit = true;
    }

    private void OnDestroy()
    {
        instance = null;
    }
}
