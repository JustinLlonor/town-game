using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabButtons : MonoBehaviour
{
    /// <summary>
    /// A tab button and its corresponding menu object
    /// </summary>
    [System.Serializable]
    public struct TabButton
    {
        public Button button;
        public GameObject menu;
    }

    public TabButton[] buttons;
    public Color selectedButtonColor;
    public Color deselectedButtonColor;
    public Color selectedTextColor;
    public Color deselectedTextColor;

    private void Awake()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            TabButton tabButton = buttons[i];
            int n = i;
            tabButton.button.onClick.AddListener(delegate { SelectButton(n); });
        }
        SelectButton(0);
    }

    private void SelectButton(int index)
    {
        PhysTabButton tabButton = buttons[index].button.GetComponent<PhysTabButton>();
        tabButton.SetButtonColor(selectedButtonColor);
        tabButton.SetTextColor(selectedTextColor);
        buttons[index].menu.SetActive(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == index) continue;
            DeselectButton(i);
        }
    }

    private void DeselectButton(int index)
    {
        PhysTabButton tabButton = buttons[index].button.GetComponent<PhysTabButton>();
        tabButton.SetButtonColor(deselectedButtonColor);
        tabButton.SetTextColor(deselectedTextColor);
        buttons[index].menu.SetActive(false);
    }
}
