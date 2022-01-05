using System;
using UnityEngine;
using MagnumFoundation2.System;
using System.Collections.Generic;
using MagnumFoundation2.System.Core;

namespace MagnumFoundation2.Objects
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class o_character : s_object
    {
        #region variables
        [Serializable]
        public struct ai_behaviour
        {
            public ai_behaviour(float timer, ai_function func)
            {
                this.func = func;
                this.timer = timer;
            }
            public ai_function func;
            public float timer;
        }
        public List<o_character> targets = new List<o_character>();

        public bool isInvicible = false;
        public float dashDivider = 2.5f;    //Used for when the controlled character can stop dashing
        public bool detailedAnim = false;

        public Vector2 direction;
        [HideInInspector]
        public Vector2 dashDirection;
        public LayerMask collisionLayer;

        public bool IS_KINEMATIC = true;
        public delegate void ai_function();
        //public Stack<ai_behaviour> AI_QUEUED_STATES = new Stack<ai_behaviour>();
        public ai_function currentAIFunction;
        protected float ai_timer = 0;

        [HideInInspector]
        public float damage_timer = 0;
        protected float fallposy = 0;
        protected float dashSpeed = 0;
        Vector2 originSprite;

        public enum CHARACTER_STATES
        {
            STATE_IDLE,
            STATE_MOVING,
            STATE_DASHING,
            STATE_HURT,
            STATE_ATTACK,
            STATE_JUMPING,
            STATE_NOTHING,
            STATE_FALLING,
            STATE_CLIMBING,
            STATE_DEFEAT,
            STATE_PRE_ATTACK,
            STATE_PRE_DASH
        };
        public CHARACTER_STATES CHARACTER_STATE;
        [HideInInspector]
        public GameObject sensro;
        public o_character target;

        public float health;
        public float maxHealth;

        [HideInInspector]
        public Collider2D hitbox;

        [HideInInspector]
        public GameObject SpriteObj;
        protected Vector2 spriteOffset = new Vector2(0, 0);

        protected float velSlip = 0.85f;
        public GameObject attackobject;

        protected float crashTimer = 0;
        public float dashdelayStart = 0;
        public float dashdelay = 0;
        float preDashDelay = 0;
        protected float angle;
        protected Vector3 mouse;
        protected Vector3 spawnpoint;
        protected Vector3 lastposbeforefall;
        public Vector3 offsetCOL;

        public bool AI;
        public bool control = true;
        public string faction;


        public bool AI_timerUp { get { return ai_timer <= 0; } }

        public float walktimer { get; private set; }
        #endregion

        public void Initialize()
        {
            rbody2d = GetComponent<Rigidbody2D>();
            if (transform.Find("sprite_obj") != null) {
                SpriteObj = transform.Find("sprite_obj").gameObject;
                originSprite = SpriteObj.transform.position;
            }
            collision = GetComponent<BoxCollider2D>();
            if (SpriteObj != null)
                rendererObj = SpriteObj.GetComponent<SpriteRenderer>();
            health = maxHealth;
            terminalspd = terminalSpeedOrigin;
            //nodegraph = GameObject.Find("General").GetComponent<s_nodegraph>();
            //cam = GameObject.Find("Camera").GetComponent<Camera>();
        }

        private void Awake()
        {
            Initialize();
        }

        public T CharacterType<T>() where T : o_character
        {
            return GetComponent<T>();
        }
        public T[] CharacterTypeS<T>(List<o_character> charaList) where T : o_character
        {
            List<T> charaListAr = new List<T>();
            for (int i = 0; i < charaList.Count; i++)
            {
                T chara = charaList[i].GetComponent<T>();
                if (chara != null)
                    charaListAr.Add(chara);
            }
            T[] returnVal = charaListAr.ToArray();
            return returnVal;
        }

        public void SetSpawnPoint(Vector2 vec)
        {
            spawnpoint = vec;
        }

        public void HurtFunction(float damage)
        {
            health -= damage;
            damage_timer = 0.4f;
            //Perhaps spawn particles and things here
        }

        protected void SetAIFunction(float timer, ai_function function)
        {
            ai_timer = timer;
            currentAIFunction = function;
        }

        public Vector2 MouseAng()
        {
            Vector2 mouseRet = new Vector2();
            if (cam != null)
                mouseRet = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            else
            {
                cam = Camera.main;
                mouseRet = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            }
            angle = ReturnAngle(new Vector3(mouse.x, mouse.y, 0));
            return mouseRet;
        }

        /*
        protected void PopAIFunction()
        {
            AI_QUEUED_STATES.Pop();
        }
        protected void AddAIFunction(float timer, ai_function function)
        {
            if (ai_timer >= 0)
                ai_timer = timer;
            AI_QUEUED_STATES.Push(new ai_behaviour(timer, function));
        }

        public void SwapAIFunction(float timer, ai_function function)
        {
            if (ai_timer >= 0)
                ai_timer = timer;
            AI_QUEUED_STATES.Pop();
            AI_QUEUED_STATES.Push(new ai_behaviour(timer, function));
        }
        */

        public Vector3 LookAtTarget(o_character target)
        {
            return (target.transform.position - transform.position).normalized;
        }
        public Vector3 LookAtTarget(Vector2 target)
        {
            return (target - (Vector2)transform.position).normalized;
        }

        public bool CheckTargetDistance(o_character target, float distance)
        {
            float calcdis = Vector2.Distance(target.transform.position, transform.position);
            if (Physics2D.Linecast(target.transform.position, transform.position, collisionLayer))
                return false;
            return calcdis <= distance;
        }
        public bool CheckTargetDistance(Vector2 target, float distance)
        {
            float calcdis = Vector2.Distance(target, transform.position);
            if (Physics2D.Linecast(target, transform.position, collisionLayer))
                return false;
            return calcdis <= distance;
        }

        public void SetTransformPar(s_object o)
        {
            parentTrans = o;
        }
        public bool ArrowKeyControlPref()
        {
            if (Input.GetKey(s_globals.GetKeyPref("right")))
            {
                direction.x = 1;
            }
            if (Input.GetKey(s_globals.GetKeyPref("left")))
            {
                direction.x = -1;
            }
            if (Input.GetKey(s_globals.GetKeyPref("up")))
            {
                direction.y = 1;
            }
            if (Input.GetKey(s_globals.GetKeyPref("down")))
            {
                direction.y = -1;
            }
            if (!Input.GetKey(s_globals.GetKeyPref("left")) && !Input.GetKey(s_globals.GetKeyPref("right"))
                && (Input.GetKey(s_globals.GetKeyPref("up")) || Input.GetKey(s_globals.GetKeyPref("down"))))
            {
                direction.x = 0;
            }
            if (!Input.GetKey(s_globals.GetKeyPref("up")) && !Input.GetKey(s_globals.GetKeyPref("down"))
                && (Input.GetKey(s_globals.GetKeyPref("left")) || Input.GetKey(s_globals.GetKeyPref("right"))))
            {
                direction.y = 0;
            }
            if (Input.GetKey(s_globals.GetKeyPref("up")) ||
                Input.GetKey(s_globals.GetKeyPref("down")) ||
                Input.GetKey(s_globals.GetKeyPref("left")) ||
                Input.GetKey(s_globals.GetKeyPref("right")))
            {
                return true;
            }
            return false;
        }

        public bool ArrowKeyControl()
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
                return true;
            }
            //direction = new Vector2(0, 0);
            return false;
        }

        public Vector2 ArrowKeyControlPerfGetVec()
        {
            Vector2 v = new Vector2();
            if (Input.GetKeyDown(s_globals.GetKeyPref("right")))
            {
                v.x = 1;
            }
            if (Input.GetKeyDown(s_globals.GetKeyPref("left")))
            {
                v.x = -1;
            }
            if (!Input.GetKeyDown(s_globals.GetKeyPref("left")) && !Input.GetKeyDown(s_globals.GetKeyPref("right")))
            {
                v.x = 0;
            }
            if (Input.GetKeyDown(s_globals.GetKeyPref("up")))
            {
                v.y = 1;
            }
            if (Input.GetKeyDown(s_globals.GetKeyPref("down")))
            {
                v.y = -1;
            }
            if (!Input.GetKeyDown(s_globals.GetKeyPref("up")) && !Input.GetKeyDown(s_globals.GetKeyPref("down")))
            {
                v.y = 0;
            }
            //direction = new Vector2(0, 0);
            return v;
        }
        public Vector2 ArrowKeyControlGetVec()
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }
            //direction = new Vector2(0, 0);
            return Vector2.zero;
        }
        public virtual void Dash(float preDashDelay, float delay, float sped)
        {
            if (dashdelay > 0)
                return;
            this.preDashDelay = preDashDelay;
            dashdelayStart = delay;
            dashdelay = delay;
            dashSpeed = sped;
            dashDirection = direction;
            CHARACTER_STATE = CHARACTER_STATES.STATE_DASHING;
        }

        public virtual void Dash(float delay)
        {
            if (dashdelay > 0)
                return;
            dashdelayStart = delay;
            dashdelay = delay;
            dashSpeed = 1;
            dashDirection = direction;
            CHARACTER_STATE = CHARACTER_STATES.STATE_DASHING;
        }
        public virtual void Dash(float delay, float sped)
        {
            if (dashdelay > 0)
                return;
            dashdelayStart = delay;
            dashdelay = delay;
            dashSpeed = sped;
            dashDirection = direction;
            CHARACTER_STATE = CHARACTER_STATES.STATE_DASHING;
        }

        public bool Jump(float gravity)
        {
            if (grounded)
            {
                this.gravity = gravity;
                return true;
            }
            return false;
        }

        public virtual void OnHit(o_bullet b)
        {
        }
        public bool JumpWithoutGround(float gravity, float ZPOSlimit)
        {
            if (ZPOSlimit > _Z_offset)
            {
                this.gravity = gravity;
                return true;
            }
            return false;
        }

        public virtual void WalkControl()
        {
            if (walktimer >= 0)
            {
                walktimer -= Time.deltaTime;
                /*
                if (Physics2D.Raycast(transform.position, direction * 4))
                {
                    direction *= -1;
                }
                */
            }
            else
            {
                int random = UnityEngine.Random.Range(0, 2);
                switch (random)
                {
                    case 0:
                        Move();
                        CHARACTER_STATE = CHARACTER_STATES.STATE_MOVING;
                        direction = new Vector2(UnityEngine.Random.Range(-2, 2), UnityEngine.Random.Range(-2, 2)).normalized;
                        break;

                    case 1:
                        CHARACTER_STATE = CHARACTER_STATES.STATE_IDLE;
                        break;
                }
                walktimer = UnityEngine.Random.Range(0.6f, 3);
            }
        }

        public void PlaySound(string sound)
        {
            s_soundmanager.sound.PlaySound(sound);
        }

        public virtual void AnimMove()
        {
            if (detailedAnim)
            {
                int verticalDir = Mathf.RoundToInt(direction.y);
                int horizontalDir = Mathf.RoundToInt(direction.x);

                if (CHARACTER_STATE == CHARACTER_STATES.STATE_MOVING)
                {
                    if (verticalDir == -1 && horizontalDir == 0)
                        SetAnimation("walk_d", true);
                    else if (verticalDir == 1 && horizontalDir == 0)
                        SetAnimation("walk_u", true);
                    else if (horizontalDir == -1 && verticalDir == 1 ||
                        horizontalDir == -1 && verticalDir == -1 || horizontalDir == -1 && verticalDir == 0)
                    {
                        rendererObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        SetAnimation("walk_s", true);
                    }
                    else if (horizontalDir == 1 && verticalDir == 1 ||
                        horizontalDir == 1 && verticalDir == -1 || horizontalDir == 1 && verticalDir == 0)
                    {
                        rendererObj.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));
                        SetAnimation("walk_s", true);
                    }
                }
                else if (CHARACTER_STATE == CHARACTER_STATES.STATE_IDLE)
                {
                    if (verticalDir == -1 && horizontalDir == 0)
                        SetAnimation("idle_d", true);
                    else if (verticalDir == 1 && horizontalDir == 0)
                        SetAnimation("idle_u", true);
                    else if (horizontalDir == -1 && verticalDir == 1 ||
                        horizontalDir == -1 && verticalDir == -1 || horizontalDir == -1 && verticalDir == 0)
                    {
                        rendererObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                        SetAnimation("idle_s", true);
                    }
                    else if (horizontalDir == 1 && verticalDir == 1 ||
                        horizontalDir == 1 && verticalDir == -1 || horizontalDir == 1 && verticalDir == 0)
                    {
                        rendererObj.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));
                        SetAnimation("idle_s", true);
                    }

                }
            }
            else
            {
                if (CHARACTER_STATE == CHARACTER_STATES.STATE_MOVING)
                {

                    int horizontalDir = Mathf.RoundToInt(direction.x);
                    if (horizontalDir == -1)
                    {
                        rendererObj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    }
                    else if (horizontalDir == 1)
                    {
                        rendererObj.transform.rotation = Quaternion.Euler(new Vector3(0, -180, 0));
                    }
                    SetAnimation("walk_d", true);
                }
                if (CHARACTER_STATE == CHARACTER_STATES.STATE_IDLE)
                {
                    SetAnimation("idle_d", true);
                }
            }
        }
        public void CheckForVelLimits()
        {
            if (Mathf.Abs(rbody2d.velocity.y) > terminalspd)
            {
                float signedVel = Mathf.Sign(rbody2d.velocity.y);
                rbody2d.velocity = new Vector2(rbody2d.velocity.x, terminalspd * signedVel);
            }
            if (Mathf.Abs(rbody2d.velocity.x) > terminalspd)
            {
                float signedVel = Mathf.Sign(rbody2d.velocity.x);
                rbody2d.velocity = new Vector2(terminalspd * signedVel, rbody2d.velocity.y);
            }
        }

        protected void SetAttackObject<T>(string attkObName, float pow) where T : o_bullet
        {
            if (transform.Find(attkObName) != null)
            {
                attackobject = transform.Find(attkObName).gameObject;
                attackobject.GetComponent<o_bullet>().attack_pow = pow;
                attackobject.GetComponent<o_bullet>().parent = this;
            }
        }
        protected void SetAttackObject<T>(string attkObName) where T : o_bullet
        {
            if (transform.Find(attkObName) != null)
            {
                attackobject = transform.Find(attkObName).gameObject;
                attackobject.GetComponent<o_bullet>().attack_pow = 1;
                attackobject.GetComponent<o_bullet>().parent = this;
            }
        }
        protected void SetAttackObject<T>() where T : o_bullet
        {
            if (transform.Find("attack_object") != null)
            {
                attackobject = transform.Find("attack_object").gameObject;
                attackobject.GetComponent<o_bullet>().attack_pow = 1;
                attackobject.GetComponent<o_bullet>().parent = this;
            }
        }
        protected void SetAttackObject<T>(int pow) where T : o_bullet
        {
            if (transform.Find("attack_object") != null)
            {
                attackobject = transform.Find("attack_object").gameObject;
                attackobject.GetComponent<o_bullet>().attack_pow = pow;
                attackobject.GetComponent<o_bullet>().parent = this;
            }
        }

        protected void AddForce(Vector2 mouse, float power)
        {

        }

        public List<o_character> GetTargets()
        {
            return targets;
        }
        public T GetClosestTarget<T>(float range) where T : o_character
        {
            T[] targs = FindObjectsOfType<T>();
            T targ = null;
            float smallest = float.MaxValue;
            foreach (T c in targs)
            {
                if (c == null)
                    continue;
                if (c == this)
                    continue;
                if (c.faction == faction)
                    continue;
                if (!c.gameObject.activeSelf)
                    continue;
                float dist = TargetDistance(c);
                if (dist > range)
                    continue;
                //Get this target for now if nothing else is better
                if (targ == null)
                    targ = c;
                if (dist <= smallest)
                {
                    targ = c;
                    smallest = dist;
                }
            }
            return targ;
        }
        public T GetClosestTarget<T>() where T : o_character
        {
            T[] targs = FindObjectsOfType<T>();
            T targ = null;
            float smallest = float.MaxValue;
            foreach (T c in targs)
            {
                if (c == this)
                    continue;
                if (c.faction == faction)
                    continue;
                float dist = TargetDistance(c);
                if (dist < smallest)
                {
                    targ = c;
                    smallest = dist;
                }
            }
            return targ;
        }

        public float TargetDistance(o_character targ)
        {
            return Vector2.Distance(targ.transform.position, transform.position);
        }

        public void AddFactions(List<o_character> alc)
        {
            targets.Clear();
            foreach (o_character c in alc)
            {
                if (c == null)
                    continue;
                if (c == this)
                    continue;
                if (c.faction == "")
                {
                    targets.Add(c);
                    continue;
                }
                if (c.faction == faction
                    || c.faction == "N/A"
                    || c.faction == "N/a"
                    || c.faction == "n/A"
                    || c.faction == "n/a")
                    continue;
                targets.Add(c);
            }
        }

        protected void EnableAttack(BoxCollider2D col)
        {
            if (col != null)
            {
                col.gameObject.SetActive(true);
                attackobject.transform.localPosition = direction * 20;
            }
        }
        protected void EnableAttack()
        {
            if (attackobject != null)
            {
                attackobject.SetActive(true);
                attackobject.transform.localPosition = direction * 20;
            }
        }
        protected void EnableAttack(Vector2 dir)
        {
            if (attackobject != null)
            {
                attackobject.GetComponent<o_bullet>().SetTimer(0.05f);
                attackobject.SetActive(true);
                attackobject.GetComponent<o_bullet>().bulletObjType = o_bullet.BULLET_TYPE.MELEE;
                attackobject.transform.localPosition = dir * 26;
                attackobject.GetComponent<o_bullet>().direction = dir;
            }
        }
        protected void EnableAttack(Vector2 dir, float timr)
        {
            if (attackobject != null)
            {
                attackobject.GetComponent<o_bullet>().SetTimer(timr);
                attackobject.SetActive(true);
                attackobject.GetComponent<o_bullet>().bulletObjType = o_bullet.BULLET_TYPE.MELEE;
                attackobject.transform.localPosition = dir * 20;
                attackobject.GetComponent<o_bullet>().direction = dir;
            }
        }
        protected void DisableAttack()
        {
            if (attackobject != null)
                attackobject.SetActive(false);
        }
        protected void DisableAttack(BoxCollider2D col)
        {
            if (col != null)
                col.gameObject.SetActive(false);
        }

        protected float ReturnAngle2(Vector3 position)
        {
            Vector2 unitvec = position.normalized;
            return Mathf.Atan2(unitvec.x, unitvec.y) * Mathf.Rad2Deg;
        }
        protected float ReturnAngle(Vector3 position)
        {
            return 360 - Mathf.Atan2(position.x, position.y) * Mathf.Rad2Deg;
        }

        public void AssignToFaction()
        {

        }

        /// <summary>
        /// This is the state when the enemy does not really see their target. They don't really do much other than just walk around.
        /// Maybe if a sound is heard they could walk towards it.
        /// </summary>
        public virtual void IdleState() { }

        /// <summary>
        /// This is the state which the enemy would enter into when they are done attacking or possibly low on health. This is so that they do not just
        /// run aimlessly towards the player and constantly attack.
        /// </summary>
        public virtual void RetreatState() { }

        /// <summary>
        /// 
        /// </summary>
        public virtual void AttackState() { }

        /// <summary>
        /// This is the state where the AI would chase their target, it would often lead into the attack state or the retreat state
        /// </summary>
        public virtual void ActiveState() { }

        Vector2 colPos;
        public bool CheckIfCornered(Vector3 dir)
        {
            Vector2 siz = collision.size * 0.9f;

            Vector2 offset =
                new Vector2(
                    collision.transform.position.x + collision.offset.x,
                    collision.transform.position.y + collision.offset.y);

            colPos = (Vector3)offset + dir * 2;
            Collider2D col = Physics2D.OverlapBox(colPos, siz, 0, collisionLayer);

            if (col != null)
                return true;
            return false;
        }

        void Move()
        {
            switch (CHARACTER_STATE)
            {
                case CHARACTER_STATES.STATE_NOTHING:
                    if (crashTimer > 0)
                        crashTimer -= Time.deltaTime;
                    if (crashTimer <= 0)
                    {
                        dashdelay = 0;
                        CHARACTER_STATE = CHARACTER_STATES.STATE_IDLE;
                        AfterDash();
                    }
                    break;

                case CHARACTER_STATES.STATE_IDLE:
                    if (rbody2d != null)
                        rbody2d.velocity *= 0.85f;
                    break;

                case CHARACTER_STATES.STATE_MOVING:
                    if (rbody2d != null)
                        rbody2d.velocity = direction * terminalspd;
                    //transform.Translate(velocity * Time.deltaTime);
                    break;

                case CHARACTER_STATES.STATE_FALLING:
                    transform.position -= new Vector3(0, 5, 0);
                    if (transform.position.y <= fallposy)
                    {
                        dashdelay = 0;
                        CHARACTER_STATE = CHARACTER_STATES.STATE_IDLE;
                    }
                    break;

                case CHARACTER_STATES.STATE_HURT:
                    if (damage_timer <= 0)
                    {
                        rendererObj.color = Color.white;
                        dashdelay = 0;
                        CHARACTER_STATE = CHARACTER_STATES.STATE_IDLE;
                    }
                    else
                    {
                        rendererObj.color = Color.red;
                    }
                    break;

                case CHARACTER_STATES.STATE_PRE_DASH:
                    preDashDelay -= Time.deltaTime;
                    if (preDashDelay <= 0)
                    {
                        CHARACTER_STATE = CHARACTER_STATES.STATE_DASHING;
                    }
                    break;

                case CHARACTER_STATES.STATE_DASHING:
                    if (dashdelay <= 0)
                    {
                        AfterDash();
                    }
                    else
                    {
                        rbody2d.velocity = dashDirection * terminalspd * dashSpeed;
                        if (dashdelay < dashdelayStart / dashDivider)
                            if (!AI)
                                if (!Input.GetKey(s_globals.GetKeyPref("dash")))
                                    dashdelay = 0;

                        dashdelay -= Time.deltaTime;
                        //Check for a wall in front if NOCLIP mode isn't on
                        if (!collision.isTrigger && CheckIfCornered(dashDirection * 1.5f))
                        {
                            CrashAfterDash();
                        }
                    }
                    break;
            }

            if (SpriteObj != null)
            {
                //SpriteObj.transform.position = new Vector3(originSprite.x + spriteOffset.x, originSprite.y + spriteOffset.y + _Z_offset);
            }
            /*
            if (IS_KINEMATIC)
            {
                collision.isTrigger = false;
            }
            else
            {
                collision.isTrigger = true;
            }
            */
        }

        public virtual void AfterDash()
        {
            CHARACTER_STATE = CHARACTER_STATES.STATE_MOVING;
        }

        public virtual void AfterDefeat()
        {
        }

        public virtual void CrashAfterDash()
        {
            crashTimer = 0.38f;
            rbody2d.velocity = Vector2.zero;
            CHARACTER_STATE = CHARACTER_STATES.STATE_NOTHING;
        }
        public void CrashOnDash()
        {
        }

        public new void Update()
        {
            base.Update();
            if (control)
            {
                if (health > 0)
                {
                    if (AI)
                        ArtificialIntelleginceControl();
                    else
                        PlayerControl();
                }
            }
        }

        protected void FixedUpdate()
        {
            if (damage_timer > 0)
            {
                damage_timer -= Time.deltaTime;
                rendererObj.color = Color.red;
            }

            if (ai_timer > 0)
                ai_timer = ai_timer - Time.deltaTime;

            if (health <= 0)
            {
                if (CHARACTER_STATE != CHARACTER_STATES.STATE_DEFEAT)
                {
                    AfterDefeat();
                    CHARACTER_STATE = CHARACTER_STATES.STATE_DEFEAT;
                }
            }
            if (CHARACTER_STATE == CHARACTER_STATES.STATE_IDLE)
            {
                rbody2d.velocity *= velSlip;
            }
            Move();

        }



        public virtual void PlayerControl()
        {

        }

        public virtual void ArtificialIntelleginceControl()
        {

        }

        public void Pushforce(Vector2 dir, float force)
        {
            rbody2d.velocity += dir * force;
        }

        public void TeleportAfterFall()
        {
            transform.position = lastposbeforefall;
        }

        public void Hurt(o_bullet b)
        {
            //CHARACTER_STATE = CHARACTER_STATES.STATE_HURT;
            HurtFunction(b.attack_pow);
            Pushforce(b.direction, 135f);
        }

        protected T GetBullet<T>(BoxCollider2D collisn) where T : o_bullet
        {
            if (collisn == null)
                return null;

            Collider2D[] chara = Physics2D.OverlapBoxAll(transform.position, collisn.size, 0);

            if (chara == null)
                return null;
            for (int i = 0; i < chara.Length; i++)
            {
                Collider2D co = chara[i];
                if (co.gameObject == gameObject)
                    continue;

                T b = co.gameObject.GetComponent<T>();
                if (b == null)
                    continue;
                if (targets.Find(x => x == b.parent) != null)
                {
                    o_character host = b.parent;
                    if (host != this)
                    {
                        return b;
                    }
                }
            }
            return null;
        }

    }
}
