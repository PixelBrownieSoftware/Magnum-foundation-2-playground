using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagnumFoundation2.Objects;
using MagnumFoundation2.System;

namespace MagnumFoundation2.Tests.src {

    public class c_player : o_character
    {
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Initialize();
            base.Start();
        }

        new void Update()
        {
            base.Update();
            if (control)
            {
                if (ArrowKeyControlPref())
                {
                    CHARACTER_STATE = CHARACTER_STATES.STATE_MOVING;
                }
                else
                {
                    CHARACTER_STATE = CHARACTER_STATES.STATE_IDLE;
                }
            }
            else
            {
                CHARACTER_STATE = CHARACTER_STATES.STATE_IDLE;
            }
        }
    }
}
