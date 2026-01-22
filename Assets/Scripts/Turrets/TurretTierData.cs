using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Turret/Tier Data")]
public class TurretTierData : ScriptableObject
{
    public float range;
    public float fireRate;
    public GameObject bulletPrefab;
    public int damage;
    public int cost;
}
