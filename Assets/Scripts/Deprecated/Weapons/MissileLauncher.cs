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

    private Hazard launchedMissile;

    // #DEPRECATED
    
    override protected IEnumerator AnimationCoroutine(GridBlock targetGrid)
    {
        //#ToDo: Need to rework this animation
        //          - activate a Particle System so missile looks like it is being fired
        if (launchedMissile != null)
        {
            // D = s * t
            //float distance = Vector3.Distance(launchedMissile.animateStartWorldLocation, launchedMissile.animateEndWorldLocation);
            float startTime = Time.time;
            float percentTraveled = 0.0f;

            while (percentTraveled <= 1.0f)
            {
                float traveled = (Time.time - startTime) * launchedMissile.animateMoveSpeed;
                percentTraveled = traveled / distance;  // Interpolator for Vector3.Lerp
                launchedMissile.gameObject.transform.position =
                    Vector3.Lerp
                    (
//                        launchedMissile.animateStartWorldLocation,
//                        launchedMissile.animateEndWorldLocation,
                        Mathf.SmoothStep(0f, 1f, percentTraveled)
                    );

                yield return null;
            }
        }
    }
}
