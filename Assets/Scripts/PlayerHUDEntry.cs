using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUDEntry : MonoBehaviour
{
    [SerializeField] private GameObject selector;
    [SerializeField] private Sprite entrySprite;
    [SerializeField] private Text entryText;

    private void Start()
    {
        Image imageChild = GetComponentInChildren<Image>();
        imageChild.sprite = entrySprite;

        entryText.color = Color.white;
        
    }

    public void Init(Sprite icon, int ammo)
    {
        entrySprite = icon;
        entryText.text = ammo.ToString();
    }

    public void SetSelector(bool isSelected)
    {
        selector.SetActive(isSelected);
    }
}
