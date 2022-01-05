using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class s_menuhandler : s_singleton<s_menuhandler>
{
    public List<s_menucontroller> menuControllers;
    public s_menucontroller lastActiveMenu;
    string currentMenuName;

    private void Awake()
    {
        menuControllers = GetComponentsInChildren<s_menucontroller>().ToList();
        menuControllers.ForEach(x => x.gameObject.SetActive(false));
        DontDestroyOnLoad(this);
    }


    public T GetMenu<T>(string _menu) where T : s_menucontroller {
        T menuToSwitchTo = menuControllers.Find(x => x.menuType == _menu).GetComponent<T>();
        return menuToSwitchTo;
    }

    public string GetCurrentMenuName()
    {
        return currentMenuName;
    }

    public void SwitchMenu(string _menu) {
        if (lastActiveMenu != null) {
            lastActiveMenu.gameObject.SetActive(false);
        }

        s_menucontroller menuToSwitchTo = menuControllers.Find(x=> x.menuType == _menu);
        if (menuToSwitchTo != null)
        {
            menuToSwitchTo.gameObject.SetActive(true);
            lastActiveMenu = menuToSwitchTo;
            currentMenuName = menuToSwitchTo.name;
            menuToSwitchTo.OnOpen();
        }
        //print("Switched menu");
    }
}
