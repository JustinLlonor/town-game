using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffecterHolder : MonoBehaviour
{
    public Dictionary<string, GameObject> affecters = new Dictionary<string, GameObject>();
    public Color[] statColors = new Color[] { };
    public GameObject affecterPrefab;
    public float spacing = 55f;
    public float closedHeight = 72f;
    public float openedHeight = 148f;
    PlayerStats trackedStats;

    private void Awake()
    {
        FindObjectOfType<PlayerManager>().OnInstantiatePlayer += GetReferences;
    }

    void GetReferences(GameObject player)
    {
        trackedStats = player.GetComponent<PlayerStats>();
        trackedStats.OnAddAffecter += AddAffecter;
        trackedStats.OnRemoveAffecter += RemoveAffecter;
    }

    void AddAffecter(StatAffecter affecter)
    {
        if (!affecter.display) return;
        GameObject go = Instantiate(affecterPrefab, transform);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -spacing * (affecters.Count));
        affecters.Add(affecter.name, go);
        PhysAffecter pa = go.GetComponent<PhysAffecter>();
        pa.SetHeight(closedHeight);
        pa.openedHeight = openedHeight;
        pa.closedHeight = closedHeight;
        pa.SetTitle(affecter.name);
        pa.SetDescription(affecter.description);
        pa.SetColor(statColors[(int)affecter.stat]);
        pa.SetChange(affecter.changeRate / 100f); // Change this to adapt to each max stat if max stat changes are added in the future
        pa.timeAffected = !affecter.isInfinite;
        if (!affecter.isInfinite)
        {
            pa.StartTimer(affecter.timeLeft);
        }
        OrganizeAffecters();
    }

    void RemoveAffecter(StatAffecter affecter)
    {
        Debug.Log("removing...");
        Destroy(affecters[affecter.name]);
        affecters.Remove(affecter.name);
        OrganizeAffecters();
    }

    void OrganizeAffecters()
    {
        StopAllCoroutines();

        int i = 0;
        foreach (KeyValuePair<string, GameObject> pair in affecters)
        {
            StartCoroutine(MoveToPosition(i, pair.Value));
            Debug.Log(i);

            i++;
        }
    }

    IEnumerator MoveToPosition(int i, GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        float desiredY = i * -spacing;
        float ogY = rt.anchoredPosition.y;
        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime * 4f;
            float newY = Mathf.SmoothStep(ogY, desiredY, time);
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, newY);
            yield return null;
        }
        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, desiredY);
    }
}
