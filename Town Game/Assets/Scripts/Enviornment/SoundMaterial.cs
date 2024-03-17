using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundMaterial : MonoBehaviour
{
    public Texture2D texture;
    public SMatIndex soundIndex;
    public Texture2D hitTexture;
    
    /// <summary>
    /// Gets the sound material returned as a string. Requires the textureCoord of the hit
    /// </summary>
    public string GetSMat(Vector2 pixelUV)
    {
        // Adjusts uv to the texture
        pixelUV.x *= texture.width;
        pixelUV.y *= texture.height;

        Color color = texture.GetPixel(Mathf.FloorToInt(pixelUV.x), Mathf.FloorToInt(pixelUV.y));

        float closeness = 0f;
        SMatIndex.SoundColor closest = null;
        foreach (SMatIndex.SoundColor c in soundIndex.soundMaterials)
        {
            float newCloseness = 1f;
            newCloseness -= Mathf.Abs(c.color.r - color.r) / 3f;
            newCloseness -= Mathf.Abs(c.color.g - color.g) / 3f;
            newCloseness -= Mathf.Abs(c.color.b - color.b) / 3f;
            if (newCloseness > closeness)
            {
                closeness = newCloseness;
                closest = c;
            }
        }

        if (closest == null) return null;
        return closest.materialChar;
    }
}
