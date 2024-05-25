using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon Type", menuName = "Weapon")]
public class weapon : ScriptableObject
{
    public enum firetype
    {
        rifle = 0,
        shotgun,
        grenade
    }
    public enum reloadtype
    {
        magazine = 0,
        roundsreload
    }
    public enum aimtype
    {
        general = 0,
        precise
    }

    public firetype fire;
    public reloadtype reload;
    public aimtype aim;

    public float shotcount;
    public float damage;
    public int AP;
    public int STP;

    public int magazine;
    public int sparemags;
    public float reloadtime;

    public bool auto;
    public float firedelay;
    public float spread;
}

