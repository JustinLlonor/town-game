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
        mm.AddIcon(gameObject.name, tex, transform.position, transform.eulerAngles.y);
    }

    private void Update()
    {
        mm.SetIconPosition(gameObject.name, transform.position);
        mm.SetIconRotation(gameObject.name, transform.eulerAngles.y);
    }

    private void OnDisable()
    {
        mm.RemoveIcon(gameObject.name);
    }
}
