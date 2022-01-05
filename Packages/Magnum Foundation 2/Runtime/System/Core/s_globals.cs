using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using MagnumFoundation2.Objects;
using UnityEngine.SceneManagement;
using System;

namespace MagnumFoundation2.System.Core
{

    public class s_globals : s_singleton<s_globals>
    {
        public string currentlevelname;
        public string currentMapName;
        public string sceneToGoto = "Intro";    //After all the don't destory on loads are initialised
        public static string saveDataName = "save.mf2";

        public static s_globals globalSingle;
        public GUISkin skinGUI;

        bool CALL_STATIC_SCRIPT = false;
        bool COLLISION_SHOW = false;
        bool DEBUG_MODE_ON = false;
        enum EDITOR_MODE
        {
            FLAG,
            MAP_TRANS,
            NOCLIP,
            ENABLE_COLLISIONS,
            CALL_MAP_SCRIPT,
            SAVE
        }
        EDITOR_MODE EDIT;

        public char tpPointFlag;

        Vector2 scroll = new Vector2(0, 0);
        public string spawnObjectName;
        public string mapObject;
        Vector2 TPPos;
        public o_character player;
        public o_character playerPrefab;
        public static bool isPause = false;
        int map_menu_select = 0;

        public bool saveOnQuit;
        public bool isMainGame;
        public static bool allowPause = true;

        public bool isFixedSaveAreaInput = false;
        static bool isFixedSaveArea;

        public string fixedSaveAreaNameInput;
        static string fixedSaveAreaName;

        public bool EnableDebug;

        public static int Money;
        public List<ev_integer> GlobalFlagCache = new List<ev_integer>();
        public static Dictionary<string, int> GlobalFlags = new Dictionary<string, int>();

        public static Dictionary<string, KeyCode> arrowKeyConfig = new Dictionary<string, KeyCode>();
        public string[] allScenes;
        public List<triggerState> objectStates = new List<triggerState>();

        public void SetNewFlags() {
            foreach (ev_integer e in GlobalFlagCache)
            {
                if (!GlobalFlags.ContainsKey(e.integer_name))
                    GlobalFlags.Add(e.integer_name, e.integer);
            }
        }
        public void SetSavedFlags(dat_globalflags flgs)
        {
            foreach (KeyValuePair<string,int> e in flgs.Flags)
            {
                if (!GlobalFlags.ContainsKey(e.Key))
                    GlobalFlags.Add(e.Key, e.Value);
                else
                    GlobalFlags[e.Key] = e.Value;
            }
        }

        public void Awake()
        {
            if (globalSingle == null)
            {
                globalSingle = this;
                DontDestroyOnLoad(gameObject);
                //SceneManager.LoadScene(sceneToGoto);
            }
            else
            {
                Destroy(gameObject);
            }
            s_BGM.GetInstance().BGMvolume = PlayerPrefs.GetFloat("BGMvolume");
            if (isMainGame)
            {
                isFixedSaveArea = isFixedSaveAreaInput;
                fixedSaveAreaName = fixedSaveAreaNameInput;
                if (!s_mainmenu.isload)
                {
                    SetNewFlags();
                }
                if (!EnableDebug)
                {
                    if(player != null)
                        player.IS_KINEMATIC = true;
                    COLLISION_SHOW = false;
                }
            }
        }
        public triggerState GetTriggerState(s_object trig)
        {
            triggerState trState = objectStates.Find(x => (x.name == trig.name && x.scene == trig.gameObject.scene.name));
            return trState;
        }

        public void AddTriggerState(triggerState trig) {
            triggerState trState = objectStates.Find(x => (x.name == trig.name && x.scene == trig.scene));
            if (trState != null) {
                trState.switchA = trig.switchA;
                trState.switchB = trig.switchB;
                trState.switchC = trig.switchC;
                trState.switchD = trig.switchD;
            }
            else
            {
                objectStates.Add(trig);
                return;
            }
            objectStates.Add(trState);
        }

        public virtual void LoadSaveData() {

        }

        public virtual void StartStuff()
        {
            isFixedSaveArea = isFixedSaveAreaInput;
            fixedSaveAreaName = fixedSaveAreaNameInput;

            Vector2 loc = new Vector2();
            if (!s_mainmenu.isload)
                SetNewFlags();
            else {
                dat_save d = (dat_save)s_mainmenu.save;
               // print("this");
                if(d.trigStates != null)
                    objectStates.AddRange(d.trigStates.ToArray());
                SetSavedFlags(d.gbflg);
                loc = new Vector3(d.location.x, d.location.y);
            }
            
            if (!EnableDebug)
            {
                if(player != null)
                player.IS_KINEMATIC = true;
                COLLISION_SHOW = false;
            }
            StartCoroutine(s_triggerhandler.trigSingleton.Fade(Color.clear));

            if (player != null)
            {
                DontDestroyOnLoad(player.gameObject);
                player.gameObject.SetActive(false);
            }
            else
            {
                player = Instantiate(playerPrefab);
                player.name = playerPrefab.name;
                s_triggerhandler.trigSingleton.player = player;
                DontDestroyOnLoad(player.gameObject);
            }
            if (player != null)
                player.gameObject.SetActive(true);
            player.transform.position = new Vector3(loc.x, loc.y);
        }

        IEnumerator SceneLoadingEnumerator()
        {
            globalSingle.isMainGame = true;

            SetCamToTPPos();
            GameObject mapThing = GameObject.Find(mapObject);

            if (mapThing != null)
            {
                s_mapholder mapEvHolder = mapThing.GetComponent<s_mapholder>();
                s_camera.cam.mapSize = mapEvHolder.mapSize;
                s_camera.cam.tileSize = new Vector2(25, 25);
            }

            yield return new WaitForSeconds(Time.deltaTime);

            //doingEvents = false;
            if (mapThing != null)
            {
                if (player != null)
                    player.control = true;
            }
            else
                print("Failed to find " + mapObject + "!");
        }

        public void SetCamToTPPos() {

            GameObject telp = GameObject.Find(spawnObjectName);
            if (telp != null)
                TPPos = new Vector3(telp.transform.position.x, telp.transform.position.y, 0);
            if (telp != null)
                if (player != null)
                    player.transform.position = TPPos;
            s_camera.GetInstance().transform.position = new Vector3(TPPos.x, TPPos.y, s_camera.GetInstance().transform.position.z);
            GameObject mapThing = GameObject.Find(mapObject);
        }

        public void SceneLoad(Scene scene, LoadSceneMode sm)
        {
            player.gameObject.SetActive(true);
            StartCoroutine(SceneLoadingEnumerator());
        }

        public IEnumerator TriggerSpawnEnum(string string0, char point) {

            tpPointFlag = point;
            yield return SceneManager.LoadSceneAsync(string0);
        }

        public void TriggerSpawn(string string0, char point)
        {
            tpPointFlag = point;
            SceneManager.LoadScene(string0);
        }
        public virtual void TriggerSpawn(string teleporter)
        {
            o_character selobj = null;
            spawnObjectName = teleporter;
            selobj = GameObject.Find("Player").GetComponent<o_character>();
        }

        public static void SetButtonKeyPref(string buttonName, KeyCode keyCode)
        {
            PlayerPrefs.SetInt(buttonName, (int)keyCode);
        }
        public static void SetButtonKey(string buttonName, KeyCode keyCode)
        {
            if (!arrowKeyConfig.ContainsKey(buttonName))
            {
                arrowKeyConfig.Add(buttonName, keyCode);
                return;
            }
            arrowKeyConfig[buttonName] = keyCode;
        }

        public static void SetGlobalFlag(string flagname, int flag)
        {
            if (!GlobalFlags.ContainsKey(flagname))
            {
                GlobalFlags.Add(flagname, flag);
                return;
            }
            GlobalFlags[flagname] = flag;
        }

        public static int GetGlobalFlag(string flagname)
        {
            if (!GlobalFlags.ContainsKey(flagname))
            {
                return int.MinValue;
            }
            return GlobalFlags[flagname];
        }

        public static KeyCode GetKeyPref(string keyName)
        {
            if (PlayerPrefs.HasKey(keyName))
            {
                return (KeyCode)PlayerPrefs.GetInt(keyName);
            }
            return KeyCode.None;
        }
        public static KeyCode GetKey(string keyName)
        {
            if (arrowKeyConfig.ContainsKey(keyName))
            {
                return arrowKeyConfig[keyName];
            }
            return KeyCode.None;
        }

        public virtual void AddKeys()
        {
            SetButtonKeyPref("left", KeyCode.A);
            SetButtonKeyPref("right", KeyCode.D);
            SetButtonKeyPref("down", KeyCode.S);
            SetButtonKeyPref("up", KeyCode.W);
            SetButtonKeyPref("jump", KeyCode.Space);
            SetButtonKeyPref("dash", KeyCode.LeftShift);
            SetButtonKeyPref("select", KeyCode.E);
        }

        public void UnPauseAllObjects()
        {
            o_character[] ob = FindObjectsOfType<o_character>();
            foreach (var a in ob)
            {
                a.enabled = true;
                a.rbody2d.velocity = Vector2.zero;
            }
            player.enabled = true;
        }
        public void PauseAllObjects() {
            o_character[] ob = FindObjectsOfType<o_character>();
            foreach (var a in ob) {
                a.enabled = false;
                a.rbody2d.velocity = Vector2.zero;
            }
            player.enabled = false;
        }

        public void LoadFlag(dat_globalflags flag)
        {
            GlobalFlags = flag.Flags;
        }

        public virtual void SaveData()
        {
            FileStream fs = new FileStream(saveDataName, FileMode.Create);
            BinaryFormatter bin = new BinaryFormatter();

            dat_save sav = new dat_save();

            if (isFixedSaveArea)
                sav.currentmap = fixedSaveAreaName;

            bin.Serialize(fs, sav);
            fs.Close();
        }


        public virtual void Pause()
        {
            PauseAllObjects();
            Time.timeScale = 0;
        }
        public virtual void Resume()
        {
            UnPauseAllObjects();
            Time.timeScale = 1;
        }

        public void Update()
        {
            if (allowPause)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (!isPause)
                    {
                        isPause = true;
                        Pause();
                    }
                }
            }
        }

        IEnumerator QuitToTitle()
        {
            Time.timeScale = 1;
            PauseAllObjects();
            isPause = false;
            yield return StartCoroutine(s_triggerhandler.GetInstance().Fade(Color.black));
            s_camera.GetInstance().transform.position = new Vector3(0,0, s_camera.GetInstance().transform.position.z);
            ClearAllThings();
            SceneManager.LoadScene(0);
            yield return StartCoroutine(s_triggerhandler.GetInstance().Fade(Color.clear));
        }

        public void OnGUI()
        {
            if (isPause && isMainGame)
            {
                if (saveOnQuit)
                {
                    if (GUILayout.Button("Save and quit", skinGUI.GetStyle("Box")))
                    {
                        PlayerPrefs.SetFloat("BGMvolume", s_BGM.GetInstance().BGMvolume);
                        SaveData();
                        ClearAllThings();
                        SceneManager.LoadScene(0);
                    }
                } else {
                    if (GUILayout.Button("Quit game", skinGUI.GetStyle("Box")))
                    {
                        PlayerPrefs.SetFloat("BGMvolume", s_BGM.GetInstance().BGMvolume);
                        StartCoroutine(QuitToTitle());
                    }
                }
                if (GUILayout.Button("Resume", skinGUI.GetStyle("Box")))
                {
                    PlayerPrefs.SetFloat("BGMvolume", s_BGM.GetInstance().BGMvolume);
                    isPause = false;
                    Resume();
                }
                GUILayout.Label("Music Volume", skinGUI.GetStyle("Box"));
                s_BGM.GetInstance().BGMvolume = GUILayout.HorizontalSlider(s_BGM.GetInstance().BGMvolume, 0, 1, skinGUI.GetStyle("Box"), skinGUI.GetStyle("Box"));
                GUILayout.Label("Sound Volume", skinGUI.GetStyle("Box"));
                s_soundmanager.GetInstance().soundVolume = GUILayout.HorizontalSlider(s_soundmanager.GetInstance().soundVolume, 0, 1, skinGUI.GetStyle("Box"), skinGUI.GetStyle("Box"));
            }
            if (isMainGame)
            {
                if (EnableDebug)
                {
                    int ind = 2;
                    foreach (KeyValuePair<string, int> flag in GlobalFlags)
                    {
                        GUI.Label(new Rect(0, 50 * ind, 90, 40), flag.Key + " Value: " + flag.Value, GUIStyle.none);
                        if (GUI.Button(new Rect(100, 50 * ind, 90, 40), "+"))
                        {
                            SetGlobalFlag(flag.Key, flag.Value + 1);
                        }
                        if (GUI.Button(new Rect(200, 50 * ind, 90, 40), "-"))
                        {
                            SetGlobalFlag(flag.Key, flag.Value - 1);
                        }
                        ind++;
                    }
                    
                }
                else
                {
                    if(player != null)
                    player.IS_KINEMATIC = true;
                }
            }
        }
        public virtual void ClearAllThings() {
            Destroy(player.gameObject);
            GlobalFlags.Clear();
            objectStates.Clear();
            currentMapName = "";
            tpPointFlag = ' ';
        }
    }
}
/*
                    if (DEBUG_MODE_ON)
                    {
                        if (GUI.Button(new Rect(0, 0, 120, 40), "Debug Mode Off"))
                        {
                            DEBUG_MODE_ON = false;
                        }
                        if (GUI.Button(new Rect(0, 60, 120, 40), EDIT.ToString()))
                        {
                            EDIT++;
                            EDIT = (EDITOR_MODE)(int)Mathf.Clamp((float)EDIT, 0, 6);
                            if ((int)EDIT == 6)
                            {
                                EDIT = EDITOR_MODE.FLAG;
                            }
                        }
                        int ind = 2;
                        int x = 0, y = 0;
                        switch (EDIT)
                        {
                            case EDITOR_MODE.FLAG:

                                foreach (KeyValuePair<string, int> flag in GlobalFlags)
                                {
                                    GUI.Label(new Rect(0, 50 * ind, 90, 40), flag.Key + " Value: " + flag.Value, GUIStyle.none);
                                    if (GUI.Button(new Rect(100, 50 * ind, 90, 40), "+"))
                                    {
                                        SetGlobalFlag(flag.Key, flag.Value + 1);
                                    }
                                    if (GUI.Button(new Rect(200, 50 * ind, 90, 40), "-"))
                                    {
                                        SetGlobalFlag(flag.Key, flag.Value - 1);
                                    }
                                    ind++;
                                }
                                break;

                            case EDITOR_MODE.MAP_TRANS:

                                if (allScenes.Length > 9)
                                {
                                    if (GUI.Button(new Rect(355, 50 * 1, 50, 50), "<"))
                                    {
                                        map_menu_select--;
                                    }
                                    if (GUI.Button(new Rect(405, 50 * 1, 50, 50), ">"))
                                    {
                                        map_menu_select++;
                                    }
                                    float lengA = 9;
                                    float lengB = allScenes.Length;

                                    map_menu_select = Mathf.Clamp(map_menu_select, 0, (int)(lengB / lengA));
                                }

                                for (int i = 0; i < 9; i++)
                                {
                                    if (i + (9 * map_menu_select) > allScenes.Length - 1)
                                    {
                                        break;
                                    }
                                    string ma = allScenes[i + (9 * map_menu_select)];
                                    if (GUI.Button(new Rect(160, 50 * (y + 2), 160, 50), ma))
                                    {
                                        //TriggerSpawn(ma, "");
                                    }
                                    y++;
                                }

                                break;

                            case EDITOR_MODE.NOCLIP:
                                if (player != null)
                                {
                                    if (player.IS_KINEMATIC)
                                    {
                                        if (GUI.Button(new Rect(0, 100, 160, 50), "Disable"))
                                        {
                                            player.IS_KINEMATIC = false;
                                        }
                                    }
                                    else
                                    {
                                        if (GUI.Button(new Rect(0, 100, 160, 50), "Enable"))
                                        {
                                            player.IS_KINEMATIC = true;
                                        }
                                    }
                                }
                                break;
                                

                            case EDITOR_MODE.SAVE:
                                if (GUI.Button(new Rect(200, 50 * ind, 90, 40), "Save data"))
                                {
                                    SaveData();
                                }
                                break;
                        }

                    }
                    else
                    {
                        if (GUI.Button(new Rect(0, 0, 120, 40), "Debug Mode"))
                        {
                            DEBUG_MODE_ON = true;
                        }
                    }
                    */