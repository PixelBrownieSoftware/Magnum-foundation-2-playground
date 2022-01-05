using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class s_menuswitcher : s_button
{
    protected override void OnButtonClicked()
    {
        switch (buttonType)
        {
            default:
                canvasManager.SwitchMenu(buttonType);
                break;
        }
    }
}
