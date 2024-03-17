using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using JetBrains.Annotations;
using Unity.VisualScripting;

public class PlayerClothing : MonoBehaviour
{
    public bool isMale;
    public bool isCorpse = false;
    public Attire[] attires;
    public RandomizedClothing[] randomizedClothing;
    ObjectManager om;
    PhotonView view;

    private void Awake()
    {
        om = FindObjectOfType<ObjectManager>();
        view = gameObject.GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (isCorpse) return;
        RandomizeGender();
        RandomizeClothing();
    }

    [PunRPC]
    public void SetClothing(string clothingName, bool male)
    {
        isMale = male;
        Clothing clothing = om.clothingSearch[clothingName];
        int i = 0;

        foreach (Attire attire in attires)
        {
            if (attire.bodyPart == clothing.bodyPart)
            {
                break;
            }
            i++;
        }
        attires[i].clothing = clothing;

        RenderClothing(attires[i]);
    }

    [PunRPC]
    public void SetSex(bool male)
    {
        isMale = male;
    }

    public void RandomizeGender()
    {
        bool male = false;
        int randomGender = Random.Range((int)0, (int)2);
        if (randomGender == 0) male = true;
        ExitGames.Client.Photon.Hashtable playerProperties = view.Owner.CustomProperties;
        playerProperties["isMale"] = male;
        view.Owner.SetCustomProperties(playerProperties);
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
            SetClothing(selectedClothing.name, isMale);
            view.RPC("SetClothing", RpcTarget.OthersBuffered, selectedClothing.name, isMale);
        }
    }

    void RenderClothing(Attire attire)
    {
        if (attire.clothing != null) attire.renderer.material.mainTexture = attire.clothing.texture;
        if (isMale)
        {
            if (attire.clothing.maleArmModel != null) FindObjectOfType<FirstPerson>().ChangeArmMesh(attire.clothing.maleArmModel);
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
            if (attire.clothing.femaleArmModel != null) FindObjectOfType<FirstPerson>().ChangeArmMesh(attire.clothing.femaleArmModel);
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
