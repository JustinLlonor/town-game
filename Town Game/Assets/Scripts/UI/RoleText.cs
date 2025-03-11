using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RoleText : MonoBehaviour
{
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI cursorText;

    public void SetFontSize(float size)
    {
        roleText.fontSize = size;
        cursorText.fontSize = size;
    }

    public void StartTextRoll(string text, Color color, float duration, float wait)
    {
        StartCoroutine(RollText(text, color, duration, wait));
    }

    public void StartCursorBlink(float duration, int amount, float wait)
    {
        StartCoroutine(BlinkCursor(duration, amount, wait));
    }

    IEnumerator RollText(string text, Color color, float rollTextDuration, float wait)
    {
        yield return new WaitForSeconds(wait);
        roleText.color = color;
        string newText = "";
        for (int i = 0; i < text.Length; i++)
        {
            newText += text[i];
            roleText.text = newText;
            yield return new WaitForSeconds(rollTextDuration / text.Length);
        }
        yield return null;
    }

    IEnumerator BlinkCursor(float seconds, int amount, float wait)
    {
        yield return new WaitForSeconds(wait);
        float timer = 0f;
        float blinkTimer = 0f;
        bool cEnabled = true;
        while (timer < seconds)
        {
            yield return null;
            timer += Time.deltaTime;
            blinkTimer += Time.deltaTime;
            if (blinkTimer > seconds/amount)
            {
                blinkTimer = 0f;
                if (cEnabled)
                {
                    cEnabled = false;
                } else
                {
                    cEnabled = true;
                }
                cursorText.enabled = cEnabled;
            }
        }
        cursorText.enabled = true;
    }
}
