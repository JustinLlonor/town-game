using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct SeqScene : INetworkStruct
{
    public Vector3 camPosition;
    public Quaternion camRotation;
    public int sceneIndex;
    [Networked, Capacity(8)] NetworkDictionary<int, SeqRef> references => default;
    public int id;
    public static int idCounter = 0;

    public SeqScene(Vector3 camPosition, Quaternion camRotation, int sceneIndex, Dictionary<int, SeqRef> refs)
    {
        this.camPosition = camPosition;
        this.camRotation = camRotation;
        this.sceneIndex = sceneIndex;
        id = idCounter++;
        foreach (var kvp in refs) references.Add(kvp.Key, kvp.Value);
    }

    public bool Equals(SeqScene other)
    {
        return id == other.id;
    }

    public static SeqScene None
    {
        get
        {
            SeqScene none = new SeqScene();
            none.sceneIndex = -1;
            none.id = -1;
            return none;
        }
    }
}