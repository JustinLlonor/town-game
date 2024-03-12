using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerClothing : MonoBehaviour
{
    public bool isMale;
    public Attire[] attires;
    ObjectManager om;

    private void Awake()
    {
        om = FindObjectOfType<ObjectManager>();
    }

    [PunRPC]
    public void SetClothing(string clothingName)
    {
        Clothing clothing = om.clothingSearch[clothingName];
        Attire foundAttire = new Attire();

        foreach (Attire attire in attires)
        {
            if (attire.bodyPart == clothing.bodyPart)
            {
                foundAttire = attire;
                break;
            }
        }
        foundAttire.clothing = clothing;

        RenderClothing(foundAttire);
    }

    void RenderClothing(Attire attire)
    {
        attire.renderer.material.mainTexture = attire.clothing.texture;
        if (isMale)
        {
            attire.meshFilter.mesh = attire.clothing.maleModel;
            return;
        }
        attire.meshFilter.mesh = attire.clothing.femaleModel;
    }

    // Data structure for a body part that can wear clothing
    [System.Serializable]
    public struct Attire
    {
        public Clothing.BodyPart bodyPart;
        public Renderer renderer;
        public MeshFilter meshFilter;
        public Clothing clothing;
    }
}
