using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.UIElements;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Inspector Attributes   
    [SerializeField]
    private float speed = 2.0f;
    [SerializeField]
    private Transform weaponSource;
    [SerializeField]
    private GameObject missile;

    [Header("Player Components")]
    [SerializeField]
    private GameObject Thruster;
    [SerializeField]
    private GameObject Cannon;

    [SerializeField]
    private int CannonDamage;
    [SerializeField]
    private int MissileDamage;
    [SerializeField]
    private int RailDamage;
    #endregion

    private GridManager gm;

    #region Private Fields
    private string currentlyFacing = "";
    private Vector2Int delta = Vector2Int.zero;
    private bool isRequestingAttack = false;
    //private bool isRequestingMove = false;

    private float attackWaitTime = 2.0f;
    private float moveWaitTime = 1.0f;
    private float waitWaitTime = 0.5f;
    #endregion

    #region Movement Animation Properties
    public Vector3 currentWorldLocation;
    public Vector3 targetWorldLocation;
    
    private float moveSpeed = 1.0f;
    private bool thrusterCoroutineIsRunning = false;
    private bool moveCoroutineIsRunning = false;

    //private float distance;
    //private float startTime;
    //    private float moveDuration = 1.0f;
    //    private Vector3 velocity = Vector3.zero;
    #endregion

    public System.Action OnPlayerAdvance;


    private void Start()
    {
        gm = GameObject.FindWithTag("GameController").GetComponent<GridManager>();
    }

    void Update()
    {
        PlayerInput();
        
        if(Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Currently facing: " + currentlyFacing);
        }

/*      if (currentWorldLocation != targetWorldLocation)
        {           
            // Distance moved equals elapsed time times speed..
            float traveled = (Time.time - startTime) * moveSpeed;

            // Fraction of journey completed equals current distance divided by total distance.
            float traveledAmount = traveled / distance;

            // Set our position as a fraction of the distance between the markers.
            //transform.position = Vector3.Lerp(currentWorldLocation, targetWorldLocation, traveledAmount);

            // Weird that this only works for the first movement
            transform.position = Vector3.Lerp(currentWorldLocation, targetWorldLocation, Mathf.SmoothStep(0f, 1f, traveledAmount)); 
        }
*/
    }


    void PlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {

            transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            currentlyFacing = "Up";
            delta = Vector2Int.up;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
            currentlyFacing = "Down";
            delta = Vector2Int.down;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.forward);
            currentlyFacing = "Left";
            delta = Vector2Int.left;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.forward);
            currentlyFacing = "Right";
            delta = Vector2Int.right;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            isRequestingAttack = true;
            if (OnPlayerAdvance != null) OnPlayerAdvance();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (OnPlayerAdvance != null && moveCoroutineIsRunning == false) OnPlayerAdvance();
        }
    }


    public float OnTickUpdate()
    {
        if (isRequestingAttack)
        {
            isRequestingAttack = false;
            Attack();
            return attackWaitTime;
        }
        else if (delta != Vector2Int.zero)
        {
            Move();
            return moveWaitTime;
        }
        
        return waitWaitTime;
    }


    private void Move()
    {
        GridBlock current = gm.FindGridBlockContainingObject(this.gameObject);
        bool canMove = gm.CheckIfMoveIsValid(this.gameObject, current.location, current.location + delta);

        if (canMove)
        {
            targetWorldLocation = gm.GridToWorld(current.location + delta);
            
            if (thrusterCoroutineIsRunning == false) StartCoroutine(FireThrusterParticles());
            if (moveCoroutineIsRunning == false) StartCoroutine(AnimateMovementCoroutine());   
        }

    }

    private IEnumerator FireThrusterParticles()
    {
        ParticleSystem ps = Thruster.GetComponent<ParticleSystem>();
        ps.Play();
        yield return new WaitForSeconds(1.0f);
        ps.Stop();
    }

    private IEnumerator AnimateMovementCoroutine()
    {
        moveCoroutineIsRunning = true;

        // D = s * t
        float distance = Vector3.Distance(currentWorldLocation, targetWorldLocation);
        float startTime = Time.time;
        float percentTraveled = 0.0f;

        while (percentTraveled <= 1.0f)
        {
            float traveled = (Time.time - startTime) * moveSpeed;
            percentTraveled = traveled / distance;  // Interpolator for Vector3.Lerp
            transform.position = Vector3.Lerp(currentWorldLocation, targetWorldLocation, Mathf.SmoothStep(0f, 1f, percentTraveled));

            Debug.Log(percentTraveled);
            yield return null;
        }
        
        currentWorldLocation = targetWorldLocation;
        moveCoroutineIsRunning = false;
    }


    private void Attack()
    {
        /*  NOTES
         *   - Might be helpful to re-tool this to be FindWeaponTarget()
         *   - Add a parameter for the weapon type to execute specific behavior
         */

        /*  STEPS
         *   - Need to find current location on the grid (GridBlock)
         *   - Determine which direction player is facing
         *   - Gather all GridBlocks on the cannon trajectory
         *   - Test each block for a destructible GameObject
         *   - Apply damage to the first detected GameObject
         *   - Update Tick
         */

        GridBlock currentlyAt = gm.FindGridBlockContainingObject(this.gameObject);
        
        // Determine weapon path
        List<GridBlock> possibleTargets = new List<GridBlock>();
        switch (currentlyFacing)
        {
            // Facing forward
            case "Up":
                for (int y = currentlyAt.location.y + 1; y < gm.GridHeight; y++)
                {
                    possibleTargets.Add(gm.levelGrid[currentlyAt.location.x, y]);
                }
                break;
            // Facing down
            case "Down":
                for (int y = currentlyAt.location.y - 1; y >= 0; y--)
                {
                    possibleTargets.Add(gm.levelGrid[currentlyAt.location.x, y]);
                }
                break;
            // Facing left
            case "Left":
                for (int x = currentlyAt.location.x -1 ; x >= 0; x--)
                {
                    possibleTargets.Add(gm.levelGrid[x, currentlyAt.location.y]);
                }
                break;
            // Facing right
            case "Right":
                for (int x = currentlyAt.location.x + 1; x < gm.GridWidth; x++)
                {
                    possibleTargets.Add(gm.levelGrid[x, currentlyAt.location.y]);
                }
                break;
        }

        foreach (GridBlock target in possibleTargets)
        {
            if (target.isOccupied == true)
            {
                Health hp = target.objectOnBlock.GetComponent<Health>();

                if (hp != null)
                {
                    StartCoroutine(FireCannonParticles(target));
                    hp.ApplyDamage(CannonDamage);
                    Debug.Log("Target's current health: " + hp.CurrentHP);
                }

                break;       
            }
        }
    }

    private IEnumerator FireCannonParticles(GridBlock target)
    {
        ParticleSystem ps = Cannon.GetComponent<ParticleSystem>();
        var trigger = ps.trigger;
        trigger.enabled = true;
        
        trigger.SetCollider(0, target.objectOnBlock.GetComponent<Collider>());
        trigger.radiusScale = 0.5f;

        ps.Play();
        yield return new WaitForSeconds(2.0f);
        ps.Stop();
    }
}
