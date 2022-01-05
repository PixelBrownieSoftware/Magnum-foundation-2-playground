using System;
using System.Collections.Generic;
using MagnumFoundation2.Objects;
using MagnumFoundation2.System.Core;
using UnityEngine;

namespace MagnumFoundation2.System
{
    public abstract class s_object : MonoBehaviour
    {
        public string ID; //To return back to the object pooler
        [HideInInspector]
        public GameObject shadow;
        public float _Z_offset;  //To show the character jumping
        public int Z_floor;
        //[HideInInspector]
        public BoxCollider2D collision;
        //[HideInInspector]
        //public s_nodegraph nodegraph;
        public float terminalSpeedOrigin;
        protected float terminalspd;
        public float gravity;
        [HideInInspector]
        public Rigidbody2D rbody2d;
        protected s_object parentTrans;
        public SpriteRenderer rendererObj;
        public Animator animHand;
        protected Camera cam;

        public bool useCondition = false;
        public bool disspearOnConditionTrue = false;
        public ev_details.LOGIC_TYPE logicType;
        public string flagName;
        public int conditionNum;

        const float wldgravity = 3.98f;
        public bool isHover = false;
        public bool grounded = true;

        public bool switchA;
        public bool switchB;
        public bool switchC;
        public bool switchD;

        public trig_cond[] conditions;
        public ev_script eventScript;
        public enum TRIGGER_TYPE
        {
            CONTACT,
            CONTACT_INPUT,
            NONE
        }
        public TRIGGER_TYPE TRIGGER_T;

        public void Start()
        {
            collision = GetComponent<BoxCollider2D>();
            if (rendererObj != null)
                animHand = rendererObj.GetComponent<Animator>();
            triggerState st = s_globals.globalSingle.GetTriggerState(this);
            if (st != null) {
                switchA = st.switchA;
                switchB = st.switchB;
                switchC = st.switchC;
                switchD = st.switchD;
            }
        }

        private void Awake()
        {
            if (conditions != null)
            {
                foreach (trig_cond condition in conditions)
                {
                    bool cond = false;
                    if (condition.checkFlag)
                    {
                        int flagNum = s_globals.GetGlobalFlag(condition.condFlgName);
                        //print("flag is " + flagNum);
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
                        }
                    }
                    else
                    {
                        if (condition.checkSwitches)
                        {
                            if (switchA && condition.checkA)
                                cond = true;
                            if (switchB && condition.checkB)
                                cond = true;
                            if (switchC && condition.checkC)
                                cond = true;
                            if (switchD && condition.checkD)
                                cond = true;
                        }
                        else
                        {
                            cond = true;
                        }
                    }
                    if (!condition.checkSwitches && !condition.checkFlag)
                    {
                        eventScript = condition.script;
                        if (condition.setTriggerType)
                        {
                            TRIGGER_T = condition.trigType;
                        }
                    }

                    /*
                    if (!cond)
                    {
                        if (condition.isDestroy)
                        {
                            foreach (GameObject o in condition.linkedObjects)
                            {
                                Destroy(o);
                            }
                            Destroy(gameObject);
                        }
                    }
                    */
                    if (cond)
                    {
                        if (condition.isDestroy)
                        {
                            foreach (GameObject o in condition.linkedObjects)
                            {
                                Destroy(o);
                            }
                            Destroy(gameObject);
                        }
                        else
                        {
                            eventScript = condition.script;
                            break;
                        }
                    }
                }
            }
        }

        public void DespawnObject()
        {
            s_objpooler.GetInstance().DespawnObject(this);
        }

        private void OnDrawGizmos()
        {
            if (collision != null)
                Gizmos.DrawWireCube(transform.position, collision.size);
        }

        /*
        public s_node CheckNode(float x, float y)
        {
            return CheckNode(new Vector2(x, y));
        }
        public s_node CheckNode(Vector2 v)
        {
            if (nodegraph == null)
                return null;
            s_node no = nodegraph.PosToNode(v);
            if (no == null)
                return null;
            else
                return no;
        }
        */

        public void SetAnimation(string anim, bool isloop, float speed)
        {
            if (animHand != null)
            {
                animHand.speed = speed;
                animHand.Play(anim);
            }
        }
        public void SetAnimation(string anim, bool isloop)
        {
            if (animHand != null)
            {
                animHand.speed = 1;
                animHand.Play(anim);
            }
        }

        protected void Update()
        {
            if (conditions != null)
            {
                foreach (trig_cond condition in conditions)
                {
                    bool cond = false;
                    if (condition.checkFlag)
                    {
                        int flagNum = s_globals.GetGlobalFlag(condition.condFlgName);
                        //print("flag is " + flagNum);
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
                        }
                    }
                    else
                    {
                        if (condition.checkSwitches)
                        {
                            if (switchA && condition.checkA)
                                cond = true;
                            if (switchB && condition.checkB)
                                cond = true;
                            if (switchC && condition.checkC)
                                cond = true;
                            if (switchD && condition.checkD)
                                cond = true;
                        }
                    }
                    if (!condition.checkSwitches && !condition.checkFlag)
                    {
                        eventScript = condition.script;
                    }

                    /*
                    if (!cond)
                    {
                        if (condition.isDestroy)
                        {
                            foreach (GameObject o in condition.linkedObjects)
                            {
                                Destroy(o);
                            }
                            Destroy(gameObject);
                        }
                    }
                    */
                    if (cond)
                    {
                        if (condition.isDestroy)
                        {
                            foreach (GameObject o in condition.linkedObjects)
                            {
                                Destroy(o);
                            }
                            Destroy(gameObject);
                        }
                        else
                        {
                            eventScript = condition.script;
                            break;
                        }
                    }
                }
            }
            if (!isHover)
                _Z_offset += gravity;

            if (_Z_offset > 0)
            {
                grounded = false;
                if (!isHover)
                    gravity -= Time.deltaTime * wldgravity;
            }
            else
            {
                if (!grounded)
                    OnGround();
                grounded = true;
                _Z_offset = 0;
            }
        }
        public virtual void OnGround()
        {

        }

        public void PlaySound(AudioClip cl)
        {
            s_soundmanager.GetInstance().PlaySound(cl);
        }
        
        public Collider2D GetCharacter(BoxCollider2D collisn) { return Physics2D.OverlapBox(transform.position, collisn.size, 0); }
        public Collider2D GetCharacter(BoxCollider2D collisn, string name)
        {
            Collider2D col = Physics2D.OverlapBox(collisn.transform.position, collisn.size, 0);
            if (col == null)
                return null;

            if (col.name == name)
                return col;
            else return null;
        }
        public Collider2D GetCharacter(BoxCollider2D collisn, int lay)
        {
            return Physics2D.OverlapBox(collisn.transform.position, collisn.size, 0, lay);
        }
        public Collider2D GetCharacter(BoxCollider2D collisn, int lay, string nameofobj)
        {
            Collider2D col = Physics2D.OverlapBox(collisn.transform.position, collisn.size, 0, lay);

            if (col == null)
                return null;
            //print(col.name);
            if (col.name == nameofobj)
                return col;
            else return null;
        }

        public Collider2D GetAllCharacters(BoxCollider2D collisn) { return Physics2D.OverlapBox(transform.position, collisn.size, 0); }

        protected T IfTouchingGetCol<T>(BoxCollider2D collisn) where T : s_object
        {
            if (collisn == null)
                return null;

            Collider2D[] chara = Physics2D.OverlapBoxAll(collisn.transform.position + (Vector3)collisn.offset, collisn.size, 0);

            if (chara == null)
                return null;
            for (int i = 0; i < chara.Length; i++)
            {
                Collider2D co = chara[i];
                if (co.gameObject == gameObject)
                    continue;

                T obj = co.gameObject.GetComponent<T>();
                if (obj == null)
                    continue;
                return obj;
            }
            return null;
        }
        protected Collider2D IfTouchingGetCol<T>(BoxCollider2D collisn, string character) where T : s_object
        {
            if (collisn == null)
                return null;

            Collider2D[] chara = Physics2D.OverlapBoxAll(collisn.transform.position + (Vector3)collisn.offset, collisn.size, 0);

            if (chara == null)
                return null;
            for (int i = 0; i < chara.Length; i++)
            {
                Collider2D co = chara[i];
                if (co.gameObject == gameObject)
                    continue;

                //print(chara.gameObject.GetComponent<s_object>().GetType());
                s_object obj = chara[i].gameObject.GetComponent<T>();
                if (obj == null)
                    continue;
                if (obj.name == character)
                    return co;
            }
            return null;
        }
        protected Collider2D IfTouchingGetCol(BoxCollider2D collisn, float size_multip, int layer)
        {
            if (collisn == null)
                return null;

            return Physics2D.OverlapBox(collisn.transform.position + (Vector3)collisn.offset, collisn.size * size_multip, 0, layer);
        }

        internal bool IfTouching(BoxCollider2D collisn)
        {
            if (collisn == null)
                return false;

            return Physics2D.OverlapBox(transform.position, collisn.size, 0);
        }
        protected bool IfTouching(BoxCollider2D collisn, string nameofobj)
        {
            Collider2D col = Physics2D.OverlapBox(transform.position + collisn.transform.position, collisn.size, 0);
            if (collisn == null)
                return false;

            if (col == null)
                return false;

            if (col.gameObject == gameObject)
                return false;
            if (col)
                return col.name == nameofobj;

            return false;
        }
        protected bool IfTouching(BoxCollider2D collisn, object characterdata)
        {
            if (collisn == null)
                return false;

            Collider2D chara = Physics2D.OverlapBox(collisn.transform.position, collisn.size, 0);

            if (chara == null)
                return false;
            if (chara.gameObject == gameObject)
                return false;

            //print(chara.gameObject.GetComponent<s_object>().GetType());
            s_object obj = chara.gameObject.GetComponent<s_object>();
            if (obj == null)
                return false;

            if (obj.GetType() == characterdata.GetType())
                return true;

            return false;
        }
        protected bool IfTouching(BoxCollider2D collisn, int layer)
        {
            if (collisn == null)
                return false;

            return Physics2D.OverlapBox(transform.position, collisn.size, 0, layer);
        }
        protected bool IfTouching(BoxCollider2D collisn, float size_multip, int layer)
        {
            if (collisn == null)
                return false;

            return Physics2D.OverlapBox(transform.position, collisn.size * size_multip, 0, layer);
        }
        protected bool IfTouching(BoxCollider2D collisn, int layer, string nameofobj)
        {
            if (collisn == null)
                return false;
            Collider2D cap = Physics2D.OverlapBox(transform.position, collisn.size, 0, layer);

            if (cap == null)
                return false;

            if (cap == this)
                return false;

            if (cap.name == name)
                return cap;

            return false;
        }
    }
}
