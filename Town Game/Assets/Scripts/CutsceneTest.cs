using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneTest : MonoBehaviour
{
    public Transform second;

    private void Awake()
    {
        FindAnyObjectByType<CutsceneManager>().onSequenceStart += GetScenes;
    }

    private List<SeqScene> GetScenes(string sequence)
    {
        SeqScene newScene = new SeqScene(transform.position, transform.rotation, 0);
        SeqScene secondScene = new SeqScene(second.position, second.rotation, 0);
        return new List<SeqScene>() { newScene, secondScene };
    }
}
