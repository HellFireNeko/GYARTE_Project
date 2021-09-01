using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DiscordTestBehavior))]
public class DiscordEditorWindow : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DiscordTestBehavior behavior = (DiscordTestBehavior)target;

        if (GUILayout.Button("Host") && Application.isPlaying)
        {
            behavior.Button();
        }
    }
}
