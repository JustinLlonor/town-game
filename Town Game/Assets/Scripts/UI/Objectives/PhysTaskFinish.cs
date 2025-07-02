using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WebSocketSharp;

public class PhysTaskFinish : MonoBehaviour
{
    public Color rewardColor;
    public Color mixedRewardColor;
    public Color punishmentColor;
    public TextMeshProUGUI taskText;
    public TextMeshProUGUI rewardText;
    [Header("Animation")]
    public AnimationCurve closeCurve;
    public float closeSpeed = 1f;
    public float finishDuration = 5f;

    private void OnEnable()
    {
        StartCoroutine(CloseAnimation());
    }

    /// <summary>
    /// To be called when this taskfinish thingie does not need to contain info
    /// </summary>
    public void SetNull()
    {
        taskText.enabled = false;
        rewardText.enabled = false;
    }

    public void SetTaskText(string rewardText, string strikeText)
    {
        string outputText = "";
        bool rewardEmpty = rewardText.IsNullOrEmpty();
        bool strikeEmpty = strikeText.IsNullOrEmpty();
        if (!rewardEmpty && !strikeEmpty)
        {
            outputText = rewardText + " " + strikeText;
        }
        else if (rewardEmpty && !strikeEmpty)
        {
            outputText = strikeText;
        }
        else
        {
            outputText = rewardText;
        }
        taskText.text = outputText;
    }

    public void SetRewardText(float currencyReward, int strikes)
    {
        // Set currency and strike text
        string currencyString = "";
        if (currencyReward > 0)
        {
            currencyString = (currencyReward).ToString("#.##");
            currencyString = "+" + currencyString;
        }
        string strikeString = "";
        if (strikes == 1)
        {
            strikeString = strikes + " strike receieved";
        }
        else if (strikes > 1)
        {
            strikeString = strikes + " strikes received";
        }
        // Set the corresponding text color and text combo
        string rewardString = "";
        bool currencyRewarded = !currencyString.IsNullOrEmpty(); // if not null or empty, currency awarded
        bool strikeReceived = !strikeString.IsNullOrEmpty();
        if (currencyRewarded && strikeReceived)
        {
            SetRewardTextColor(mixedRewardColor);
            rewardString = currencyString + ", " + strikeString;
        }
        else if (currencyRewarded && !strikeReceived)
        {
            SetRewardTextColor(rewardColor);
            rewardString = currencyString;
        }
        else if (!currencyRewarded && strikeReceived)
        {
            SetRewardTextColor(punishmentColor);
            rewardString = strikeString;
        }
        rewardText.text = rewardString;
    }

    private void SetRewardTextColor(Color color)
    {
        rewardText.color = color;
    }

    IEnumerator CloseAnimation()
    {
        yield return new WaitForSeconds(finishDuration);
        RectTransform rt = (RectTransform)transform;
        float progress = 1f;
        float startHeight = rt.sizeDelta.y;
        while (progress > 0f)
        {
            yield return null;
            progress -= Time.deltaTime * closeSpeed;
            float newHeight = startHeight * closeCurve.Evaluate(progress);
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, newHeight);
        }
        Destroy(gameObject);
    }
}
