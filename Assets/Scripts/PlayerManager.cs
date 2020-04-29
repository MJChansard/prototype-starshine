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

    public GridBlock parentBlock;

    #endregion

    #region Private Attributes
    GridManager gridManager;
    Vector2Int delta = Vector2Int.zero;

    #endregion

    private void Start()
    {
        gridManager = GameObject.FindWithTag("GameController").GetComponent<GridManager>();
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
            delta = Vector2Int.up;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            transform.rotation = Quaternion.AngleAxis(180.0f, Vector3.forward);
            delta = Vector2Int.down;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            transform.rotation = Quaternion.AngleAxis(90.0f, Vector3.forward);
            delta = Vector2Int.left;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            transform.rotation = Quaternion.AngleAxis(-90.0f, Vector3.forward);
            delta = Vector2Int.right;
        }
        
        //Debug.Log("Current Vector3: " + transform.position + ". Target Vector3: " + playerMoveTo);
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            gridManager.RequestMoveRelative(this.gameObject, delta);
            Debug.Log("Move requested.");
            //gridManger.UpdateBoard();
        }
    }
}
