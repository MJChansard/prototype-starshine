using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleUseData
{
    // #FIELDS
    private int baseDamage;
    private float damageMultiplier;
    private bool doesPenetrate;

    private bool eligibleToMove;
    private Vector2Int moveDirection;
    private int numberOfMoves;

    private bool doesPlaceObjectInWorld;
    private GameObject objectToPlaceInWorld;

    public int newAmmoAmount;

    // #PROPERTIES
    public bool DoesMove { get; private set; }
    public bool DoesDamage { get; private set; }
    public bool DoesShield { get; private set; }

    public bool DynamicDamage { get; private set; }
    public bool DoesPenetrate { get; private set; }
    public int BaseDamage { get; private set; }
    public float DamageMultiplier { get; private set; }

    public bool EligibleToMove { get; private set; }
    public Vector2Int DirectionToMove { get; private set; }
    public int NumberOfMoves { get; private set; }


    // #METHODS
    public void SetDamageFields(bool dynamicDamage, bool doesPenetrate, int baseDamage, float damageMultiplier)
    {
        if (DoesDamage)
        {
            this.DynamicDamage = dynamicDamage;
            this.DoesPenetrate = doesPenetrate;
            this.BaseDamage = baseDamage;
            this.DamageMultiplier = damageMultiplier;
        }
    }

    public void SetMoveFields(bool eligibleToMove, Vector2Int moveDirection, int numberOfMoves = 1)
    {
        if (DoesMove)
        {
            this.EligibleToMove = eligibleToMove;
            this.DirectionToMove = moveDirection;
            this.NumberOfMoves = numberOfMoves;
        }
    }

    public ModuleUseData(bool damage, bool move, bool shield)
    {
        DoesDamage = damage;
        DoesMove = move;
        DoesShield = shield;
    }
}
