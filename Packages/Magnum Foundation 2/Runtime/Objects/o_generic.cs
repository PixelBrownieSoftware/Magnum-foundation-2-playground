using UnityEngine;
using MagnumFoundation2.System;

namespace MagnumFoundation2.Objects
{
    public class o_generic : s_object
    {
        public Vector2 uppercollisionsize { get; set; }
        public Vector2 uppercollision { get; set; }
        public GameObject graphic;
        public bool issolid = true;
        public LayerMask layuer;
        public string character;
        public string characterCannot = null;

        new private void Start()
        {
            base.Start();
            if (transform.Find("sprite_obj"))
                animHand = transform.Find("sprite_obj").GetComponent<Animator>();
        }

        new void Update()
        {
            base.Update();

        }

        private void OnDrawGizmos()
        {
            if (collision == null)
                collision = GetComponent<BoxCollider2D>();
            else
            {
                if (character != "")
                    Gizmos.DrawWireCube(transform.position, collision.size);
            }
        }

    }
}
