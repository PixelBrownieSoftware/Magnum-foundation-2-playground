using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Timeline;
using MagnumFoundation2.Objects;
using MagnumFoundation2.System;

namespace MagnumFoundation2.Objects
{
    [CreateAssetMenu(fileName = "event", menuName = "Scripted Event")]
    public class ev_script : ScriptableObject
    {
        public List<string> objectReferences;
        public List<ev_details> elements;
    }
    
    [Serializable]
    public struct ev_integer {
        public int integer;
        public string integer_name;
    }
    public enum EVENT_TYPES
    {
        MOVEMNET,
        DIALOGUE,
        SET_HEALTH,
        RUN_CHARACTER_SCRIPT,
        ANIMATION,
        SOUND,
        CUSTOM_FUNCTION,
        SET_FLAG,
        CHECK_FLAG,
        CAMERA_MOVEMENT,
        BREAK_EVENT,
        JUMP_TO_LABEL,
        SET_UTILITY_FLAG,
        FADE,
        CREATE_OBJECT,
        DISPLAY_CHARACTER_HEALTH,
        UTILITY_INITIALIZE,
        UTILITY_CHECK,
        WAIT,
        CALL_SCRIPT,
        CHANGE_SCENE,
        PUT_SHUTTERS,
        DISPLAY_IMAGE,
        SHOW_TEXT,
        CHANGE_MAP,
        ENABLE_DISABLE_OBJECT,
        DELETE_OBJECT,
        MAIN_MENU,
        ADD_CHOICE_OPTION,
        CLEAR_CHOICES,
        PRESENT_CHOICES,
        SAVE_DATA,
        SET_SWITCH,
        SET_DIALOGUE_SPEAKER,
        LABEL,
        FADE_SPRITE,
        BIG_TEXT,
        OBJECT_DIRECTION,
        CHANGE_MUSIC
    }
    [Serializable]
    public class ev_details
    {
        public int logic;
        public EVENT_TYPES eventType;
        public string funcName;
        public bool simultaenous;
        public float simlutaneousDelay;
        public s_camera.CAMERA_MODE camMode;

        public int jump;
        public Vector2 pos;
        public Color colour;
        public int int0;
        public int int1;
        public int int2;
        public char tpPoint;
        public bool boolean;
        public bool boolean1;
        public bool boolean2;
        public string string0;
        public string string1;
        public float posX;
        public float posY;  //So the editor can read it
        public float float0;
        public float float1;
        public string[] stringList;
        public s_dialogue_choice[] dialogueChoices;
        public int[] intList;
        public ScriptableObject scrObj;
        public s_object objectToControl;
        public enum LOGIC_TYPE
        {
            VAR_GREATER,
            VAR_EQUAL,
            VAR_LESS,
            VAR_NOT_EQUAL,
            ITEM_OWNED,
            CHECK_UTILITY_RETURN_NUM,
            NONE
        }
        public LOGIC_TYPE logicType;
        public TimelineAsset timelineAsset;
        public AudioClip sound;
    }

}
