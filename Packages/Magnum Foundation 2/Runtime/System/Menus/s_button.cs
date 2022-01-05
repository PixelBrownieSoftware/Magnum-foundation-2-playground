using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MagnumFoundation2.System.Core;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
[RequireComponent(typeof(Button))]
public class s_button : MonoBehaviour
{
    public string buttonType;

    protected s_menuhandler canvasManager;
    protected Button menuButton;
    protected EventTrigger eventTrigger;
    public Text txt;
    public Image buttonTex;
    public bool isPause = true;
    public bool allowMenuPause= true;

    protected void Start()
    {
        buttonTex = GetComponent<Image>();
        eventTrigger = GetComponent<EventTrigger>();
        EventTrigger.Entry ent = new EventTrigger.Entry();
        ent.eventID = EventTriggerType.PointerEnter;
        ent.callback.AddListener((eventData) => { OnHover(); });

        eventTrigger.triggers.Add(ent);
        menuButton = GetComponent<Button>();
        menuButton.onClick.AddListener(OnButtonClicked);
        canvasManager = s_menuhandler.GetInstance();
    }

    protected virtual void OnHover() {

    }

    public virtual void OnStart() {

    }

    protected virtual void OnButtonClicked()
    {
        switch (buttonType)
        {
            default:
                if (buttonType != "")
                {
                    canvasManager.SwitchMenu(buttonType);
                    if (isPause)
                    {
                        s_globals.allowPause = false;
                        Time.timeScale = 0;
                    }
                    else
                    {
                        if (allowMenuPause)
                            s_globals.allowPause = true;
                        else
                            s_globals.allowPause = false;
                        Time.timeScale = 1;
                    }
                }
                break;
        }
    }

}
