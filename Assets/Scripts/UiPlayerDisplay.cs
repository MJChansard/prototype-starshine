using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiPlayerDisplay : MonoBehaviour
{
    private Image displayedWeapon;
    
    [SerializeField] private Image autoCannonImage;
    [SerializeField] private Text autoCannonText;

    [SerializeField] private Image missileLauncherImage;
    [SerializeField] private Text missileLauncherText;

    [SerializeField] private Image railgunImage;
    [SerializeField] private Text railGunText;

    [SerializeField] private RawImage selector;

    Dictionary <string, RectTransform> weapons = new Dictionary<string, RectTransform>();

    private void Start()
    {
        displayedWeapon = GetComponent<Image>();
        
        weapons.Add("AutoCannon", autoCannonImage.rectTransform);
        weapons.Add("Missile Launcher", missileLauncherImage.rectTransform);
        weapons.Add("Railgun", railgunImage.rectTransform);
    }

    public void SetDisplayWeapon(Sprite currentWeapon)
    {
        //displayedWeapon.sprite = currentWeapon;
    }

    public void SetDisplayWeapon(Weapon currentWeapon)
    {
        string weaponName = currentWeapon.Name;
        RectTransform selectorLocation = weapons[weaponName];

        if (selectorLocation != null)
        {
            selector.rectTransform.anchoredPosition = selectorLocation.anchoredPosition;
        }
    }
}
