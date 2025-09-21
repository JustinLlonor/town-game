using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Fusion;

public class PlayerClothing : NetworkBehaviour
{
    [Networked] public bool isMale { get; set; }
    public bool isCorpse = false;
    [Tooltip("Change the capacity in code when adding/removing attires")]
    public Attire[] attires;
    [Networked, Capacity(6)] public NetworkArray<int> nAttires { get; } = MakeInitializer(new int[] { -1, -1, -1, -1, -1}); // Change this number as you add more clothes
    public RandomizedClothing[] randomizedClothing;
    [Networked] public int skinTone { get; set; } = -1;
    public Material[] skinTones;
    ObjectManager om;
    ChangeDetector changeDetector;
    FirstPerson fps;
    Material updatedMat = null;
    //PhotonView view;

    private void Start()
    {
        //RenderAllClothing();
        if (skinTone != -1)
        {
            updatedMat = skinTones[skinTone];
            SetAllAttireMaterials(updatedMat);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            RenderAllClothing();
        }
    }

    public override void Spawned()
    {
        fps = FindFirstObjectByType<FirstPerson>();
        om = FindFirstObjectByType<ObjectManager>();
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (isCorpse) return;
        if (!HasStateAuthority) return;
        RandomizeGender();
        RandomizeClothing();
        RandomizeSkinColor();
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(nAttires):
                    RenderAllClothing();
                    break;
                case nameof(skinTone):
                    updatedMat = skinTones[skinTone];
                    SetAllAttireMaterials(updatedMat);
                    if (HasInputAuthority) fps.ChangeArmMaterials(skinTones[skinTone]);
                    break;
            }
        }
    }

    void RenderAllClothing()
    {
        foreach (int clothingIndex in nAttires)
        {
            if (clothingIndex == -1) continue;
            //SetClothing(om.clothings[clothingIndex]);
            foreach (Attire attire in attires)
            {
                RenderClothing(attire);
            }
        }
    }

    public void SetClothing(string clothingName)
    {
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
        //attires[i].clothing = clothing;
        /**
        Attire indexedAttire = attires[i];
        Clothing.BodyPart[] hiddenParts = new Clothing.BodyPart[indexedAttire.hiddenParts.Length];
        for (int n = 0; n < indexedAttire.hiddenParts.Length; n++)
        {
            hiddenParts[n] = indexedAttire.hiddenParts[n];
        }
        **/

        attires[i].clothing = clothing;
        if (HasStateAuthority) nAttires.Set(i, Array.IndexOf(om.clothings, clothing)); // Sets the networked array clotyhing to the name of the clothing
    }

    public void SetClothing(Clothing clothing)
    {
        int i = 0;

        foreach (Attire attire in attires)
        {
            if (attire.bodyPart == clothing.bodyPart)
            {
                break;
            }
            i++;
        }
        // Sets attire to the clothing
        attires[i].clothing = clothing;
    }

    public void SetSex(bool male)
    {
        isMale = male;
    }
    
    public void RandomizeGender()
    {
        int randomGender = UnityEngine.Random.Range((int)0, (int)2);
        bool male = (randomGender == 0);
        SetSex(male);
    }

    public void RandomizeClothing()
    {
        foreach (RandomizedClothing rc in randomizedClothing)
        {
            if (rc.nullChance != 0f)
            {
                if (UnityEngine.Random.value <= rc.nullChance) continue;
            }
            Clothing selectedClothing = rc.clothings[UnityEngine.Random.Range(0, rc.clothings.Length)];
            SetClothing(selectedClothing.name);
        }
    }

    public void RandomizeSkinColor()
    {
        int randomColor = UnityEngine.Random.Range((int)0, (int)skinTones.Length);
        SetSkinColor(randomColor);
    }

    void RenderClothing(Attire attire)
    {
        if (attire.clothing == null) return;
        attire.renderer.material.mainTexture = attire.clothing.texture;
        if (isMale)
        {
            if (attire.clothing.maleArmModel != null && HasInputAuthority) fps.ChangeArmMesh(attire.clothing.maleArmModel);
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
            if (attire.clothing.femaleArmModel != null && HasInputAuthority) fps.ChangeArmMesh(attire.clothing.femaleArmModel);
            if (attire.renderer.transform.GetComponent<MeshFilter>() != null)
            {
                attire.renderer.transform.GetComponent<MeshFilter>().mesh = attire.clothing.femaleModel;
                return;
            }
            ((SkinnedMeshRenderer)attire.renderer).sharedMesh = attire.clothing.femaleModel;
            return;
        }
    }

    public void SetSkinColor(int skinColorIndex)
    {
        skinTone = skinColorIndex;
    }

    public void SetAllAttireMaterials(Material material)
    {
        foreach (Attire attire in attires)
        {
            attire.renderer.material = material;
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

        public Attire(Clothing.BodyPart bodyPart, Renderer renderer, Clothing clothing, Clothing.BodyPart[] hiddenParts)
        {
            this.bodyPart = bodyPart;
            this.renderer = renderer;
            this.clothing = clothing;
            this.hiddenParts = hiddenParts;
        }
    }

    [System.Serializable]
    public struct RandomizedClothing
    {
        public Clothing[] clothings;
        [Range(0f, 1f)]
        public float nullChance;
    }

    public void SetClothingLayer(int layer)
    {
        foreach (Attire attire in attires)
        {
            attire.renderer.gameObject.layer = layer;
        }
    }
}
