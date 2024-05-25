using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Type", menuName = "Type")]
public class Type : ScriptableObject
{
    enum ratings
    {
        chaff,
        elite,
        boss,
    }

    public int rating;
    public new string name; //enemy name

    public float basehealth;
    public float maxextrahealth;

    public float armor;

    public float speed;

    public float damage;

    public float radius;
    public float turnradius;
}
