using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Bloompunk/SpawnPointData")]

public class SpawnPointData : ScriptableObject
{
    #region variables
    [Space(10)]
    [Title("Enemy Spawn Rates")]
    [ValidateInput("SumToOne", "SniperRate and SpawnerRate should sum to 1")]
    [Range(0, 1)] public float SniperRate;
    [ValidateInput("SumToOne", "SpawnerRate and SniperRate should sum to 1")]
    [Range(0, 1)] public float SpawnerRate;
    #endregion

    private bool SumToOne()
    {
        if (SniperRate + SpawnerRate > .99f && SniperRate + SpawnerRate < 1.01f)
        {
            return true;
        } else
        {
            return false;
        }
    }
}
