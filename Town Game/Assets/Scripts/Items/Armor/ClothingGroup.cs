using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ClothingGroup
{
    None = -1,
    Head = 0,
    Torso = 1,
    Legs = 2
}

public static class ClothingGroupExtensions
{
    public static Clothing.BodyPart[] GetBodyParts(this ClothingGroup group)
    {
        switch (group)
        {
            case ClothingGroup.Head:
                return new Clothing.BodyPart[] { Clothing.BodyPart.Head, Clothing.BodyPart.Hat, Clothing.BodyPart.Mask };
            case ClothingGroup.Torso:
                return new Clothing.BodyPart[] { Clothing.BodyPart.Torso };
            case ClothingGroup.Legs:
                return new Clothing.BodyPart[] { Clothing.BodyPart.Legs };
            default:
                return new Clothing.BodyPart[0];
        }
    }
}
