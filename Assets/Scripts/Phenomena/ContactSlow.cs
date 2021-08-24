using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactSlow : MonoBehaviour
{
    public GameObject[] AffectedObjects;
    public int tickDelayAmount;

    public void ApplySlow(MovePattern move)
    {
        move.ReceiveMoveDelay(tickDelayAmount);
    }
}
