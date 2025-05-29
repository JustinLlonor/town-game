using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarUI : MonoBehaviour
{
    public GameObject barTick;
    public RectTransform fill;
    public RectTransform plusIndicator;
    public RectTransform minusIndicator;
    public Transform barGlass;
    public BarStatsUI bsUI;
    [Header("Animation Settings")]
    public AnimationCurve statChangeCurve;
    public float statChangeDelay;
    public float statChangeSpeed = 5f;
    private float maxFillSize;
    private int maxValue;
    private int destinedValue = 3;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Init(3);
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            SetValue(destinedValue - 1);
            destinedValue--;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SetValue(destinedValue + 1);
            destinedValue++;
        }
    }
    
    /// <summary>
    /// To be called when the stat is recieved.
    /// </summary>
    /// <param name="statMax"></param>
    public void Init(int statMax)
    {
        maxValue = statMax;
        maxFillSize = fill.sizeDelta.x;
        float tickSpacing = maxFillSize / (float)statMax;
        for (int i = 1; i < statMax; i++)
        {
            GameObject tickObject = Instantiate(barTick, barGlass);
            tickObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(tickSpacing * i, 0f);
        }
    }

    public void SetValue(int newValue)
    {
        plusIndicator.gameObject.SetActive(false);
        minusIndicator.gameObject.SetActive(false);
        float currentValue = (fill.sizeDelta.x / maxFillSize) * (float)maxValue;
        StopAllCoroutines();
        if (newValue > currentValue)
        {
            StartAddAnimation(newValue);
        }
        else if (newValue < currentValue)
        {
            StartRemovalAnimation(newValue);
        }
        else
        {
            return;
        }
        StartCoroutine(ChangeAnimation(newValue));
    }

    /// <summary>
    /// Sets add fill indicator
    /// </summary>
    private void StartAddAnimation(int newValue)
    {
        plusIndicator.gameObject.SetActive(true);

        float sizeDelta = ((float)(newValue) / (float)maxValue) * maxFillSize;
        plusIndicator.anchoredPosition = new Vector2(0f, 0f);
        plusIndicator.sizeDelta = new Vector2(sizeDelta, plusIndicator.sizeDelta.y);
    }

    /// <summary>
    /// Sets remove fill indicator
    /// </summary>
    private void StartRemovalAnimation(int newValue)
    {
        minusIndicator.gameObject.SetActive(true);

        float anchoredX = ((float)newValue / (float)maxValue) * maxFillSize;
        float sizeDelta = (float)maxValue * maxFillSize;
        minusIndicator.anchoredPosition = new Vector2(anchoredX, 0f);
        minusIndicator.sizeDelta = new Vector2(sizeDelta, plusIndicator.sizeDelta.y);
    }

    IEnumerator ChangeAnimation(int finalValue)
    {
        yield return new WaitForSeconds(statChangeDelay);
        float progress = 0f;
        float originalSize = fill.sizeDelta.x;
        float finalSize = ((float)finalValue / (float)maxValue) * maxFillSize;
        while (progress < 1f)
        {
            progress += Time.deltaTime * statChangeSpeed;
            float eval = Mathf.Lerp(originalSize, finalSize, statChangeCurve.Evaluate(progress));
            fill.sizeDelta = new Vector2(eval, fill.sizeDelta.y);
            yield return null;
        }
        fill.sizeDelta = new Vector2(finalSize, fill.sizeDelta.y);
        plusIndicator.gameObject.SetActive(false);
        minusIndicator.gameObject.SetActive(false);
    }
}
