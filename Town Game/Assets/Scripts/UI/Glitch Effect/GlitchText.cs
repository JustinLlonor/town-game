using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GlitchText : MonoBehaviour
{
    [Header("Font")]
    public TMP_FontAsset[] fontCycle;
    public float cycleFrequency;
    [HideInInspector] public TextMeshProUGUI text;
    float cycleTimer;
    int cycleIndex;
    [Header("Color")]
    public bool doColorCycle = false;
    public Color color1;
    public Color color2;
    public float colorSpeed = 1f;
    float perlinTimer;

    private void OnEnable()
    {
        cycleIndex = Random.Range(0, fontCycle.Length);
        perlinTimer = Random.Range(0f, 100f);
    }

    private void Update()
    {
        Cycle();
        if (!doColorCycle) return;
        ColorCycle();
    }

    void Cycle()
    {
        cycleTimer += Time.deltaTime * cycleFrequency;
        if (cycleTimer < 1f) return;
        cycleTimer = 0f;
        text.font = fontCycle[cycleIndex];
        cycleIndex++;
        if (cycleIndex >= fontCycle.Length) cycleIndex = 0;
    }

    void ColorCycle()
    {
        perlinTimer += Time.deltaTime * colorSpeed;
        float perlin = Mathf.PerlinNoise1D(perlinTimer);
        text.color = Color.Lerp(color1, color2, perlin);
    }
}
