using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BuildingChooseUI : MonoBehaviour
{
    public RoomManager roomManager;
    public TextMeshProUGUI selectionText;
    public GameObject selectedText;
    public GameObject selectionButton;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI timer;
    public GameObject error;
    public Color plusEnergy;
    public Color minusEnergy;
    public Color neutralEnergy;

    private void OnEnable()
    {
        roomManager.onSelectionUpdate += ReceiveSelection;
    }

    private void OnDisable()
    {
        roomManager.onSelectionUpdate -= ReceiveSelection;
    }

    private void ReceiveSelection(string roomName, int energyDiff, bool canAfford, bool selected)
    {
        Debug.Log("received selection");
        // If can afford and not selected, show
        selectionButton.SetActive(canAfford && !selected);
        error.SetActive(!canAfford);
        selectedText.SetActive(selected);
        
        // Energy diff code
        if (energyDiff > 0)
        {
            energyText.color = plusEnergy;
            energyText.text = "+";
        }
        else if (energyDiff < 0)
        {
            energyText.color = minusEnergy;
            energyText.text = "-";
        } else
        {
            energyText.color = neutralEnergy;
            energyText.text = "·";
        }

        if (roomName == "house") roomName = "Your house";

        selectionText.text = roomName;
    }
}
