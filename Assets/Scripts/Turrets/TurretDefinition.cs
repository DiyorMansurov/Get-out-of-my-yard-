using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Turret/Definition")]
public class TurretDefinition : ScriptableObject
{
    public TurretTierData[] tiers;
}
