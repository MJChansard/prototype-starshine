using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // #INSPECTOR
    [SerializeField] private bool VerboseLogging;
    // #FIELDS
    public bool InputActive { get; private set; }

    // #DELEGATES
    public System.Action NextModuleButtonPressed;
    public System.Action PreviousModuleButtonPressed;

    public System.Action EndTurnButtonPressed;
    public System.Action MoveButtonPressed;
    public System.Action ActivateModuleButtonPressed;

    public System.Action<Vector2Int> ChangeDirectionButtonPressed;

    void Awake()
    {
        InputActive = true;
    }

    void Update()
    {
        if (InputActive)
            PlayerInput();
    }

    private void PlayerInput()
    {
        // #MOVEMENT
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (ChangeDirectionButtonPressed != null)
                ChangeDirectionButtonPressed(Vector2Int.up);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (ChangeDirectionButtonPressed != null)
                ChangeDirectionButtonPressed(Vector2Int.down);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (ChangeDirectionButtonPressed != null)
                ChangeDirectionButtonPressed(Vector2Int.left);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (ChangeDirectionButtonPressed != null)
                ChangeDirectionButtonPressed(Vector2Int.right);
        }


        // #MODULE CONTROL
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (PreviousModuleButtonPressed != null)
                PreviousModuleButtonPressed();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (NextModuleButtonPressed != null)
                NextModuleButtonPressed();
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (ActivateModuleButtonPressed != null)
            {
                Debug.Log("Module button Pressed!");
                ActivateModuleButtonPressed();
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (EndTurnButtonPressed != null)
                EndTurnButtonPressed();

            // NOTE: This button is incorrectly moving the player
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (MoveButtonPressed != null)
                MoveButtonPressed();
        }
    }

    public void SetInputActive(bool isActive)
    {
        InputActive = isActive;
    }

}


/*  #DEPRECATED
     if (Input.GetKeyDown(KeyCode.Q))
     {
         //SelectWeapon(-1);
         //OnChangeSelectedWeapon?.Invoke(SelectedWeapon.weaponIcon, SelectedWeapon.currentAmmunition);

         /*
         Weapon w = weaponInventory[indexSelectedWeapon];

         if (w.currentAmmunition > 0)
         {
             IsAttackingThisTick = true;
             w.SubtractAmmo();
             OnAmmoAmountChange?.Invoke(indexSelectedWeapon, w.currentAmmunition, 0.0f);
         }

         if (OnPlayerAdvance != null) OnPlayerAdvance();
     }

    if (Input.GetKeyDown(KeyCode.UpArrow))
    {
        Debug.Log("UpArrow key pressed.");

        for (int i = 0; i < weaponInventory.Length; i++)
        {
            if (weaponInventory[i].IsActive)
                weaponInventory[i].ToggleWeaponActive();
        }
        weaponInventory[indexSelectedWeapon].ToggleWeaponActive();
        OnActivateSelectedWeapon?.Invoke(indexSelectedWeapon);
    }

    if (Input.GetKeyDown(KeyCode.E))
    {
        //SelectWeapon(1);
        //OnChangeSelectedWeapon?.Invoke(SelectedWeapon.weaponIcon, SelectedWeapon.currentAmmunition);

    }

    if (Input.GetKeyDown(KeyCode.Q))
    {
        int indexPreviousWeapon = indexSelectedWeapon;
        SelectWeapon(-1);
        OnChangeSelectedWeapon?.Invoke(indexPreviousWeapon, indexSelectedWeapon);

        if (PreviousModuleButtonPressed != null)
            PreviousModuleButtonPressed();
    }

    if (Input.GetKeyDown(KeyCode.E))
    {

    }

    if (Input.GetKeyDown(KeyCode.Q))
    {
        int indexPreviousWeapon = indexSelectedWeapon;
        SelectWeapon(-1);
        OnChangeSelectedWeapon?.Invoke(indexPreviousWeapon, indexSelectedWeapon);

        if (PreviousModuleButtonPressed != null)
            PreviousModuleButtonPressed();
    }

     if (Input.GetKeyDown(KeyCode.RightArrow))
    {
    
        int indexPreviousWeapon = indexSelectedWeapon;
        SelectWeapon(1);
        OnChangeSelectedWeapon?.Invoke(indexPreviousWeapon, indexSelectedWeapon);
    

        if (NextModuleButtonPressed != null)
            NextModuleButtonPressed();
    }

// Player Movement & turn advancement


if (Input.GetKeyDown(KeyCode.S))
{
    //if (OnPlayerAdvance != null) OnPlayerAdvance();
}

if (Input.GetKeyDown(KeyCode.T))
{
    // Debug button
}
*/