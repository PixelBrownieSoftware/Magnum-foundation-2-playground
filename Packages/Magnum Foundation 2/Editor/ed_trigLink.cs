using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace MagnumFoundation2Editor
{
    [CustomEditor(typeof(s_triglink), true)]
    public class ed_trigLink : Editor
    {
        s_triglink link;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            link = (s_triglink)target;


        }
    }
}

