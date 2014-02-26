using UnityEngine;
using System.Collections;

//C#
public static class AppHelper
{
    #if UNITY_WEBPLAYER
        public static string webplayerQuitURL = "http://google.com";
    #endif

    public static void Quit()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBPLAYER
                Application.OpenURL(webplayerQuitURL);
        #else
                Application.Quit();
        #endif
    }

    public static void Pause(bool paused = true)
    {
        AudioListener.pause = paused;
        Time.timeScale = paused ? 0.0f : 1.0f;

        Debug.Log(AudioListener.pause + " ts  " + Time.timeScale);
    }
}
