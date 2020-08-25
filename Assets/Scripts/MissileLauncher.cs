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

    public GameObject projectilePrefab;
    private Hazard launchedMissile;

    private PlayerManager pm;

    private void Awake()
    {
         pm = GetComponentInParent<PlayerManager>();
    }


    public Hazard LaunchMissile(GridBlock currentGrid, Vector2Int facingDirection)
    {
        if (weaponAmmunition > 0)
        {
            Vector3 currentWorldLocation = new Vector3(currentGrid.location.x, currentGrid.location.y, 0);
            GameObject missile = Instantiate(projectilePrefab, currentWorldLocation, transform.rotation);
            MovePattern movement = missile.GetComponent<MovePattern>();

            if (facingDirection == Vector2Int.up) movement.SetMovePatternUp();
            else if (facingDirection == Vector2Int.down) movement.SetMovePatternDown();
            else if (facingDirection == Vector2Int.left) movement.SetMovePatternLeft();
            else if (facingDirection == Vector2Int.right) movement.SetMovePatternRight();

            weaponAmmunition -= 1;
            
            launchedMissile = missile.GetComponent<Hazard>();
            return launchedMissile;
        }
        else return null;
    }

    override protected IEnumerator AnimationCoroutine(GridBlock targetGrid)
    {
        // D = s * t
        float distance = Vector3.Distance(launchedMissile.currentWorldLocation, launchedMissile.targetWorldLocation);
        float startTime = Time.time;
        float percentTraveled = 0.0f;

        while (percentTraveled <= 1.0f)
        {
            float traveled = (Time.time - startTime) * launchedMissile.moveSpeed;
            percentTraveled = traveled / distance;  // Interpolator for Vector3.Lerp
            launchedMissile.gameObject.transform.position =
                Vector3.Lerp
                (
                    launchedMissile.currentWorldLocation,
                    launchedMissile.targetWorldLocation,
                    Mathf.SmoothStep(0f, 1f, percentTraveled)
                );

            yield return null;
        }            
    }
}
