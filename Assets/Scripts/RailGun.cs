using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RailGun : Weapon
{
    override public string Name
    {
        get
        {
            weaponName = "Railgun";
            return weaponName;
        }
    }

    [SerializeField] private GameObject railPrefab;
    
    private GameObject launchedRailgunProjectile;
    private float railTravelSpeed = 5.0f;
    private Transform player;

    private void Start()
    {
        player = GetComponentInParent<Transform>();     
    }
    
    public bool FireRailgun(Vector3 currentWorldLocation)
    {
        if (weaponAmmunition > 0)
        {
            launchedRailgunProjectile = Instantiate(railPrefab, currentWorldLocation, player.transform.rotation);
            weaponAmmunition -= 1;
            return true;
        }

        return false;
    }


    protected override IEnumerator AnimationCoroutine(GridBlock target)
    {
        if (launchedRailgunProjectile != null)
        {      
            ParticleSystem ps = gameObject.GetComponent<ParticleSystem>();
            Vector3 rotation = new Vector3(player.rotation.x, player.rotation.y, player.rotation.z);
            
            var shape = ps.shape;
            shape.rotation = rotation;
            ps.Play();

            // D = s * t
            Vector3 currentWorldLocation = launchedRailgunProjectile.transform.position;
            Vector3 targetWorldLocation = new Vector3(target.location.x, target.location.y, 0.0f);
        
            float distance = Vector3.Distance(currentWorldLocation, targetWorldLocation);
            float startTime = Time.time;
            float percentTraveled = 0.0f;

            while (percentTraveled <= 1.0f)
            {
                float traveled = (Time.time - startTime) * railTravelSpeed;
                percentTraveled = traveled / distance;  // Interpolator for Vector3.Lerp
                
                launchedRailgunProjectile.transform.position = 
                    Vector3.Lerp
                    (
                        currentWorldLocation,
                        targetWorldLocation,
                        percentTraveled
                    );

                yield return null;
            }

            Destroy(launchedRailgunProjectile);
        }
    }
    public override void StartAnimationCoroutine(GridBlock animationFinalGridBlock) 
    {
        if (FireRailgun(player.position))
        {
            //ui.UpdateHUDWeapons(indexSelectedWeapon, railGun.weaponAmmunition);  Might not need to do this now actually
            base.StartAnimationCoroutine(animationFinalGridBlock);
        }
        else
        {
            Debug.Log("Out of Rail Gun ammunition.");
        }
    }           
}
