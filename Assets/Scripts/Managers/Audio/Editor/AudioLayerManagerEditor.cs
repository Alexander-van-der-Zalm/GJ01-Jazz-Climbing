using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(AudioLayerManager))]
public class AudioLayerManagerEditor : EditorPlus
{
    private string description = "The layers are based on the hardcoded AudioLayersEnum.\n The enum can be found in the AudioLayerManager.cs file. This might change in the future.";
    private string settingsNameTooltip = "The layer names are hardcoded in the AudioLayerManager";

    public override void OnInspectorGUI()
    {
        AudioLayerManager manager = target as AudioLayerManager;

        int count = manager.audioLayerSettings.Count;
        if (count == 0)
            manager.Init();

        if(SavedFoldout("Description"))
            EditorGUILayout.TextArea(description);
        
        for (int i = 0; i < count; i++)
        {
            AudioLayerSettings settings = manager.audioLayerSettings[i]; 
            GUIContent name = new GUIContent("AudioLayer: " + settings.Layer.ToString() + " Clips: " + settings.ClipsPlaying.ToString(), settingsNameTooltip);

            if (SavedFoldout(name, i))
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                {
                    settings.Volume = EditorGUILayout.Slider("Volume: ",settings.Volume, 0, 1);
                    settings.Mute = EditorGUILayout.Toggle("Mute: ", settings.Mute);
                    settings.MaxClips = EditorGUILayout.IntField("MaxClips: ", settings.MaxClips);
                }
                EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck())
                {
                    AudioManager.Mute(settings.Layer);
                    AudioManager.UpdateVolume(settings.Layer);
                }
            }
        }
    }
}
