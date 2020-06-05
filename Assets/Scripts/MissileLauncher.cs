using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileLauncher : Weapon
{
    override public string Name
    {
        get
        {
            weaponName = "Missile Launcher";
            return weaponName;
        }
    }
    override public int Damage
    {
        get
        {
            weaponDamage = 0;
            return weaponDamage;
        }
    }

    override protected IEnumerator AnimationCoroutine(GridBlock target)
    {
        Debug.Log("Missile Launcher currently has no animation.");
        yield return new WaitForSeconds(2.0f);
    }
}
