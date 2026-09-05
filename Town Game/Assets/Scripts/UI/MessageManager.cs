using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// A class for sending client sided messages
/// </summary>
public class MessageManager : MonoBehaviour
{
    public GameObject messageObject;
    public float fadeSpeed = 2f;

    public static MessageManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SendMessage(string message, Color color, float duration = 3f)
    {
        GameObject newMessage = Instantiate(messageObject, transform);
        TextMeshProUGUI text = newMessage.GetComponent<TextMeshProUGUI>();
        newMessage.transform.SetSiblingIndex(0);
        text.text = message;
        text.color = color;
        StartCoroutine(WaitForMessage(text, duration));
    }

    IEnumerator WaitForMessage(TextMeshProUGUI message, float duration)
    {
        yield return new WaitForSeconds(duration);
        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * fadeSpeed;
            message.color = new Color(message.color.r, message.color.g, message.color.b, 1f - progress);
            yield return null;
        }
        Destroy(message.gameObject);
    }
}
