using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace PSNSample
{
    public class MenuStack
    {
#if UNITY_PS5 || UNITY_PS4
        MenuLayout activeMenu = null;
        List<MenuLayout> menuStack = new List<MenuLayout>();

        public void SetMenu(MenuLayout menu)
        {
            if (activeMenu != null)
            {
                activeMenu.GetOwner().OnExit();
            }
            menuStack.Clear();
            activeMenu = menu;
            activeMenu.GetOwner().OnEnter();
        }

        public MenuLayout GetMenu()
        {
            return activeMenu;
        }

        public void PushMenu(MenuLayout menu)
        {
            if (activeMenu != null)
            {
                activeMenu.GetOwner().OnExit();
            }
            menuStack.Add(activeMenu);
            activeMenu = menu;
            activeMenu.GetOwner().OnEnter();
        }

        public void PopMenu()
        {
            if (menuStack.Count > 0)
            {
                activeMenu.GetOwner().OnExit();
                activeMenu = menuStack[menuStack.Count - 1];
                menuStack.RemoveAt(menuStack.Count - 1);
            }
        }
#endif
    };
}
