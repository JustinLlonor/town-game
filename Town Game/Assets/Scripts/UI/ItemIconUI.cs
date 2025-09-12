using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemIconUI : MonoBehaviour
{
    public RawImage uiIcon;
    public TextMeshProUGUI iconText;
    public RawImage[] arrowImages;
    public Texture2D[] arrowTextures;
    public Color increaseColor;
    public Color decreaseColor;

    public void SetItemIcon(Item item)
    {
        uiIcon.texture = item.icon;
        if (iconText != null)
        {
            iconText.text = item.name;
        }
    }

    public void SetAttributeIcon(ItemAttribute attribute)
    {
        uiIcon.texture = AttributeManager.i.GetAttributeTexture(attribute);
        if (iconText != null)
        {
            iconText.text = attribute.ToReadable();
        }
    }

    public void SetArrowImages(int arrowDelta)
    {
        if (arrowDelta > 3 || arrowDelta < 3) return;
        // If there is no arrow delta, then hide the images
        if (arrowDelta == 0)
        {
            foreach (RawImage image in arrowImages) image.enabled = false;
            return;
        }
        foreach (RawImage image in arrowImages) image.enabled = true;
        // Sets arrow image based on the delta amount
        int absoluteDelta = Mathf.Abs(arrowDelta);
        Texture2D arrowTexture = arrowTextures[absoluteDelta-1];
        foreach (RawImage image in arrowImages) image.texture = arrowTexture;
        // Sets color and point direction. Negative means decrease color and pointing down,
        // positive means increase color and pointing up
        if (arrowDelta > 0)
        {
            foreach (RawImage image in arrowImages)
            {
                image.transform.localScale = Vector3.one;
                image.color = new Color(increaseColor.r, increaseColor.g, increaseColor.b, image.color.a);
            }
            return;
        }
        foreach (RawImage image in arrowImages)
        {
            image.transform.localScale = new Vector3(1f, -1f, 1f);
            image.color = new Color(decreaseColor.r, decreaseColor.g, decreaseColor.b, image.color.a);
        }
    }
}
