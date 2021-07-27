using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactLeak : MonoBehaviour
{
    public int leakAmount;

    public enum Type
    {
        AutoCannon = 1,
        Missile = 2,
        RailGun = 3,
        JumpFuel = 4
    }

    public Type SelectRandom()
    {
        return (Type)Random.Range(1, 4);
    }
}
