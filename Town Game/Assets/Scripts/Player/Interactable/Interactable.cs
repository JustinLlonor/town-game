using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool doOutline = true;
    public Hover[] hovers;

    [System.Serializable]
    public struct Hover
    {
        public string lore;
        public KeyCode key;
        public string function;
        public MonoBehaviour interactScript;
    }
}
