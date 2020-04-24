using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Inspector Attributes

    [SerializeField]
    private float speed = 2.0f;

    #endregion


    #region Public Attributes

    public Vector3 playerMoveTo;
    public GridBlock parentBlock;
    
    public int playerX
    {
        get { return (int) transform.position.x; }
    }

    public int playerY
    {
        get { return (int)transform.position.y; }
    }

    #endregion

    #region Private Attributes
    Experiment gameManager;

    #endregion

    private void Start()
    {
        gameManager = GameObject.FindWithTag("GameController").GetComponent<Experiment>();
    }

    void Update()
    {
        Movement();
    }

    void Movement()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {

            transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
            playerMoveTo = transform.position + new Vector3(0f, 1f, 0f);            
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
            playerMoveTo = transform.position + new Vector3(0f, -1f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.forward);
            playerMoveTo = transform.position + new Vector3(-1f, 0f, 0f);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.forward);
            playerMoveTo = transform.position + new Vector3(1f, 0f, 0f);
        }
        
        Debug.Log("Current Vector3: " + transform.position + ". Target Vector3: " + playerMoveTo);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gameManager.RequestMove(this.gameObject, transform.position, playerMoveTo);
        }
    }

    
}
