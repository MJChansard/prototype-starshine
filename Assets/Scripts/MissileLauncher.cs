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


    public IEnumerator LaunchMissileCoroutine(GridBlock currentGrid, GridBlock targetGrid)
    {
        Vector3 currentWorldLocation = new Vector3(currentGrid.location.x, currentGrid.location.y, 0);
        Vector3 targetWorldLocation = new Vector3(targetGrid.location.x, targetGrid.location.y, 0);

        GameObject missile = Instantiate(projectilePrefab, currentWorldLocation, transform.rotation);
        
        MovePattern movement = missile.GetComponent<MovePattern>();
        Vector2Int facingDirection = targetGrid.location - currentGrid.location;
        
        if (facingDirection == new Vector2Int(0, 1)) movement.SetMovePatternUp();
        else if (facingDirection == new Vector2Int(0, -1)) movement.SetMovePatternDown();
        else if (facingDirection == new Vector2Int(-1, 0)) movement.SetMovePatternLeft();
        else if (facingDirection == new Vector2Int(1, 0)) movement.SetMovePatternRight();

        launchedMissile = missile.GetComponent<Hazard>();
        launchedMissile.currentWorldLocation = currentWorldLocation;
        launchedMissile.targetWorldLocation = targetWorldLocation;

        StartCoroutine(AnimationCoroutine(targetGrid));
        yield return new WaitForSeconds(4.0f);

        if (pm.OnPlayerAddHazard != null)
        {
            Debug.Log("OnPlayerAddHazard() called.");
            pm.OnPlayerAddHazard(launchedMissile, targetGrid.location);
        }
        else
        {
            Debug.LogError("No subscribers to OnPlayerHazard().");
        }
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
