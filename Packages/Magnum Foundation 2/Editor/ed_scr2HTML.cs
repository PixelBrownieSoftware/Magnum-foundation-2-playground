using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(s_eventsToHTML), true)]
public class ed_scr2HTML : Editor
{
    //
    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Convert to HTML")) {
            s_eventsToHTML ev =(s_eventsToHTML)target;
            ev.ScriptToHTML();
        }
        base.OnInspectorGUI();
    }
}
