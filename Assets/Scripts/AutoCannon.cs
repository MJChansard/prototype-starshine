using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoCannon : Weapon
{
    override public string Name
    {
        get
        {
            weaponName = "AutoCannon";
            return weaponName;    
        }
    }

    override public int Damage
    {
        get
        {
            weaponDamage = 50;
            return weaponDamage;
        }
    }

    override protected IEnumerator AnimationCoroutine(GridBlock target)
    {
        //cannonCoroutineIsRunning = true;

        ParticleSystem ps = GetComponent<ParticleSystem>();
        var trigger = ps.trigger;
        trigger.enabled = true;

        trigger.SetCollider(0, target.objectOnBlock.GetComponent<Collider>());
        trigger.radiusScale = 0.5f;

        ps.Play();
        yield return new WaitForSeconds(2.0f);
        ps.Stop();

        //cannonCoroutineIsRunning = false;
    }
}
