using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIToolbarControl : MonoBehaviour
    {        
        Stack<Transform> LastClickedButtons = new Stack<Transform>();

        void Start()
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                SetupTopbarMenu(i);
            }
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
            ToggleSubMenus(toolbarButton.transform, false);
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
                    ToggleSubMenus(LastClickedButtons.Peek(), true);
                }
            );
        }

        void BindOnBackButtonClicked(Button button)
        {
            button?.onClick.AddListener(() =>
                {
                    ToggleSubMenus(LastClickedButtons.Pop(), false);
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

                    ToggleSubMenus(LastClickedButtons.Peek(), true);

                    var comp = button.GetComponent<DanbiUIPanelControl>();
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
            ToggleSubMenus(button.transform, false);
        }

        public void ToggleSubMenus(Transform parent, bool flag)
        {
            // child index : 0 -> embedded text, 1 -> vertical layout group.
            parent.GetChild(1).gameObject.SetActive(flag);
        }
    };
};