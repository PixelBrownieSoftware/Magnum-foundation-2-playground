using MagnumFoundation2.System;
using UnityEngine;

namespace MagnumFoundation2.Objects
{
    public class o_bullet : s_object
    {
        public o_character parent;
        public float attack_pow;
        public bool isbullet = true;
        public Vector2 direction { get; set; }
        protected float timer = 0;

        public enum BULLET_TYPE {
            MELEE,
            BULLET,
            HITSCAN
        }
        public BULLET_TYPE bulletObjType;
        public LineRenderer rendLine;

        public void SetTimer(float tmr)
        {
            timer = tmr;
        }

        new protected void Start()
        {
            base.Start();
        }

        public virtual void CheckCharacterIntersect<T>() where T : o_character
        {
            T c = IfTouchingGetCol<T>(collision);
            if (c != null)
            {
                if (c != parent)
                {
                    if (parent.GetTargets().Find(x => x == c) != null)
                    {
                        if (c != parent)
                        {
                            if (c.CHARACTER_STATE != o_character.CHARACTER_STATES.STATE_DASHING)
                            {
                                if (!c.isInvicible)
                                    OnImpact(c);
                            }
                        }
                    }
                }
            }
        }

        new protected void Update()
        {
            base.Update();
            switch (bulletObjType) {
                case BULLET_TYPE.BULLET:
                    transform.Translate(direction * terminalSpeedOrigin * 60 * Time.deltaTime);
                    if (timer < 0) OnImpact(); else
                    {collision.enabled = false; timer -= Time.deltaTime; }
                    break;
                    
                case BULLET_TYPE.MELEE:
                    if (timer < 0) gameObject.SetActive(false); else
                    {collision.enabled = true; timer -= Time.deltaTime; }
                    break;
            }
        }

        public virtual void OnImpact()
        {
            if (isbullet)
                DespawnObject();
        }
        public virtual void OnImpact<T>(T character) where T : o_character
        {
            if (isbullet)
                DespawnObject();
            character.OnHit(this);
        }
    }
}
