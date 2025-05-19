using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconTest : MonoBehaviour
{
    public Texture2D tex;
    MinimapManager mm;

    private void OnEnable()
    {
        mm = FindAnyObjectByType<MinimapManager>();
        mm.AddIcon("Barry", tex, transform.position, 0f, false);
    }

    private void Update()
    {
        mm.SetIconPosition("Barry", transform.position);
    }

    private void OnDisable()
    {
        mm.RemoveIcon("Barry");
    }
}
