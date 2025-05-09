using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JobDescUI : MonoBehaviour
{
    public GameObject descriptionObject;
    public TextMeshProUGUI jobTitle;
    public TextMeshProUGUI jobPlayerCount;
    public TextMeshProUGUI jobDescription;
    public TextMeshProUGUI deadlineText;
    public TextMeshProUGUI accessText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI timeText;
    public Button button;
    public TextMeshProUGUI buttonText;
    public TextMeshProUGUI errorText;

    public void ToggleDescription(bool enabled)
    {
        descriptionObject.SetActive(enabled);
    }

    public void SetTitle(string title)
    {
        jobTitle.text = title;
    }

    public void UpdatePlayerCount(int playerCount, int maxPlayers)
    {
        if (playerCount < 0 || maxPlayers < 0)
        {
            jobPlayerCount.text = "(-/-)";
        }
        jobPlayerCount.text = "(" + playerCount + "/" + maxPlayers + ")";
    }

    public void SetDescription(string description)
    {
        jobDescription.text = description;
    }

    /// <summary>
    /// Sets the deadline. Convert the period time of the deadline to clock time before putting it in here.
    /// </summary>
    /// <param name="deadlineTime"></param>
    public void SetDeadline(string deadlineTime)
    {
        deadlineText.gameObject.SetActive(true);
        deadlineText.text = "Application Closes " + deadlineTime;
    }

    public void HideDeadline()
    {
        deadlineText.gameObject.SetActive(false);
    }

    public void SetAccess(string[] accessList)
    {
        string accessString = "Access to ";
        for (int i = 0; i  < accessList.Length; i++)
        {
            accessString += accessList[i];
            if (i != accessList.Length - 1) accessString += ", ";
            if (i == accessList.Length - 2) accessString += "and ";
        }
        accessText.text = accessString;
    }

    public void SetPay(Job.PayLevel pay)
    {
        moneyText.text = pay.ToString() + " Pay";
    }

    public void SetHours(Job.TimeLevel hours)
    {
        moneyText.text = hours.ToString() + " Hours";
    }

    public void ShowButton(string text = "APPLY")
    {
        button.gameObject.SetActive(true);
        errorText.gameObject.SetActive(true);
        buttonText.text = text;
    }

    public void ShowErrorText(string text, Color color)
    {
        errorText.gameObject.SetActive(true);
        button.gameObject.SetActive(false);
        errorText.text = text;
        errorText.color = color;
    }
}
