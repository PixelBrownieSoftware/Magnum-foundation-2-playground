using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Playables;

using MagnumFoundation2.Objects;
using MagnumFoundation2.System.Core;

using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace MagnumFoundation2.System
{
    [Serializable]
    public struct s_dialogue_choice
    {
        public s_dialogue_choice(string option, int flagTojump)
        {
            this.option = option;
            this.flagTojump = flagTojump;
        }

        public string option;
        public int flagTojump;
    }
    [Serializable]
    public struct customEv
    {
        public delegate void cutsceneFunction();
        public cutsceneFunction func;
        public string name;
        public bool hasString0;
        public bool hasString1;
        public bool hasInt0;
        public bool hasInt1;
        public bool hasInt2;
    }

    [Serializable]
    public class name_colour {
        public string name;
        public Color colour;
    }

    public class s_triggerhandler : s_singleton<s_triggerhandler>
    {
        public List<s_dialogue_choice> dialogueChoices = new List<s_dialogue_choice>();
        public float travSpeed = 22.5f;
        public static s_triggerhandler trigSingleton;
        public bool isSkipping = false;
        public bool doingEvents = false;
        public Image fade;
        public GameObject shutterObj;
        public Text Dialogue;
        const float shutterdepth = 1.55f;
        public List<ev_details> StaticEvents;
        public o_character player;
        public ev_script Events;
        bool doStatic = false;
        public o_character[] characters;
        public s_object selobj;
        public int pointer;
        int textNum = 0;
        public Image textBox;
        public Text bigTxt;

        public List<s_object> characterReferences = new List<s_object>();

        public Color defaultNameColour = Color.white;
        public List<name_colour> nameColours;

        public Image speakerTextbox;
        public Text speakerText;
        public Text continueTxt;

        bool _callFlag = false;

        public string keyWord;  //Stuff used in things like items

        bool activated_shutters = false;
        public List<customEv> customEvAndFunction;
        public s_object scriptCaller;
        public delegate void eventEnd();
        public eventEnd evEnd;

        bool _selectPressed = false;

        public virtual void CreateData()
        {
            customEvAndFunction = new List<customEv>();
        }

        public void Awake()
        {
            if (trigSingleton == null)
            {
                trigSingleton = this;
                if(transform.parent == null)
                    DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Reserve event number -1 for label
        /// </summary>
        /// <param name="label"></param>
        public void JumpToEvent(ev_script scr)
        {
            if (!doingEvents)
            {
                s_globals.allowPause = false;
                pointer = 0;
                Events = scr;
                doingEvents = true;
                StartCoroutine(EventPlayMast());
            }
        }
        
        public void JumpToEvent(ev_script scr, string kwd)
        {
            keyWord = kwd;
            JumpToEvent(scr);
        }

        public IEnumerator EventPlayMast()
        {
            characterReferences = new List<s_object>();
            if (Events.objectReferences != null) {
                if (Events.objectReferences.Count > 0)
                {
                    foreach (string a in Events.objectReferences)
                    {
                        if (GameObject.Find(a) != null)
                        {
                            GameObject obj = GameObject.Find(a);
                            if (obj != null)
                            {
                                s_object obClass = obj.GetComponent<s_object>();
                                if (obClass != null)
                                {
                                    characterReferences.Add(obClass);
                                }
                            }
                        }
                    }
                }
            }
            while (doingEvents)
            {
                if (pointer == Events.elements.Count)
                {
                    doingEvents = false;
                    break;
                }

                if (Events.elements[pointer].simultaenous)
                    StartCoroutine(EventPlay(Events.elements[pointer]));
                else
                    yield return StartCoroutine(EventPlay(Events.elements[pointer]));

                if (pointer == -1)
                    break;
                else if(_callFlag)
                {
                    _callFlag = false;
                    pointer = -1;
                    //print("RESET");
                }
                pointer++;
                //print("Pointer at: " + pointer);
            }
            pointer = 0;
            speakerTextbox.color = Color.clear;
            speakerText.text = "";
            isSkipping = false;
            Time.timeScale = 1;
            doingEvents = false;
            activated_shutters = false;
            if (evEnd != null)
                evEnd.Invoke();
            s_globals.allowPause = true;
        }
        public IEnumerator FadeChar(SpriteRenderer rend ,Color col)
        {
            float t = 0;
            while (rend.color != col)
            {
                t += Time.deltaTime;
                rend.color = Color.Lerp(rend.color, col, t);
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }

        public IEnumerator Fade(Color col, float time)
        {
            float t = 0;
            while (fade.color != col)
            {
                t += Time.unscaledDeltaTime * time;
                fade.color = Color.Lerp(fade.color, col, t);
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }
        }
        public IEnumerator Fade(Color col) {
            float t = 0;
            while (fade.color != col)
            {
                t += Time.unscaledDeltaTime;
                fade.color = Color.Lerp(fade.color, col, t);
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
            }
        }

        public IEnumerator MoveCharacter(o_character c, Vector2 newpos) {
            
            float dist = Vector2.Distance(c.transform.position, newpos);
            Vector2 dir = (newpos - new Vector2(c.transform.position.x, c.transform.position.y)).normalized;

            while (Vector2.Distance(c.transform.position, newpos) > 2.5f)
            {
                dir = (newpos - new Vector2(c.transform.position.x, c.transform.position.y)).normalized;
                if (c != null)
                {
                    //Set charaacter's anims whilst moving
                    c.direction = dir;
                    c.CHARACTER_STATE = o_character.CHARACTER_STATES.STATE_MOVING;
                    c.AnimMove();
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            c.CHARACTER_STATE = o_character.CHARACTER_STATES.STATE_IDLE;
            c.AnimMove();
        }

        /// <summary>
        /// Plays all the general events, if you want to add more events then override this function
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerator EventPlay(ev_details current_ev)
        {
            GameObject go = null;
            yield return new WaitForSecondsRealtime(current_ev.simlutaneousDelay);
            int labelNum = 0;
            switch (current_ev.eventType)
            {
                case EVENT_TYPES.BREAK_EVENT:
                    doingEvents = false;
                    break;

                #region PLAY SOUND
                case EVENT_TYPES.SOUND:
                    s_soundmanager.GetInstance().PlaySound(current_ev.sound);
                    break;
                #endregion

                #region CHANGE MUSIC
                case EVENT_TYPES.CHANGE_MUSIC:
                    if (current_ev.boolean)
                    {
                        yield return StartCoroutine(s_BGM.GetInstance().FadeOutMusic(1.35f));
                    }
                    else {
                        yield return StartCoroutine(s_BGM.GetInstance().FadeInMusic(current_ev.sound));
                    }
                    break;
                #endregion

                #region ANIMATE CHARACTER
                case EVENT_TYPES.ANIMATION:

                    if (current_ev.string1 != "o_player")
                    {
                        go = GameObject.Find(current_ev.string1);
                        if (go != null)
                        {
                            Animator anim = go.GetComponent<Animator>();
                            anim.Play(current_ev.string0);
                        }

                    }
                    else
                        selobj = player;
                    break;
                #endregion

                #region OBJECT DIRECTION
                case EVENT_TYPES.OBJECT_DIRECTION:
                    {
                        GameObject gameObj = null;
                        o_character charaMove = null;
                        //print(characterReferences[current_ev.int0].name);
                        gameObj = characterReferences[current_ev.int0].gameObject;
                        charaMove = gameObj.GetComponent<o_character>();
                        charaMove.direction = new Vector2(current_ev.float0, current_ev.float1);
                        charaMove.AnimMove();
                    }
                    break;
                #endregion

                #region MOVE CHARACTER
                case EVENT_TYPES.MOVEMNET:
                    {
                        float timer = 1.02f;
                        s_object charaMove = null;
                        o_generic group = null;
                        Vector2 newpos = current_ev.pos;
                        GameObject gameObj = null;

                        //USES NEW SYSTEM?
                        if (current_ev.boolean2)
                        {
                            gameObj = characterReferences[current_ev.int0].gameObject;
                        }
                        else
                        {
                            gameObj = GameObject.Find(current_ev.string0);
                        }
                        if (gameObj != null)
                        {
                            if (current_ev.string0 != "o_player")
                            {
                                group = gameObj.GetComponent<o_generic>();
                                if (group != null)
                                {
                                    newpos = current_ev.pos;
                                    group.transform.position = new Vector3((int)newpos.x, (int)newpos.y, 0);
                                    break;
                                }
                            }

                            charaMove = gameObj.GetComponent<o_character>();

                            if (current_ev.boolean)
                            {
                                gameObj.transform.position = newpos;
                                break;
                            }
                            o_character c = charaMove.GetComponent<o_character>();
                            if (c != null)
                            {
                                yield return StartCoroutine(MoveCharacter(c, newpos));
                            }
                        }
                    }
                    break;
                #endregion

                #region DIALOUGE
                case EVENT_TYPES.DIALOGUE:
                    textBox.gameObject.SetActive(true);
                    string[] str = current_ev.string0.Split(new string[1] { " " }, StringSplitOptions.None);
                    string dia = "";

                    for (int i = 0; i < str.Length; i++) {
                        if (str[i] == "<kwd>") {
                            str[i] = " " + keyWord;
                            //continue;
                        }
                        dia += " " + str[i];
                    }

                    speakerTextbox.color = Color.clear;
                    if (current_ev.string1 != "")
                    {
                        speakerText.color = defaultNameColour;
                        if (nameColours.Find(x => x.name == current_ev.string1) != null) {

                            speakerText.color = nameColours.Find(x => x.name == current_ev.string1).colour;
                        }
                        speakerText.text = current_ev.string1;
                    }
                    else
                    {
                        speakerText.text = "";
                    }

                    while (Dialogue.text.Length < dia.Length)
                    {
                        if (isSkipping)
                            break;
                        Dialogue.text += dia[textNum];
                        if (_selectPressed)
                        {
                            Dialogue.text = dia;
                            continue;
                        }
                        textNum++;
                        yield return new WaitForSeconds(Time.deltaTime);
                    }
                    if (!isSkipping)
                    {
                        continueTxt.enabled = true;
                        continueTxt.text = "Press " + s_globals.GetKeyPref("select").ToString() + " to continue.\nPress X to skip";

                        float a = 0;
                        float t = 0;
                        while (!_selectPressed)
                        {
                            if (isSkipping)
                            {
                                break;
                            }
                            t = Mathf.Sin(a);
                            a += Time.deltaTime * 2.5f;
                            continueTxt.color = Color.Lerp(Color.white, new Color(1,1,1,0.25f), t);
                            yield return new WaitForSeconds(Time.deltaTime);
                        }
                        continueTxt.color = Color.clear;
                        continueTxt.enabled = false;
                    }
                    Dialogue.text = "";
                    textNum = 0;
                    if (pointer + 1 < Events.elements.Count)
                    {
                        switch (Events.elements[pointer + 1].eventType) {
                            case EVENT_TYPES.DIALOGUE:
                            case EVENT_TYPES.PRESENT_CHOICES:
                                break;

                            default:
                                speakerTextbox.color = Color.clear;
                                speakerText.text = "";
                                textBox.gameObject.SetActive(false);
                                break;
                        }
                    }
                    break;
                #endregion

                #region RUN CHARACTER SCRIPT
                case EVENT_TYPES.RUN_CHARACTER_SCRIPT:
                    GameObject obj = GameObject.Find(current_ev.string0);
                    if (obj != null)
                    {
                        o_character ch = obj.GetComponent<o_character>();
                        if (ch != null)
                        {
                            if (ch.rbody2d != null)
                                ch.rbody2d.velocity = Vector2.zero;
                            ch.dashdelay = 0;
                            ch.dashdelayStart = 0;
                            ch.CHARACTER_STATE = o_character.CHARACTER_STATES.STATE_IDLE;
                            ch.AnimMove();
                            ch.control = current_ev.boolean;
                        }
                    }
                    break;
                #endregion

                #region CALL SCRIPT
                case EVENT_TYPES.CALL_SCRIPT:
                    Events = current_ev.scrObj as ev_script;
                    _callFlag = true;
                    pointer = 0;
                    print(Events.elements.Count);
                    break;
                #endregion

                #region SET OBJECT SWITCH
                case EVENT_TYPES.SET_SWITCH:
                    {
                        GameObject gameObj = null;
                        gameObj = GameObject.Find(current_ev.string0);
                        if (gameObj != null)
                        {
                            selobj = gameObj.GetComponent<s_object>();
                            switch (current_ev.string1)
                            {
                                case "a":
                                    selobj.switchA = current_ev.boolean;
                                    break;
                                case "b":
                                    selobj.switchB = current_ev.boolean;
                                    break;
                                case "c":
                                    selobj.switchC = current_ev.boolean;
                                    break;
                                case "d":
                                    selobj.switchD = current_ev.boolean;
                                    break;
                            }
                            triggerState tr = new triggerState(gameObj.name, gameObj.scene.name, selobj.switchA);
                            tr.switchB = selobj.switchB;
                            tr.switchC = selobj.switchC;
                            tr.switchD = selobj.switchD;

                            s_globals.globalSingle.AddTriggerState(tr);
                        }
                    }
                    break;
                #endregion

                #region CAMERA MOVEMENT
                case EVENT_TYPES.CAMERA_MOVEMENT:

                    GameObject ca = GameObject.Find("Main Camera");
                    ca.GetComponent<s_camera>().ResetSpeedProg();
                    Vector2 pos = new Vector2(0, 0);

                    float spe = current_ev.float0; //SPEED

                    s_object obje = null;

                    switch (current_ev.camMode) {
                        case s_camera.CAMERA_MODE.LERPING:
                            pos = new Vector2(current_ev.pos.x, current_ev.pos.y);
                            yield return StartCoroutine(s_camera.GetInstance().MoveCamera(pos, current_ev.float0));
                            break;

                        case s_camera.CAMERA_MODE.ZOOM:
                            pos = new Vector2(current_ev.pos.x, current_ev.pos.y);
                            s_camera.GetInstance().InstantZoomCamera(current_ev.float0);
                            break;

                        case s_camera.CAMERA_MODE.LERP_ZOOM:
                            pos = new Vector2(current_ev.pos.x, current_ev.pos.y);
                            yield return StartCoroutine(s_camera.GetInstance().ZoomCamera(current_ev.float1, pos, current_ev.float0));
                            break;

                        case s_camera.CAMERA_MODE.CHARACTER_FOCUS_ZOOM:
                            {
                                GameObject gameObj = null;
                                gameObj = characterReferences[current_ev.int0].gameObject;
                                pos = gameObj.transform.position;
                                yield return StartCoroutine(s_camera.GetInstance().ZoomCamera(current_ev.float1, pos, current_ev.float0));
                                s_camera.cam.SetPlayerFocus(gameObj);
                            }
                            break;

                        case s_camera.CAMERA_MODE.CHARACTER_FOCUS:
                            {
                                GameObject gameObj = null;
                                //USES NEW SYSTEM?
                                if (current_ev.boolean2)
                                {
                                    gameObj = characterReferences[current_ev.int0].gameObject;
                                }
                                else
                                {
                                    gameObj = GameObject.Find(current_ev.string0);
                                }
                                s_camera.cam.SetPlayerFocus(gameObj);
                                pos = gameObj.transform.position;
                            }
                            break;

                        case s_camera.CAMERA_MODE.NONE:
                            pos = new Vector2(current_ev.pos.x, current_ev.pos.y);
                            s_camera.GetInstance().TeleportCamera(pos);
                            break;
                    }

                    /*
                    if (s_camera.cam.cameraMode != s_camera.CAMERA_MODE.NONE
                        && s_camera.cam.cameraMode != s_camera.CAMERA_MODE.NONE) {
                        float dista = Vector2.Distance(ca.transform.position, new Vector3(pos.x, pos.y, -5));
                        while (Vector2.Distance(ca.transform.position, new Vector3(pos.x, pos.y, -5))
                            > dista * 0.05f)
                        {
                            yield return new WaitForSeconds(Time.deltaTime);
                        }
                    }
                    */
                    break;
                #endregion

                #region CHECK FLAG
                case EVENT_TYPES.CHECK_FLAG:
                    int integr = s_globals.GetGlobalFlag(current_ev.string0);

                    labelNum = FindLabel(current_ev.string1);

                    if (labelNum == int.MinValue)
                        labelNum = current_ev.int1 - 1;

                    switch (current_ev.logicType)
                    {
                        case ev_details.LOGIC_TYPE.VAR_EQUAL:
                            if (integr == current_ev.int0)  //Check if it is equal to the value
                            {
                                pointer = labelNum;   //Label to jump to
                            }
                            break;

                        case ev_details.LOGIC_TYPE.VAR_NOT_EQUAL:
                            if (integr != current_ev.int0)  //Check if it is not equal to the value
                            {
                                pointer = labelNum;   //Label to jump to
                            }
                            else
                            {
                                if (current_ev.boolean)     //Does this have an "else if"?
                                {
                                    pointer = current_ev.int2 - 1;      //Other Label to jump to
                                }
                            }
                            break;

                        case ev_details.LOGIC_TYPE.VAR_GREATER:
                            if (integr > current_ev.int0)  //Check if it is greater
                            {
                                pointer = labelNum;   //Label to jump to
                            }
                            break;


                        case ev_details.LOGIC_TYPE.VAR_LESS:
                            if (integr < current_ev.int0)  //Check if it is less
                            {
                                pointer = labelNum;   //Label to jump to
                            }
                            else
                            {
                                if (current_ev.boolean)     //Does this have an "else if"?
                                {
                                    pointer = current_ev.int2 - 1;      //Other Label to jump to
                                }
                            }
                            break;

                            /*
                        case LOGIC_TYPE.CHECK_UTILITY_RETURN_NUM:

                            //This checks utilities after the INITIALIZE function
                            if (GetComponent<s_utility>().eventState == current_ev.int0)
                            {
                                pointer = current_ev.int1 - 1;   //Label to jump to
                            }
                            else
                            {
                                pointer = current_ev.int2 - 1;
                            }
                            break;
                            */
                    }
                    if (integr == current_ev.int0)
                    {

                    }
                    break;
                #endregion

                #region SHUTTERS
                case EVENT_TYPES.PUT_SHUTTERS:
                    Image sh1 = shutterObj.transform.GetChild(0).GetComponent<Image>();
                    Image sh2 = shutterObj.transform.GetChild(1).GetComponent<Image>();
                    if (activated_shutters)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            sh1.rectTransform.position += new Vector3(0, shutterdepth);
                            sh2.rectTransform.position += new Vector3(0, -shutterdepth);
                            yield return new WaitForSeconds(Time.deltaTime);
                        }
                        activated_shutters = false;
                    }
                    else
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            sh1.rectTransform.position += new Vector3(0, -shutterdepth);
                            sh2.rectTransform.position += new Vector3(0, shutterdepth);
                            yield return new WaitForSeconds(Time.deltaTime);
                        }
                        activated_shutters = true;
                    }
                    break;
                #endregion

                #region FADE
                case EVENT_TYPES.FADE:
                    if (current_ev.boolean)
                    {
                        fade.color = current_ev.colour;
                    }
                    else {
                        yield return StartCoroutine(Fade(current_ev.colour));
                    }
                    break;
                #endregion

                #region WAIT
                case EVENT_TYPES.WAIT:

                    yield return new WaitForSeconds(current_ev.float0);
                    break;
                #endregion

                #region SET FLAG
                case EVENT_TYPES.SET_FLAG:
                    s_globals.SetGlobalFlag(current_ev.string0, current_ev.int0);
                    break;
                #endregion

                #region CHANGE MAP
                case EVENT_TYPES.CHANGE_MAP:

                    if (!current_ev.boolean)
                    {
                        StartCoroutine(s_BGM.GetInstance().FadeOutMusic(2.3f));
                    }
                    if (s_globals.GetInstance().player != null)
                    {
                        //s_globals.GetInstance().player.direction = new Vector2(0, 0);
                        s_globals.GetInstance().player.rbody2d.velocity = Vector2.zero;
                        s_globals.GetInstance().player.control = false;
                        s_globals.GetInstance().player.CHARACTER_STATE = o_character.CHARACTER_STATES.STATE_IDLE;
                        s_globals.GetInstance().player.AnimMove();
                        s_globals.GetInstance().player.dashdelay = 0;
                    }
                    Time.timeScale = 0;

                    //Continue playing music if this is true

                    float t2 = 0;
                    while (fade.color != Color.black)
                    {
                        t2 += Time.unscaledDeltaTime;
                        fade.color = Color.Lerp(Color.clear, Color.black, t2);
                        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
                    }
                    s_camera.GetInstance().transform.position = new Vector3(current_ev.float0, current_ev.float1, s_camera.GetInstance().transform.position.z);
                    pointer = -1;
                    Events = null;

                    s_globals.GetInstance().StartCoroutine(s_globals.GetInstance().TriggerSpawnEnum(current_ev.string0, current_ev.tpPoint));
                    //s_globals.GetInstance().SetCamToTPPos();

                    t2 = 0;
                    while (fade.color != Color.clear)
                    {
                        //print(Time.timeScale);
                        t2 += Time.unscaledDeltaTime;
                        fade.color = Color.Lerp(Color.black, Color.clear, t2);
                        yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
                    }
                    Time.timeScale = 1;
                    if (!current_ev.boolean)
                    {
                        s_globals.GetInstance().player.CHARACTER_STATE = o_character.CHARACTER_STATES.STATE_IDLE;
                        s_globals.GetInstance().player.AnimMove();
                        s_globals.GetInstance().player.control = true;
                    }
                    break;
                #endregion

                #region CHANGE SCENE
                case EVENT_TYPES.CHANGE_SCENE:
                    UnityEngine.SceneManagement.SceneManager.LoadScene(current_ev.string0);
                    break;
                #endregion

                #region SET UTILITY FLAG
                case EVENT_TYPES.SET_UTILITY_FLAG:
                    /*
                    GameObject utGO = GameObject.Find(current_ev.string0);
                    if (utGO)
                    {
                        s_utility utility = utGO.GetComponent<s_utility>();

                        if (utility != null)
                            utility.eventState = current_ev.int0;
                        else
                            break;
                    }
                    */
                    break;
                #endregion

                #region CHECK UTILITY FLAG
                //Make this like the conditional statements where it checks if the utility is in a certain state
                case EVENT_TYPES.UTILITY_CHECK:
                    /*
                    labelNum = FindLabel(current_ev.string1);
                    GameObject utGO2 = GameObject.Find(current_ev.string0);
                    if (utGO2)
                    {
                        s_utility ut = utGO2.GetComponent<s_utility>();
                        if (ut != null)
                        {
                            if (ut.eventState == current_ev.int0)
                            {
                                pointer = labelNum;
                            }
                        }
                    }
                    */
                    break;
                #endregion

                #region ADD CHOICE
                case EVENT_TYPES.ADD_CHOICE_OPTION:

                    s_dialogue_choice dialo = new s_dialogue_choice(current_ev.string0, FindLabel(current_ev.string1));

                    if (dialogueChoices != null)
                        dialogueChoices.Add(dialo);
                    else
                    {
                        dialogueChoices = new List<s_dialogue_choice>();
                        dialogueChoices.Add(dialo);
                    }
                    break;
                #endregion

                #region CLEAR CHOICES
                case EVENT_TYPES.CLEAR_CHOICES:
                    dialogueChoices.Clear();
                    break;
                #endregion

                #region PRESENT CHOICES
                case EVENT_TYPES.PRESENT_CHOICES:

                    textBox.gameObject.SetActive(true);
                    int choice = 0, finalchoice = -1;
                    //print(choice);

                    while (finalchoice == -1)
                    {
                        if (Input.GetKeyDown(s_globals.GetKeyPref("up")))
                            choice--;

                        if (Input.GetKeyDown(s_globals.GetKeyPref("down")))
                            choice++;

                        choice = Mathf.Clamp(choice, 0, dialogueChoices.Count - 1);

                        if (Input.GetKeyDown(s_globals.GetKeyPref("select")))
                        {
                            //print("Chosen");
                            finalchoice = choice;
                        }
                        Dialogue.text = "Arrow keys to scroll, Z to select" + "\n";
                        //Dialogue.text += current_ev.string0 + "\n";
                        for (int i = 0; i < dialogueChoices.Count; i++)
                        {
                            if (choice == i)
                                Dialogue.text += "-> ";

                            Dialogue.text += dialogueChoices[i].option + "\n";
                        }
                        //print(choice);
                        yield return new WaitForSeconds(Time.deltaTime);
                    }
                    Dialogue.text = "";
                    pointer = dialogueChoices[finalchoice].flagTojump - 1;
                    if (pointer + 1 < Events.elements.Count)
                    {
                        if (Events.elements[pointer + 1].eventType != EVENT_TYPES.DIALOGUE &&
                            Events.elements[pointer + 1].eventType != EVENT_TYPES.PRESENT_CHOICES)
                            textBox.gameObject.SetActive(false);
                    }
                    break;
                #endregion

                #region ENABLE/DISABLE OBJECT
                case EVENT_TYPES.ENABLE_DISABLE_OBJECT:
                    if (current_ev.boolean1)
                    {
                        go = characterReferences[current_ev.int0].gameObject;
                    }
                    else
                    {
                        go = GameObject.Find(current_ev.string0);
                    }
                    if (current_ev.boolean)
                    {
                        go.gameObject.SetActive(true);
                    }
                    else
                    {
                        go.gameObject.SetActive(false);
                    }
                    break;
                #endregion

                case EVENT_TYPES.DELETE_OBJECT:
                    go = GameObject.Find(current_ev.string0);
                    Destroy(go.gameObject);
                    break;

                case EVENT_TYPES.MAIN_MENU:
                    s_globals.GetInstance().ClearAllThings();
                    Destroy(s_globals.GetInstance().player.gameObject);
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
                    break;

                #region FADE_SPRITE
                case EVENT_TYPES.FADE_SPRITE:
                    if (current_ev.boolean1)
                    {
                        go = characterReferences[current_ev.int0].gameObject;
                    }
                    else
                    {
                        go = GameObject.Find(current_ev.string0);
                    }
                    SpriteRenderer rend = null;

                    if (go.GetComponent<s_object>()) {
                        rend = go.GetComponent<s_object>().rendererObj;
                    } else {
                        rend = go.GetComponent<SpriteRenderer>();
                    }

                    if (current_ev.boolean)
                    {
                        rend.color = current_ev.colour;
                    }
                    else
                    {
                        yield return StartCoroutine(FadeChar(rend, current_ev.colour));
                    }
                    break;
                #endregion

                #region JUMP TO LABEL
                case EVENT_TYPES.JUMP_TO_LABEL:
                    labelNum = FindLabel(current_ev.string0);
                    pointer = labelNum;
                    break;
                #endregion

                #region SAVE DATA
                case EVENT_TYPES.SAVE_DATA:
                    s_globals.globalSingle.SaveData();
                    break;
                #endregion

                #region BIG_TEXT
                case EVENT_TYPES.BIG_TEXT:

                    if (isSkipping)
                    {
                        bigTxt.color = Color.clear;
                        yield return new WaitForSeconds(Time.deltaTime);
                    }
                    else
                    {
                        bigTxt.enabled = true;
                        continueTxt.enabled = true;
                        bigTxt.text = current_ev.string0;
                        //print(current_ev.string0);
                        bigTxt.color = Color.clear;
                        float a = 0;
                        while (bigTxt.color != Color.white)
                        {
                            bigTxt.color = Color.Lerp(bigTxt.color, Color.white, a);
                            a += Time.deltaTime;
                            yield return new WaitForSeconds(Time.deltaTime * 1.75f);
                        }
                        a = 0;
                        float t = 0;
                        continueTxt.text = "Press " + s_globals.GetKeyPref("select").ToString() + " to continue.\nPress X to skip";
                        while (!Input.GetKeyDown(s_globals.GetKeyPref("select")))
                        {
                            if (isSkipping) {
                                break;
                            }
                            t = Mathf.Sin(a);
                            a += Time.deltaTime * 2.5f;
                            continueTxt.color = Color.Lerp(Color.white, Color.clear, t);
                            yield return new WaitForSeconds(Time.deltaTime);
                        }
                        continueTxt.color = Color.clear;
                        a = 0;
                        while (bigTxt.color != Color.clear)
                        {
                            bigTxt.color = Color.Lerp(bigTxt.color, Color.clear, a);
                            a += Time.deltaTime;
                            yield return new WaitForSeconds(Time.deltaTime);
                        }
                        continueTxt.enabled = false;
                        bigTxt.enabled = false;
                    }
                    break;
                    #endregion

            }
            yield return new WaitForSeconds(Time.deltaTime);
            //yield return new WaitForSeconds(0.5f);
        }
        public void Update()
        {
            if (doingEvents)
            {
                if (Input.GetKeyDown(KeyCode.X))
                {
                    isSkipping = true;
                    Time.timeScale = 2f;
                }
                _selectPressed = Input.GetKeyDown(s_globals.GetKeyPref("select"));
            }

        }
        public int FindLabel(string labelName)
        {
            for (int i = 0; i < Events.elements.Count; i++)
            {
                if (Events.elements[i].eventType != EVENT_TYPES.LABEL)
                    continue;
                if (Events.elements[i].string0 == labelName)
                    return i;
            }
            return int.MinValue;
        }
    }
}
