using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;
using MagnumFoundation2.Objects;
using MagnumFoundation2.System;
using MagnumFoundation2.System.Core;

namespace MagnumFoundation2
{
    public class s_mapholder : MonoBehaviour
    {
        public ev_script eventScript;
        public Vector2 mapSize;
        public List<o_character> allChacacters = new List<o_character>();
        public AudioClip BGM;

        public GameObject pointA;
        public GameObject pointB;
        public GameObject pointC;
        public GameObject pointD;
        public GameObject pointE;
        public GameObject pointF;
        public GameObject pointG;
        public GameObject pointH;
        public GameObject pointI;
        public trig_cond[] conditions;

        bool didEvent = false;
        bool initialized = false;

        protected void Start()
        {
            s_camera.cam.mapSize = mapSize;
            if (GameObject.Find("Collision"))
            {
                Tilemap tmC = GameObject.Find("Collision").GetComponent<Tilemap>();
                if (tmC != null)
                    tmC.color = Color.clear;
            }
            if(BGM != null)
                StartCoroutine(s_BGM.GetInstance().FadeInMusic(BGM, 1.5f));

            if (allChacacters.Count == 0)
            {
                allChacacters = new List<o_character>();
            }
            switch (s_globals.globalSingle.tpPointFlag)
            {

                default:
                    break;

                case 'a':
                    if (pointA != null)
                        s_globals.globalSingle.player.transform.position = pointA.transform.position;
                    break;

                case 'b':
                    s_globals.globalSingle.player.transform.position = pointB.transform.position;
                    break;

                case 'c':
                    s_globals.globalSingle.player.transform.position = pointC.transform.position;
                    break;

                case 'd':
                    s_globals.globalSingle.player.transform.position = pointD.transform.position;
                    break;

                case 'e':
                    s_globals.globalSingle.player.transform.position = pointE.transform.position;
                    break;

                case 'f':
                    s_globals.globalSingle.player.transform.position = pointF.transform.position;
                    break;

                case 'g':
                    s_globals.globalSingle.player.transform.position = pointG.transform.position;
                    break;
            }
            s_globals.globalSingle.currentMapName = gameObject.scene.name;
            //print("This is open");
            initialized = true;
        }

        public void Update()
        {
            if (!didEvent && initialized)
            {
                if (!s_triggerhandler.trigSingleton.doingEvents) {

                    if (conditions != null)
                    {
                        bool fufilled = false;
                        foreach (trig_cond condition in conditions)
                        {
                            bool cond = false;
                            int flagNum = s_globals.GetGlobalFlag(condition.condFlgName);

                            switch (condition.logicType)
                            {
                                case ev_details.LOGIC_TYPE.VAR_EQUAL:
                                    cond = (flagNum == condition.condFlgNum);
                                    break;

                                case ev_details.LOGIC_TYPE.VAR_GREATER:
                                    cond = (flagNum > condition.condFlgNum);
                                    break;

                                case ev_details.LOGIC_TYPE.VAR_LESS:
                                    cond = (flagNum < condition.condFlgNum);
                                    break;

                                case ev_details.LOGIC_TYPE.VAR_NOT_EQUAL:
                                    cond = (flagNum != condition.condFlgNum);
                                    break;
                                case ev_details.LOGIC_TYPE.NONE:
                                    cond = true;
                                    break;
                            }
                            if (cond)
                            {
                                fufilled = true;
                                if (condition.isDestroy)
                                {
                                    foreach (GameObject o in condition.linkedObjects)
                                    {
                                        Destroy(o);
                                    }
                                }
                                else
                                {
                                    if (condition.script != null)
                                    {
                                        didEvent = true;
                                        s_triggerhandler.trigSingleton.JumpToEvent(condition.script);
                                        break;
                                    }
                                }
                            }
                            if (!cond)
                            {
                                if (!condition.isDestroy)
                                {
                                    foreach (GameObject o in condition.linkedObjects)
                                    {
                                        Destroy(o);
                                    }
                                }
                            }
                        }
                        if (!fufilled)
                        {
                            didEvent = true;
                            s_globals.allowPause = true;
                        }
                    }
                    else
                    {
                        didEvent = true;
                        s_globals.allowPause = true;
                    }
                }
            }
        }

        public T AddCharacter<T>(o_character c, Vector3 pos)
        {
            GameObject entit = GameObject.Find("Entities");
            o_character cha = Instantiate<o_character>(c, pos, Quaternion.identity);
            cha.transform.SetParent(entit.transform);
            allChacacters.Add(cha);
            cha.AddFactions(allChacacters);
            return cha.GetComponent<T>();
        }
    }
}
