using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Clothing", menuName = "Clothing")]
public class Clothing : ScriptableObject
{
    public BodyPart bodyPart;
    public Mesh maleModel;
    public Mesh maleArmModel;
    public Mesh femaleModel;
    public Mesh femaleArmModel;
    public Texture texture;

    public enum BodyPart
    {
        Torso = 0,
        Legs = 1,
        Hands = 2,
        Head = 3,
        Hat = 4,
        Mask = 5
    }
}
