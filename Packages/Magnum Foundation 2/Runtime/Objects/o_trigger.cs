using MagnumFoundation2.System.Core;
using MagnumFoundation2.System;
using MagnumFoundation2.Objects;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace MagnumFoundation2.Objects
{
    [Serializable]
    public class trig_cond
    {
        public string condFlgName;
        public int condFlgNum;
        public ev_details.LOGIC_TYPE logicType;
        public bool isDestroy = false;
        public ev_script script;
        public GameObject[] linkedObjects;
        public bool checkFlag = true;
        public bool checkSwitches = false;
        public bool checkA;
        public bool checkB;
        public bool checkC;
        public bool checkD;
        public bool setTriggerType = false;
        public o_trigger.TRIGGER_TYPE trigType;
    }
    public class o_trigger : s_object
    {

        public LayerMask layer;
        // [HideInInspector]
        // private gui_dialogue Dialogue;
        public BoxCollider2D colider;
        [HideInInspector]
        public bool isactive = false;
        [HideInInspector]
        const float shutterdepth = 1.55f;
        [HideInInspector]
        public o_character charac;

        //[HideInInspector]
        //Image fade;
        public bool callstatic = false;

        public s_triglink triggerLink;

        [HideInInspector]
        public o_character[] characters;

        [HideInInspector]
        public int ev_num = 0;
        s_object selobj;

        public bool destroyOnTouch = false;
        
        public void Initialize()
        {
            //fade = GameObject.Find("GUIFADE").GetComponent<Image>();
            colider = GetComponent<BoxCollider2D>();
            //if (GameObject.Find("Dialogue"))
            //Dialogue = GameObject.Find("Dialogue").GetComponent<gui_dialogue>();
            rendererObj = GetComponent<SpriteRenderer>();
        }

        public new void Update()
        {
            base.Update();
            if (!s_triggerhandler.trigSingleton.doingEvents)
            {
                switch (TRIGGER_T)
                {
                    case TRIGGER_TYPE.CONTACT:

                        o_character c = IfTouchingGetCol<o_character>(collision);
                        if (c != null)
                        {
                            selobj = c.gameObject.GetComponent<s_object>();
                            //print(name + c.name);
                            if (selobj)
                            {
                                o_character posses = selobj.GetComponent<o_character>();
                                //print(name + c.name);
                                o_character ch = c.GetComponent<o_character>();
                                if (ch)
                                {
                                    if (!ch.AI && ch.control)
                                    {
                                        if (triggerLink == null)
                                        {
                                            foreach (trig_cond tr in conditions) {

                                            }
                                            s_triggerhandler.trigSingleton.selobj = selobj;
                                            s_triggerhandler.trigSingleton.JumpToEvent(eventScript);
                                            if (destroyOnTouch)
                                                Destroy(gameObject);
                                        }
                                        else {
                                            triggerLink.CallEvent();
                                        }
                                        //print("Activating trigger");
                                    }
                                }
                            }
                        }
                        break;

                    case TRIGGER_TYPE.CONTACT_INPUT:
                        if (Input.GetKeyDown(s_globals.GetKeyPref("select")))
                        {
                            c = IfTouchingGetCol<o_character>(collision);
                            if (c != null)
                            {
                                selobj = c.gameObject.GetComponent<s_object>();
                                //print(name + c.name);
                                if (selobj)
                                {
                                    o_character ch = selobj.GetComponent<o_character>();
                                    if (ch)
                                        if (ch.enabled)
                                        {
                                            if (!ch.AI && ch.control)
                                            {
                                                if (triggerLink == null)
                                                {
                                                    s_triggerhandler.trigSingleton.selobj = selobj;
                                                    s_triggerhandler.trigSingleton.JumpToEvent(eventScript);
                                                    if (destroyOnTouch)
                                                        Destroy(gameObject);
                                                }
                                                else
                                                {
                                                    triggerLink.CallEvent();
                                                }
                                            }
                                        }
                                }
                            }
                        }
                        break;
                }
            }

        }

        public void CallTrigger()
        {
            s_triggerhandler.trigSingleton.JumpToEvent(eventScript);
            isactive = true;
        }
        public void IncrementEvent()
        {
            ev_num++;
        }

    }
}
