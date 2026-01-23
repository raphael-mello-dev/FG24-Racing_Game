using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using static Debug_AttributeDisplay;

public class Debug_AttributeDisplay : MonoBehaviour
{
    public Display[] displays;
    public GameObject textPrefab;

    private void Start()
    {
        foreach(var display in displays)
        {
            var g = Instantiate(textPrefab, transform);
            display.text = g.GetComponent<TextMeshProUGUI>();
            display.Init();
            g.name = display.variblePosibilities[display.varibleName];
            g.SetActive(true);
        }
    }


    private void Update()
    {
        foreach(var display in displays)
        {
            display.DrawField();
        }
    }

    private void OnValidate()
    {
        foreach (var display in displays)
        {
            if(display.component != null)
            {
                FieldInfo[] fields = display.component.GetType().GetFields(BindingFlags.GetProperty);
                string[] fieldNames = new string[fields.Length];
                for(int i = 0; i < fields.Length; i++)
                {
                    fieldNames[i] = fields[i].Name;
                }
                display.variblePosibilities = fieldNames;
            }
        }
    }
    [Serializable]
    public class Display
    {
        public Component component;
        public int varibleName;
        public string[] variblePosibilities;
        public string format = "{0}";
        
        [HideInInspector] public TextMeshProUGUI text;
        private FieldInfo _field;


        public void Init()
        {
            _field = component.GetType().GetField(variblePosibilities[varibleName], BindingFlags.NonPublic);
            
        }

        public void DrawField()
        {
            object varible = _field.GetValue(component);
            string toString = varible.ToString();

            text.text = string.Format(format, toString);
        }
    }
}
