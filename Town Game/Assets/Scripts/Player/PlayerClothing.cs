using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class PlayerClothing : MonoBehaviour
{
    public bool isMale;
    public Attire[] attires;
    public RandomizedClothing[] randomizedClothing;
    ObjectManager om;
    PhotonView view;

    private void Start()
    {
        om = FindObjectOfType<ObjectManager>();
        view = gameObject.GetComponent<PhotonView>();
        RandomizeGender();
        RandomizeClothing();
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

    [PunRPC]
    public void SetSex(bool male)
    {
        isMale = male;
    }

    public void RandomizeGender()
    {
        bool male = false;
        int randomGender = Random.Range(0, 2);
        if (randomGender == 0) male = true;
        SetSex(male);
        view.RPC("SetSex", RpcTarget.OthersBuffered, male);
    }

    public void RandomizeClothing()
    {
        foreach (RandomizedClothing rc in randomizedClothing)
        {
            if (rc.nullChance != 0f)
            {
                if (Random.value <= rc.nullChance) continue;
            }
            Clothing selectedClothing = rc.clothings[Random.Range(0, rc.clothings.Length)];
            SetClothing(selectedClothing.name);
            view.RPC("SetClothing", RpcTarget.OthersBuffered, selectedClothing.name);
        }
    }

    void RenderClothing(Attire attire)
    {
        if (attire.clothing != null) attire.renderer.material.mainTexture = attire.clothing.texture;
        if (isMale)
        {
            if (attire.renderer.transform.GetComponent<MeshFilter>() != null)
            {
                attire.renderer.transform.GetComponent<MeshFilter>().mesh = attire.clothing.maleModel;
                return;
            }
            ((SkinnedMeshRenderer)attire.renderer).sharedMesh = attire.clothing.maleModel;
            return;
        }
        else
        {
            if (attire.renderer.transform.GetComponent<MeshFilter>() != null)
            {
                attire.renderer.transform.GetComponent<MeshFilter>().mesh = attire.clothing.femaleModel;
                return;
            }
            ((SkinnedMeshRenderer)attire.renderer).sharedMesh = attire.clothing.femaleModel;
            return;
        }
    }

    // Data structure for a body part that can wear clothing
    [System.Serializable]
    public struct Attire
    {
        public Clothing.BodyPart bodyPart;
        public Renderer renderer;
        public Clothing clothing;
        public Clothing.BodyPart[] hiddenParts;
    }

    [System.Serializable]
    public struct RandomizedClothing
    {
        public Clothing[] clothings;
        [Range(0f, 1f)]
        public float nullChance;
    }
}
