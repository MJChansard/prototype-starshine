using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameDisplay : MonoBehaviour
{
    public Text GameOverText;

    public void GameOver()
    {
        GameOverText.enabled = true;
    }
}
