using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyAttributeDrawer:PropertyDrawer 
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.LabelField(position, label.text, getValue(property));
    }

    private string getValue(SerializedProperty property)
    {
        string value = "";
        switch (property.type)
        {
            case "string":
                value = property.stringValue;
                break;
            case "int":
                value = property.intValue.ToString();
                break;
            case "float":
                value = property.floatValue.ToString();
                break;
            case "bool":
                value = property.boolValue.ToString();
                break;
            default:
                value = "RO addme: " + property.type;
                break;
        }
        return value;
    }
}
