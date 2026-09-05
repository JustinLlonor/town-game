using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Equipment : NetworkBehaviour
{
    [Tooltip("The power consumption of this piece of equipment in kWh. " +
        "If this is less than or equal to zero, this equipment will not require an outlet.")]
    [Networked] public float energyConsumption { get; set; }
    [Networked] public NetworkBool powered { get; set; }
}
