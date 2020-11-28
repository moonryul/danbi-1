using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Danbi
{
    public class DanbiUIToolbar : MonoBehaviour
    {
        [SerializeField, Readonly]
        Stack<Transform> m_clickedButtons = new Stack<Transform>();

        void Start()
        {
            GetComponent<HorizontalLayoutGroup>().spacing = 0.0f;
            for (int i = 0; i < transform.childCount; ++i)
            {
                SetupTopbarMenu(i);
            }
        }

        void SetupTopbarMenu(int index)
        {
            var toolbarButton = transform.GetChild(index).GetComponent<Button>();
            AddListenerForToolbarButtonClicked(toolbarButton);

            var submenuVerticalGroup = toolbarButton.transform.GetChild(1);
            for (int i = 0; i < submenuVerticalGroup.childCount; ++i)
            {
                var submenuButton = submenuVerticalGroup.GetChild(i).GetComponent<Button>();
                if (submenuButton.name.Contains("Back"))
                {
                    AddListenerForBackButtonClicked(submenuButton);
                }
                else
                {
                    AddListenerForSubmenuButtonClickedRecursively(submenuButton);
                }
            }
            ToggleSubMenus(toolbarButton.transform, false);
        }

        void AddListenerForToolbarButtonClicked(Button button)
        {
            button?.onClick.AddListener(
                () =>
                {
                    if (m_clickedButtons.Count == 0)
                    {
                        m_clickedButtons.Push(button.transform);
                    }

                    if (m_clickedButtons.Peek() != button.transform)
                    {
                        m_clickedButtons.Push(button.transform);
                    }
                    ToggleSubMenus(m_clickedButtons.Peek(), true);
                }
            );
        }

        void AddListenerForBackButtonClicked(Button button)
        {
            button?.onClick.AddListener(() =>
                {
                    ToggleSubMenus(m_clickedButtons.Pop(), false);
                }
            );
        }

        void AddListenerForSubmenuButtonClickedRecursively(Button button)
        {
            button?.onClick.AddListener(() =>
                {
                    //button.colors = 
                    // if there's no button input, then push it as a first one.
                    if (m_clickedButtons.Count == 0)
                    {
                        m_clickedButtons.Push(button.transform);
                    }

                    // check the button is already pushed.
                    if (m_clickedButtons.Peek() != button.transform)
                    {
                        m_clickedButtons.Push(button.transform);
                    }

                    // open all of the children buttons.
                    ToggleSubMenus(m_clickedButtons.Peek(), true);

                    var comp = button.GetComponent<DanbiUIPanelControl>();
                    comp?.OnMenuButtonSelected(m_clickedButtons);
                }
            );

            // Get the vertical layout group.
            // 0 -> text(placeholder) so other child is always GetChild(1).
            var submenuVerticalGroup = button.transform.GetChild(1);
            if (submenuVerticalGroup.name.Contains("Vertical"))
            {
                for (int i = 0; i < submenuVerticalGroup.childCount; ++i)
                {
                    var submenuButton = submenuVerticalGroup.GetChild(i).GetComponent<Button>();

                    if (submenuButton.name.Contains("Back"))
                    {
                        AddListenerForBackButtonClicked(submenuButton);
                    }
                    else
                    {
                        AddListenerForSubmenuButtonClickedRecursively(submenuButton);
                    }
                }
            }
            ToggleSubMenus(button.transform, false);
        }

        void ToggleSubMenus(Transform parent, bool flag)
        {
            // child index : 0 -> embedded text, 1 -> vertical layout group.
            parent.GetChild(1).gameObject.SetActive(flag);
        }
    };
};