using System;
using System.Linq.Expressions;
using UnityEngine;

public class UI_Selector : MonoBehaviour
{

    public Member[] members;
    public int selected = -1;



    private void Start()
    {
        SelectMember(selected);
    }

    public void SelectMember(int member)
    {
        if (selected == member)
            member = -1;
        selected = member;
        for (int i = 0; i<members.Length;i++)
        {
            members[i].Set(i==member);
        }
        
    }

    [Serializable]
    public class Member
    {
        public Animator[] objs;
        public UI_Selector[] selectors; 

        public bool set = false;

        public void Set(bool active)
        {
            if (set == active)
                return;
            set = active;

            for(int i = 0; i < objs.Length; i++)
            {
                objs[i].gameObject.SetActive(active);
                if (active)
                {
                    //_ = Animate(0, "Waiting", objs[i]);
                    objs[i].PlayInFixedTime("Activated", 0, (float)i / 8f);
                }
                else
                {
                    objs[i].Play("Waiting");
                }
            }
        if (!active)
            for (int i = 0;i < selectors.Length; i++)
            {
                selectors[i].SelectMember(-1);
            }
        }

    }
}
