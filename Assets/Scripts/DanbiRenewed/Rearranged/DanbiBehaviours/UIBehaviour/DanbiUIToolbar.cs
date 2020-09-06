using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIToolbar : MonoBehaviour
    {
        Stack<Transform> LastClickedButtons = new Stack<Transform>();

        void Start()
        {
            SetupTopbarMenu(0);
            SetupTopbarMenu(1);
            SetupTopbarMenu(2);
            //SetupTopbarMenu(3);
        }

        void SetupTopbarMenu(int index)
        {
            var toolbarButton = transform.GetChild(index).GetComponent<Button>();
            BindOnToolbarButtonClicked(toolbarButton);

            var submenuVerticalGroup = toolbarButton.transform.GetChild(1);
            for (int i = 0; i < submenuVerticalGroup.childCount; ++i)
            {
                var submenuButton = submenuVerticalGroup.GetChild(i).GetComponent<Button>();
                if (submenuButton.name.Contains("Back"))
                {
                    BindOnBackButtonClicked(submenuButton);
                }
                else
                {
                    BindOnSubmenuButtonClicked(submenuButton);
                }
            }
            DanbiUIMenu.ToggleSubMenus(toolbarButton.transform, false);
        }

        void BindOnToolbarButtonClicked(Button button)
        {
            button?.onClick.AddListener(
                () =>
                {
                    if (LastClickedButtons.Count == 0)
                    {
                        LastClickedButtons.Push(button.transform);
                    }

                    if (LastClickedButtons.Peek() != button.transform)
                    {
                        LastClickedButtons.Push(button.transform);
                    }
                    DanbiUIMenu.ToggleSubMenus(LastClickedButtons.Peek(), true);
                }
            );
        }

        void BindOnBackButtonClicked(Button button)
        {
            button?.onClick.AddListener(() =>
                {
                    DanbiUIMenu.ToggleSubMenus(LastClickedButtons.Pop(), false);
                }
            );
        }

        void BindOnSubmenuButtonClicked(Button button)
        {
            button?.onClick.AddListener(() =>
                {
                    if (LastClickedButtons.Count == 0)
                    {
                        LastClickedButtons.Push(button.transform);
                    }

                    if (LastClickedButtons.Peek() != button.transform)
                    {
                        LastClickedButtons.Push(button.transform);
                    }

                    DanbiUIMenu.ToggleSubMenus(LastClickedButtons.Peek(), true);

                    var comp = button.GetComponent<DanbiUIBaseElement>();
                    comp?.OnMenuButtonSelected(LastClickedButtons);
                }
            );

            var submenuVerticalGroup = button.transform.GetChild(1);
            if (submenuVerticalGroup.name.Contains("Vertical"))
            {
                for (int i = 0; i < submenuVerticalGroup.childCount; ++i)
                {
                    var submenuButton = submenuVerticalGroup.GetChild(i).GetComponent<Button>();

                    if (submenuButton.name.Contains("Back"))
                    {
                        BindOnBackButtonClicked(submenuButton);
                    }
                    else
                    {
                        Debug.Log($"Button: {submenuButton.name} is bound!", this);
                        BindOnSubmenuButtonClicked(submenuButton);
                    }
                }
            }
            DanbiUIMenu.ToggleSubMenus(button.transform, false);
        }
    };
};