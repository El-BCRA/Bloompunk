using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Bloompunk/EnemyData")]

public class EnemyData : ScriptableObject
{
    #region variables
    [Space(10)]
    [Title("State Parameters")]
    [PropertyTooltip("Distance from which the enemy can sense and/or see the player.")]
    public float AlertRadius;
    [PropertyTooltip("Distance from which the enemy can damage the player.")]
    public float AttackRadius;
    [PropertyTooltip("Currently unimplemented (artifact from prototype).")]
    public float InteractionRadius;

    [Space(10)]
    [Title("Movement Parameters")]
    public float WalkSpeed;
    public float RotationSpeed;
    [PropertyTooltip("Used for the thornWall ability, applied to both rotation and walk speed.")]
    [Range(0.0f, 1.0f)] public float MovementModifier = 1.0f;

    [Space(10)]
    [Title("Combat Parameters")]
    public float MaxHealth;
    public float AttackDamage;
    public float AttackCooldown;
    [PropertyTooltip("For sniper-like enemies, amount of time which this enemy has the player in-view before attacking.")]
    public float CriticalHitModifier;
    [PropertyTooltip("For sniper-like enemies, amount of time which this enemy has the player in-view before attacking.")]
    public float LockOnDelay;
    [PropertyTooltip("Amount of time spent in knockbackState before returning to idleState.")]
    public float HitStunLength;

    // [Space(10)]
    // [Title("Progression Parameters")]
    #endregion
}