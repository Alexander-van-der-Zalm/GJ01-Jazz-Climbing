using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomPropertyDrawer(typeof(AudioLibrarySampleNamesAttribute))]
public class AudioLibrarySampleNamesAttributeDrawer : PropertyDrawer 
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //base.OnGUI(position, property, label);
        var attrib = attribute as AudioLibrarySampleNamesAttribute;

        int index = attrib.list.IndexOf(property.stringValue);

        if (index < 0) index = 0;

        index = EditorGUI.Popup(position, label.text, index, attrib.list.ToArray());

        property.stringValue = attrib.list[index];
    }
}
