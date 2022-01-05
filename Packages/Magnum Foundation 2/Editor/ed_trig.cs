using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using UnityEditorInternal;
using MagnumFoundation2.Objects;
using MagnumFoundation2.System;

namespace MagnumFoundation2Editor
{
    //https://blog.terresquall.com/2020/03/creating-reorderable-lists-in-the-unity-inspector/
    [CustomEditor(typeof(ev_script), true)]
    public class ed_trig : Editor
    {
        //The array property we will edit
        public SerializedProperty wave;

        //The Reorderable list we will be working with
        public ReorderableList list;

        public void OnEnable()
        {
            //Gets the wave property in WaveManager so we can access it. 
            wave = serializedObject.FindProperty("elements");

            //Initialises the ReorderableList. We are creating a Reorderable List from the "wave" property. 
            //In this, we want a ReorderableList that is draggable, with a display header, with add and remove buttons        
            list = new ReorderableList(serializedObject, wave, true, true, true, true);

            list.drawElementCallback = DrawListItems;
            list.drawHeaderCallback = DrawHeader;
        }
        public int GetFeildInt(SerializedProperty element, string elProperty)
        {
            return element.FindPropertyRelative(elProperty).intValue;
        }

        public void DrawLabel(ref Rect rect, string label, float width, ref float sep)
        {
            EditorGUI.LabelField(new Rect(rect.x + sep, rect.y, 170, EditorGUIUtility.singleLineHeight), label);
            sep += 10 * label.Length;
            rect.height += EditorGUIUtility.singleLineHeight;
            sep += width + 10;
        }
        public void DrawFeild(ref Rect rect, SerializedProperty element, string elProperty, string label, float width, ref float sep)
        {
            EditorGUI.LabelField(new Rect(rect.x + sep, rect.y, 170, EditorGUIUtility.singleLineHeight), label);
            sep += 10 * label.Length;
            EditorGUI.PropertyField(
                new Rect(rect.x + sep, rect.y, width, EditorGUIUtility.singleLineHeight),
                element.FindPropertyRelative(elProperty),
                GUIContent.none
            );
            rect.height += EditorGUIUtility.singleLineHeight;
            sep += width + 10;
        }

        public virtual void DrawCustomEvents(string evType, ref Rect rect, ref SerializedProperty element, ref float sep)
        {
        }

        public virtual void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index); //The element in the list

            // Create a property field and label field for each property. 
            list.elementHeight = EditorGUIUtility.singleLineHeight * 4f;
            float sep = 0;
            // The 'mobs' property. Since the enum is self-evident, I am not making a label field for it. 
            // The property field for mobs (width 100, height of a single line)
            DrawFeild(ref rect, element, "eventType", "Type", 100, ref sep);
            DrawFeild(ref rect, element, "simultaenous", "Simultaneous", 30, ref sep);
            DrawFeild(ref rect, element, "simlutaneousDelay", "Delay", 60, ref sep);
            sep = 0;
            rect.y += EditorGUIUtility.singleLineHeight * 1.35f;

            switch ((EVENT_TYPES)element.FindPropertyRelative("eventType").intValue)
            {
                case EVENT_TYPES.WAIT:
                    DrawFeild(ref rect, element, "float0", "Timer", 100, ref sep);
                    break;

                case EVENT_TYPES.CHANGE_MAP:
                    DrawFeild(ref rect, element, "string0", "Map name", 100, ref sep);
                    DrawFeild(ref rect, element, "tpPoint", "Map letter (a-g)", 100, ref sep);
                    DrawFeild(ref rect, element, "boolean", "Disable player", 100, ref sep);
                    break;

                case EVENT_TYPES.SOUND:
                    DrawFeild(ref rect, element, "sound", "Sound", 100, ref sep);
                    break;

                case EVENT_TYPES.OBJECT_DIRECTION:
                    {
                        DrawFeild(ref rect, element, "int0", "Object ID", 100, ref sep);
                        DrawFeild(ref rect, element, "float0", "X", 100, ref sep);
                        DrawFeild(ref rect, element, "float1", "Y", 100, ref sep);
                        int ind = GetFeildInt(element, "int0");
                        {
                            ev_script evScript = (ev_script)target;
                            if (ind < evScript.objectReferences.Count)
                            {
                                DrawLabel(ref rect, evScript.objectReferences[ind], 35, ref sep);
                            }
                        }
                    }
                    break;

                case EVENT_TYPES.BIG_TEXT:
                    DrawFeild(ref rect, element, "string0", "Map name", 100, ref sep);
                    break;

                case EVENT_TYPES.CUSTOM_FUNCTION:
                    DrawFeild(ref rect, element, "funcName", "Function name", 160, ref sep);
                    DrawFeild(ref rect, element, "string0", "String Name", 100, ref sep);

                    sep = 0;
                    rect.y += EditorGUIUtility.singleLineHeight * 1.35f;

                    DrawFeild(ref rect, element, "string1", "String Name", 100, ref sep);
                    DrawFeild(ref rect, element, "scrObj", "Object Name", 100, ref sep);
                    DrawFeild(ref rect, element, "int0", "Integer Name", 100, ref sep);
                    DrawCustomEvents(element.FindPropertyRelative("funcName").stringValue, ref rect, ref element, ref sep);
                    break;

                case EVENT_TYPES.ANIMATION:

                    DrawFeild(ref rect, element, "string0", "Anim name", 100, ref sep);
                    DrawFeild(ref rect, element, "boolean", "Loop", 60, ref sep);
                    break;

                case EVENT_TYPES.DIALOGUE:

                    DrawFeild(ref rect, element, "string0", "Text", 200, ref sep);
                    DrawFeild(ref rect, element, "string1", "Speaker name", 100, ref sep);
                    DrawFeild(ref rect, element, "float0", "Timer", 60, ref sep);
                    break;

                case EVENT_TYPES.MOVEMNET:
                    {
                        DrawFeild(ref rect, element, "boolean2", "Use new system?", 30, ref sep);
                        DrawFeild(ref rect, element, "int0", "Character index (new system only)", 30, ref sep);
                        int ind = GetFeildInt(element, "int0");
                        {
                            ev_script evScript = (ev_script)target;
                            if (ind < evScript.objectReferences.Count)
                            {
                                DrawLabel(ref rect, evScript.objectReferences[ind], 35, ref sep);
                            }
                        }

                        DrawFeild(ref rect, element, "boolean", "Teleport", 30, ref sep);
                        DrawFeild(ref rect, element, "string0", "Object Name", 100, ref sep);
                        DrawFeild(ref rect, element, "pos", "Position", 90, ref sep);
                    }
                    break;

                case EVENT_TYPES.RUN_CHARACTER_SCRIPT:

                    DrawFeild(ref rect, element, "boolean", "Enabled", 30, ref sep);
                    DrawFeild(ref rect, element, "string0", "Object Name", 100, ref sep);
                    break;

                case EVENT_TYPES.FADE:

                    DrawFeild(ref rect, element, "boolean", "Immediate", 30, ref sep);
                    DrawFeild(ref rect, element, "colour", "Colour", 120, ref sep);
                    break;

                case EVENT_TYPES.ADD_CHOICE_OPTION:
                    DrawFeild(ref rect, element, "string0", "String Name", 100, ref sep);
                    DrawFeild(ref rect, element, "scrObj", "Object Name", 100, ref sep);
                    break;

                case EVENT_TYPES.CALL_SCRIPT:
                    DrawFeild(ref rect, element, "scrObj", "Script", 100, ref sep);
                    break;

                case EVENT_TYPES.JUMP_TO_LABEL:
                    DrawFeild(ref rect, element, "string0", "Label to jump to", 100, ref sep);
                    break;

                case EVENT_TYPES.CHECK_FLAG:
                    DrawFeild(ref rect, element, "string0", "Check flag", 100, ref sep);
                    DrawFeild(ref rect, element, "logicType", "Check flag", 80, ref sep);
                    DrawFeild(ref rect, element, "int0", "", 35, ref sep);
                    DrawFeild(ref rect, element, "string1", "Label", 100, ref sep);
                    break;

                case EVENT_TYPES.SET_FLAG:
                    DrawFeild(ref rect, element, "string0", "Flag name", 100, ref sep);
                    DrawFeild(ref rect, element, "int0", "Flag value", 35, ref sep);
                    break;

                case EVENT_TYPES.LABEL:
                    DrawFeild(ref rect, element, "string0", "Label", 100, ref sep);
                    break;
            }
        }

        void DrawHeader(Rect rect)
        {
            string name = "Events";
            EditorGUI.LabelField(rect, name);
        }

        //This is the function that makes the custom editor work
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

    }

}
