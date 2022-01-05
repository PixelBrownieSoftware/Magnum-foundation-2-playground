using System.IO;
using MagnumFoundation2.System;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using MagnumFoundation2.System.Core;

namespace MagnumFoundation2.System
{
    public class s_mainmenu : MonoBehaviour
    {
        public static dat_save legacySave;
        public static object save;
        public static bool isload = false;
        public GUISkin menuskins;
        bool option_pressed = false;
        static bool createdSingletons = false;

        public Vector2 menuOffsetPosition;
        public Vector2 optionsOffsetPosition;
        public float seperator;

        public float play_button_size = 90;
        public float load_button_size = 90;
        public float option_button_size = 90;

        public Vector2 menuButtonOffset = new Vector2(0,0);

        public Image fade;

        [Serializable]
        public struct strBool
        {
            public strBool(string string0, bool boolean, string descTrue, string descFalse)
            {
                this.boolean = boolean;
                this.descTrue = descTrue;
                this.descFalse = descFalse;
                this.string0 = string0;
            }
            public string string0;
            public bool boolean;
            public string descTrue;
            public string descFalse;
        }

        public strBool[] _buttonList;
        public string[] _keyList;

        public string mainGameLoad = "Main Game";
        public char areaPartLoad = 'a';

        string currentKeyName = "none";
        KeyValuePair<string, KeyCode> keyToChange = new KeyValuePair<string, KeyCode>("none", KeyCode.None);

        public enum MENU_MODE
        {
            NONE,
            MENU,
            OPTIONS
        };
        MENU_MODE menu = MENU_MODE.NONE;
        public enum MENU_LOAD_MODE
        {
            START,
            LOAD
        };
        MENU_LOAD_MODE menuLoad = MENU_LOAD_MODE.START;

        public virtual bool SaveExist()
        {
            return File.Exists(s_globals.saveDataName);
        }

        public static T GetSaveFile<T>() where T : dat_save
        {
            return (T)save;
        }

        public virtual void LoadSave<T>() where T : dat_save
        {
            isload = true;
            FileStream fs = new FileStream(s_globals.saveDataName, FileMode.Open);
            BinaryFormatter bin = new BinaryFormatter();

            save = (T)bin.Deserialize(fs);

            fs.Close();
            if (s_globals.globalSingle == null)
            {
                //s_globals.GetInstance().
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(mainGameLoad);
            }
            else
            {
                //s_globals.globalSingle.InitializeManager();
            }
        }

        public virtual void StartGame() {

        }

        public void Awake()
        {
            s_globals.allowPause = false;
            StartCoroutine(FadeOut());
        }

        public IEnumerator FadeIn()
        {
            option_pressed = true;
            float t = 0;
            yield return StartCoroutine(s_triggerhandler.trigSingleton.Fade(Color.black));

            switch (menuLoad)
            {
                case MENU_LOAD_MODE.START:

                    isload = false;
                    s_globals.GetInstance().StartStuff();
                    s_globals.GetInstance().tpPointFlag = areaPartLoad;
                    if (!createdSingletons)
                        UnityEngine.SceneManagement.SceneManager.LoadScene(mainGameLoad);
                    else
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(mainGameLoad);
                        if (s_globals.globalSingle == null)
                        {
                            
                        }
                        else
                        {
                            //s_mapManager.LevEd.InitializeManager();
                        }
                    }
                    option_pressed = true;
                    break;

                case MENU_LOAD_MODE.LOAD:
                    CallLoadSave();
                    isload = true;
                    createdSingletons = true;
                    option_pressed = true;
                    s_globals.GetInstance().isMainGame = true;
                    s_globals.GetInstance().StartStuff();
                    s_globals.GetInstance().LoadSaveData();
                    dat_save sa = (dat_save)save;
                    UnityEngine.SceneManagement.SceneManager.LoadScene(sa.currentmap);
                    break;
            }
        }


        public IEnumerator FadeOut()
        {
            yield return StartCoroutine(s_triggerhandler.trigSingleton.Fade(Color.clear));
            menu = MENU_MODE.MENU;
        }

        public virtual void CallLoadSave()
        {

        }

        public void DisplayConfigOptions()
        {
            if (_keyList != null)
            {

                foreach (string key in _keyList)
                {
                    KeyCode keyCod = (KeyCode)PlayerPrefs.GetInt(key);
                    if (GUILayout.Button(key + ": " + keyCod.ToString(), menuskins.GetStyle("Box")))
                    {
                        currentKeyName = key;
                    }
                }
            }
            if (_buttonList != null)
            {
                for (int i = 0; i < _buttonList.Length; i++)
                {
                    strBool key = _buttonList[i];
                    int b = PlayerPrefs.GetInt(key.string0);
                    if (b == 0)
                    {
                        if (GUILayout.Button(key.string0 + ": " + key.descFalse, menuskins.GetStyle("Box")))
                        {
                            PlayerPrefs.SetInt(key.string0, 1);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(key.string0 + ": " + key.descTrue, menuskins.GetStyle("Box")))
                        {
                            PlayerPrefs.SetInt(key.string0, 0);
                        }
                    }
                }
            }
        }

        public void OnGUI()
        {
            Event e = Event.current;
            if (!option_pressed)
            {
                switch (menu)
                {
                    case MENU_MODE.MENU:

                        if (GUI.Button(new Rect(0 + menuButtonOffset.x, 0 + menuButtonOffset.y, play_button_size, 40), "Start game", menuskins.GetStyle("Box")))
                        {
                            menuLoad = MENU_LOAD_MODE.START;
                            StartCoroutine(FadeIn());
                        }
                        if (SaveExist())
                        {
                            if (GUI.Button(new Rect(0 + menuButtonOffset.x, 50 + menuButtonOffset.y, load_button_size, 40), "Load game", menuskins.GetStyle("Box")))
                            {
                                menuLoad = MENU_LOAD_MODE.LOAD;
                                StartCoroutine(FadeIn());
                            }
                        }
                        if (GUI.Button(new Rect(0 + menuButtonOffset.x, 100 + menuButtonOffset.y, option_button_size, 40), "Options", menuskins.GetStyle("Box")))
                        {
                            menu = MENU_MODE.OPTIONS;
                        }

                        break;


                    case MENU_MODE.OPTIONS:
                        GUILayout.Label("Click on the buttons to change controls. Current Key to change: " + currentKeyName, menuskins.GetStyle("Box"));
                        foreach (KeyValuePair<string, KeyCode> key in s_globals.arrowKeyConfig)
                        {
                            if (GUILayout.Button(key + ": " + key.Value.ToString(), menuskins.GetStyle("Box")))
                            {
                                currentKeyName = key.Key;
                                PlayerPrefs.SetInt(key.Key, (int)key.Value);
                            }
                        }
                        DisplayConfigOptions();

                        if (currentKeyName != "none")
                        {

                            if (e.isKey || e.shift)
                            {
                                if (e.shift)
                                    s_globals.SetButtonKeyPref(currentKeyName, KeyCode.LeftShift);
                                else if (e.isKey)
                                    s_globals.SetButtonKeyPref(currentKeyName, e.keyCode);

                                currentKeyName = "none";
                            }
                        }
                        GUILayout.Label("Music Volume");
                        s_BGM.GetInstance().BGMvolume= GUILayout.HorizontalSlider(s_BGM.GetInstance().BGMvolume, 0, 1, menuskins.GetStyle("Box"), menuskins.GetStyle("Box"));
                        GUILayout.Label("Sound Volume");
                        s_soundmanager.GetInstance().soundVolume = GUILayout.HorizontalSlider(s_soundmanager.GetInstance().soundVolume, 0, 1, menuskins.GetStyle("Box"), menuskins.GetStyle("Box"));

                        if (GUILayout.Button("Back", menuskins.GetStyle("Box")))
                        {
                            currentKeyName = "none";
                            menu = MENU_MODE.MENU;
                        }
                        break;
                }
            }

        }
    }
}
