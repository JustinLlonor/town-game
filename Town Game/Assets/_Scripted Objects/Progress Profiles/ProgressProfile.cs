using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Progress Profile", menuName = "Progress Profile")]
public class ProgressProfile : ScriptableObject
{
    [Tooltip("The default rate of progress when a player clicks on the object. How much progress is cleared in a second, with the max progress being 100")]
    public float defaultRate = 10f;
    public string progressAddAction;
    public float defaultRateSecondary = 0f;
    public string progressSubtractAction;
    [Tooltip("The rate of progress when this handler is untouched")]
    public float untouchedRate = 0f;
    [Tooltip("Item attributes and their corresponding rate modifications")]
    public ItemAttributeRate[] attributeRates = new ItemAttributeRate[0];
    [Tooltip("Items and their corresponding rate modifications")]
    public ItemRate[] itemRates = new ItemRate[0];
    [Tooltip("The x axis is the input rate (-100)-(100) scaled down to (-1.0)-(1.0), the y axis is the ")]
    public AnimationCurve rateCurve;

    public int GetDelta(float rate)
    {
        return (int)(Mathf.Sign(rate) * Mathf.CeilToInt(Mathf.Abs(rateCurve.Evaluate(rate))));
    }
}
