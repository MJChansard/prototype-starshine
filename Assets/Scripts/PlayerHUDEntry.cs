using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDEntry : MonoBehaviour
{
    [SerializeField] private Sprite entrySprite;
    [SerializeField] private string entryText;

    private void Start()
    {
        Image imageChild = GetComponentInChildren<Image>();
        imageChild.sprite = entrySprite;

        Text textChild = GetComponentInChildren<Text>();
        textChild.text = entryText;
    }
}
