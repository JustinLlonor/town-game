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
    [Networked, Capacity(5)] public NetworkArray<int> nAttires { get; } = MakeInitializer(new int[5]); // Change this number as you add more clothes
    public RandomizedClothing[] randomizedClothing;
    ObjectManager om;
    ChangeDetector changeDetector;
    FirstPerson fps;
    //PhotonView view;

    private void Start()
    {
        RenderAllClothing();
    }

    public override void Spawned()
    {
        fps = FindFirstObjectByType<FirstPerson>();
        om = FindFirstObjectByType<ObjectManager>();
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
;        if (isCorpse) return;
        if (!HasStateAuthority) return;
        RandomizeGender();
        RandomizeClothing();
    }

    public override void Render()
    {
        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(nAttires): // goofy ahh code
                    RenderAllClothing();
                    break;
            }
        }
    }

    public void Init()
    {

    }

    void RenderAllClothing()
    {
        foreach (int clothingIndex in nAttires)
        {
            if (clothingIndex == -1) continue;
            if (!HasStateAuthority) SetClothing(om.clothings[clothingIndex]);
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
}
