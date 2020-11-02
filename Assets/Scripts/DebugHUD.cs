using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugHUD : MonoBehaviour
{
    [SerializeField] private Text currentTickValue;
       
    public void IncrementTickValue(int newTickValue)
    {
        if (currentTickValue != null)
            currentTickValue.text = newTickValue.ToString();
    }
}
