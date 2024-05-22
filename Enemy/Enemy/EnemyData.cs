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
    [PropertyTooltip("Distance from which the player is notified that the enemy is near.")]
    public float NearbyRadius;
    [PropertyTooltip("Distance from which the enemy can damage the player.")]
    public float AttackRadius;
    [PropertyTooltip("Flee radius for the sniper-jumping system. Should be 0 on all non-sniper enemies.")]
    public float FleeRadius = 0.0f;
    [PropertyTooltip("Radii between which an EnemyPoint must be to be considered a valid jump-point.")]
    public float FleeDistanceMin = 0.0f;
    [PropertyTooltip("Radii between which an EnemyPoint must be to be considered a valid jump-point.")]
    public float FleeDistanceMax = 0.0f;
    [PropertyTooltip("Time in seconds it takes for a sniper to arrive at their destination point while fleeing.")]
    public float FleeTravelTime = 2.0f;
    [PropertyTooltip("Time in seconds during which a sniper will not re-enter fleeState after recently fleeing.")]
    public float FleeCooldownTime = 10.0f;
    [PropertyTooltip("Time in seconds after which a Sniper will be forced to jump if it hasn't jumped recently.")]
    public float ForcedFleeTime = 40.0f;
    [PropertyTooltip("Number that is added or subtracted from FleeCooldownTimer for varianced (doubled and added/subtracted from ForcedFleeTime.")]
    public float FleeVariance = 3.0f;

    [Space(10)]
    [Title("Movement Parameters")]
    public float WalkSpeed;
    public float RotationSpeed;
    [HideInInspector] public float MovementModifier = 1.0f;
    [PropertyTooltip("Used for the thornwall ability, applied to both rotation and walk speed.")]
    [Range(0.0f, 1.0f)] public float ThornwallSlow = 1.0f;

    [Space(10)]
    [Title("Combat Parameters")]
    public float MaxHealth;
    public float AttackDamage;
    public float AttackCooldown;
    public float CriticalHitModifier;
    [PropertyTooltip("For sniper-like enemies, amount of time which this enemy has the player in-view before attacking.")]
    public float LockOnDelay;
    [PropertyTooltip("Amount of time spent in knockbackState before returning to idleState.")]
    public float HitStunLength;
    #endregion
}