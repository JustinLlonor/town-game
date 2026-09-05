using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarUI : MonoBehaviour
{
    public GameObject barTick;
    public RectTransform fill;
    public RectTransform plusIndicator;
    public RectTransform minusIndicator;
    public Transform barGlass;
    public List<MaskableGraphic> graphics;
    [Header("Animation Settings")]
    public AnimationCurve statChangeCurve;
    public float statChangeDelay;
    public float statChangeSpeed = 5f;
    [Header("Stat Reveal Settings")]
    public float showDuration = 3f;
    public float showSpeed = 8f;
    public float hideSpeed = 1f;
    /// <summary>
    /// If this stat is revealing or not
    /// </summary>
    public bool statRevealing = false;
    private float maxFillSize;
    private int maxValue;
    float currentAlpha = 1f;
    IEnumerator currentChangeAnimation = null;
    IEnumerator currentStatVisAnimation = null;

    private void OnDisable()
    {
        SetAlpha(0f);
        currentChangeAnimation = null;
        currentStatVisAnimation = null;
        statRevealing = false;
        plusIndicator.gameObject.SetActive(false);
        minusIndicator.gameObject.SetActive(false);
        StopAllCoroutines();
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
            graphics.Add(tickObject.GetComponent<RawImage>());
        }
    }

    public void SetValue(int newValue)
    {
        plusIndicator.gameObject.SetActive(false);
        minusIndicator.gameObject.SetActive(false);
        float currentValue = (fill.sizeDelta.x / maxFillSize) * (float)maxValue;
        if (currentChangeAnimation != null)
        {
            StopCoroutine(currentChangeAnimation);
            currentChangeAnimation = null;
        }
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
        currentChangeAnimation = ChangeAnimation(newValue);
        StartCoroutine(currentChangeAnimation);
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

    /// <summary>
    /// Sets the transparency of this stat bar.
    /// </summary>
    /// <param name="alpha"></param>
    public void SetAlpha(float alpha)
    {
        currentAlpha = alpha;
        foreach (MaskableGraphic graphic in graphics)
        {
            graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, alpha);
        }
    }

    /// <summary>
    /// When this is called, temporarily reveals this bar before disappearing.
    /// </summary>
    public void RevealStat()
    {
        if (currentAlpha == 1f)
        {
            statRevealing = false;
            return;
        }
        if (currentStatVisAnimation != null)
        {
            StopCoroutine(currentStatVisAnimation);
            currentStatVisAnimation = null;
        }
        statRevealing = true;
        currentStatVisAnimation = StatRevealAnimation();
        StartCoroutine(currentStatVisAnimation);
    }

    IEnumerator StatRevealAnimation()
    {
        float progress = 0f;
        float originalAlpha = currentAlpha;
        float finalAlpha = 1f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * showSpeed;
            float eval = Mathf.Lerp(originalAlpha, finalAlpha, progress);
            SetAlpha(eval);
            yield return null;
        }
        SetAlpha(1f);
        yield return new WaitForSeconds(showDuration);
        currentStatVisAnimation = StatHideAnimation();
        StartCoroutine(currentStatVisAnimation);
    }

    IEnumerator StatHideAnimation()
    {
        float progress = 0f;
        float originalAlpha = currentAlpha;
        float finalAlpha = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * hideSpeed;
            float eval = Mathf.Lerp(originalAlpha, finalAlpha, progress);
            SetAlpha(eval);
            yield return null;
        }
        SetAlpha(0f);
        statRevealing = false;
    }
}