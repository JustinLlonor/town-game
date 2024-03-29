using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class AnnouncementManager : MonoBehaviour
{
    public string testAnnouncement;
    public float panelSpace = 10f;
    public GameObject announcementPrefab;
    [Header("Animation")]
    public float pushDown = 80f;
    public float pushDownSpeed = 2f;
    public AnimationCurve pushDownCurve;
    public float fadeSpeed = 2f;
    List<IEnumerator> pdNumerators = new List<IEnumerator>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Announce(testAnnouncement);
        }
    }

    // textmeshpro text preferred width updates when text is updated

    [PunRPC]
    public void Announce(string text, float lifespan = 3f)
    {
        GameObject announcement = Instantiate(announcementPrefab, transform);
        announcement.transform.localPosition = new Vector3(0f, pushDown);
        TextMeshProUGUI uGui = announcement.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        uGui.text = text;
        RectTransform rt = (RectTransform)announcement.transform;
        rt.sizeDelta = new Vector2(uGui.preferredWidth + panelSpace, rt.sizeDelta.y);
        // my brain is fried from working at a coco brooks and i have no idea what this code does but im autopiloting and its working 
        foreach (IEnumerator cor in pdNumerators) StopCoroutine(cor);
        pdNumerators.Clear();
        // Pushes down all announcements in children
        foreach (Transform t in transform)
        {
            IEnumerator pdnum = PushDown(t);
            pdNumerators.Add(pdnum);
            StartCoroutine(pdnum);
        }
        // Fades the announcement in, start a coroutine for fade out as well
        StartCoroutine(Fade(announcement, 0f, 0.77254902f, 0f));
        StartCoroutine(Fade(announcement, 0.77254902f, 0f, lifespan, true));
    }

    // Fades the announcement
    IEnumerator Fade(GameObject announcement, float from, float to, float delay, bool destroy = false)
    {
        yield return new WaitForSeconds(delay);
        float alpha = from;
        float timer = 0f;
        Image panel = announcement.GetComponent<Image>();
        TextMeshProUGUI text = announcement.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        while (alpha != to)
        {
            yield return null;
            // Sets the alpha var
            alpha = Mathf.Lerp(alpha, to, timer);

            // Sets colors
            panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, alpha);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);

            // Increases timer
            timer += Time.deltaTime * fadeSpeed;
        }
        if (destroy) Destroy(announcement);
    }

    // Pushes the announcement down
    IEnumerator PushDown(Transform announcement)
    {
        float newY = 0f - (announcement.parent.childCount - announcement.GetSiblingIndex() - 1) * pushDown;
        float ogY = announcement.localPosition.y;
        float timer = 0f;
        while (announcement.localPosition.y != newY)
        {
            yield return null;
            if (announcement == null) break;
            float eval = pushDownCurve.Evaluate(timer);
            float currentY = Mathf.Lerp(ogY, newY, eval);
            announcement.localPosition = new Vector3(announcement.localPosition.x, currentY);

            timer += Time.deltaTime * pushDownSpeed;
        }
    }
}
