using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundInstance : MonoBehaviour
{
    private AudioSource source;

    private void Awake()
    {
        source = gameObject.GetComponent<AudioSource>();
    }

    private void Start()
    {
        source.enabled = true;
        StartCoroutine(WaitForDestroy());
    }

    IEnumerator WaitForDestroy()
    {
        yield return new WaitForSeconds(source.clip.length + 1f);
        Destroy(gameObject);
    }
}
