using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_menucontroller : MonoBehaviour
{
    public string menuType;
    public s_button[] buttons;

    public void ResetButton()
    {
        foreach (s_button b in buttons)
        {
            ResetButton<s_button>(b);
        }
    }
    public virtual void ResetButton<T>(T b) where T : s_button {

        b.gameObject.SetActive(false);
    }

    public T GetButton<T>(int i) where T : s_button{
        buttons[i].gameObject.SetActive(true);
        return buttons[i].GetComponent<T>();
    }

    public virtual void OnOpen() {
        if (buttons != null) {
            foreach (s_button b in buttons)
            {
                b.OnStart();
            }
        }
    }
}
