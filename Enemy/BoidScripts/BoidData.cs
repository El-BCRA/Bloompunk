using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Bloompunk/BoidData")]

public class BoidData : ScriptableObject {
    #region variables
    [Space(10)]
    [Title("General Movement Parameters")]
    public float minSpeed = 2;
    public float maxSpeed = 5;
    [PropertyTooltip("Distance from boid at which it will notice the movement of nearby boids. Defaults to 2.5.")]
    public float perceptionRadius = 2.5f;
    [PropertyTooltip("Distance from boid at which it will steer to avoid obstacles. Defaults to 1.")]
    public float avoidanceRadius = 1;
    [PropertyTooltip("Max rate at which a boids path will change in a frame. Defaults to 3.")]
    public float maxSteerForce = 3;

    [Space(10)]
    [Title("Flock Parameters")]
    [PropertyTooltip("Tendency of boids to steer in the same direction as nearby boids. Defaults to 1.")]
    public float alignWeight = 1;
    [PropertyTooltip("Tendency of boids to steer towards the center of nearby boids. Defaults to 1.")]
    public float cohesionWeight = 1;
    [PropertyTooltip("Tendency of boids to steer away from obstacles. Defaults to 1.")]
    public float seperationWeight = 1;
    [PropertyTooltip("Tendency of boids to steer towards the target transform. Defaults to 1.")]
    public float targetWeight = 1;

    [Space(10)]
    [Title("Collisions")]
    [PropertyTooltip("GameObjects which the boids will consider \"obstacles\"")]
    public LayerMask obstacleMask;
    [PropertyTooltip("Distance from the boid from which the collision sphere-cast originates. Defaults to 0.27.")]
    public float boundsRadius = .27f;
    [PropertyTooltip("Radius of the sphere-casts. Defaults to 5.")]
    public float collisionAvoidDst = 5;
    [PropertyTooltip("Multiplier that affects how strongly boids avoid collisions. Defaults to 10.")]
    public float avoidCollisionWeight = 10;
    #endregion
}