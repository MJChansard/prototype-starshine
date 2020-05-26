using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Public Properties
    public int CurrentTick = 1;
    public int CurrentLevel = 1;
    #endregion

    #region References
    HazardManager hm;
    PlayerManager pm;
    EnemyManager em;
    GridManager gm;
    #endregion

    [SerializeField]
    GameObject playerPrefab;

    private void Start()
    {
        gm = GetComponent<GridManager>();
        gm.Init();
        
        hm = GetComponent<HazardManager>();
        hm.Init();

        em = GetComponent<EnemyManager>();

        GameObject player = Instantiate(playerPrefab);
        gm.PlaceObject(player, new Vector2Int(5, 4));
        pm = player.GetComponent<PlayerManager>();
        pm.OnPlayerAdvance += OnTick;
    }

    private void OnTick()
    {
        StartCoroutine(OnTickCoroutine());
    }

    private IEnumerator OnTickCoroutine()
    {
        pm.OnPlayerAdvance -= OnTick;
       
        float delay = pm.OnTickUpdate();
        yield return new WaitForSeconds(delay);

        //hm.MoveHazards(CurrentTick);
        hm.OnTickUpdate();
        
        CurrentTick += 1;
        pm.OnPlayerAdvance += OnTick;
        yield return null;
    }
}
