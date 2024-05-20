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
    public AnimationCurve moveCurve;
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
        GameObject go = Instantiate(affecterPrefab, transform);
        PhysAffecter pa = go.GetComponent<PhysAffecter>();
        pa.SetHeight(closedHeight);
        pa.openedHeight = openedHeight;
        pa.closedHeight = closedHeight;
        pa.SetTitle(affecter.name);
        pa.SetDescription(affecter.description);
        pa.SetColor(statColors[(int)affecter.stat]);
        pa.SetChange(affecter.changeRate / 100f); // Change this to adapt to each max stat if max stat changes are added in the future
        OrganizeAffecters();
    }

    void RemoveAffecter(StatAffecter affecter)
    {
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

            i++;
        }
    }

    IEnumerator MoveToPosition(int i, GameObject go)
    {
        float desiredY = i * 
        yield return null;
    }
}
