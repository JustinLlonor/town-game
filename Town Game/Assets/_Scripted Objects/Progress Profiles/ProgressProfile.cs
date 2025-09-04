using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Progress Profile", menuName = "Progress Profile")]
public class ProgressProfile : ScriptableObject
{
    [Tooltip("The default rate of progress when a player clicks on the object. How much progress is cleared in a second, with the max progress being 100")]
    public float defaultRate = 10f;
    public float defaultRateSecondary = 0f;
    [Tooltip("The rate of progress when this handler is untouched")]
    public float untouchedRate = 0f;
    [Tooltip("Item attributes and their corresponding rate modifications")]
    public ItemAttributeRate[] attributeRates = new ItemAttributeRate[0];
    [Tooltip("Items and their corresponding rate modifications")]
    public ItemRate[] itemRates = new ItemRate[0];
}
