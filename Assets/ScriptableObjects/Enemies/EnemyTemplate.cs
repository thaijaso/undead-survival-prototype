using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "EnemyTemplate", menuName = "ScriptableObjects/Enemy/EnemyTemplate")]
public class EnemyTemplate : ScriptableObject
{

    [TabGroup("Health")]
    [MinValue(1)]
    [SuffixLabel("HP")]
    public int maxHealth = 100;

    [TabGroup("Movement")]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float alertRange = 15f;

    [TabGroup("Movement")]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float aggroRange = 10f;

    [TabGroup("Movement")]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float attackRange = 2f;

    [TabGroup("Movement")]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float patrolSpeed = 1f;

    [TabGroup("Movement")]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float chaseSpeed = 3f;

    [TabGroup("Movement")]
    [Range(1f, 100f)]
    [SuffixLabel("degrees/sec")]
    public float rotationSpeed = 20f; // Speed at which the enemy turns towards the player

    [TabGroup("Animation")]
    [InfoBox("Assign the animator controller for this enemy template.")]
    public RuntimeAnimatorController animatorController;

    [TabGroup("Animation")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float turn180Phase1Duration = 0.7f; // Duration of the first phase of 180° turning (AlertState)

    [TabGroup("Animation")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float turn180Phase2Duration = 0.6f; // Duration of the second phase of 180° turning (AlertState)

    [TabGroup("Animation")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float aggro180Phase1Duration = 0.5f; // Duration of the first phase of Aggro180 turn animation

    [TabGroup("Animation")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float aggro180Phase2Duration = 0.5f; // Duration of the second phase of Aggro180 turn animation

    [TabGroup("Effects")]
    [AssetsOnly]
    public GameObject bloodEffectPrefab;


    [TabGroup("Timing")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float alertDuration = 5f; // Duration for which the enemy remains alert


    [TabGroup("Debug")]
    [Button("Preview Range Visualization")]
    [InfoBox("This will show you the relative sizes of your detection ranges")]
    private void PreviewRanges()
    {
        Debug.Log($"[EnemyTemplate] Detection Ranges for {name}:");
        Debug.Log($"[EnemyTemplate]   Attack Range: {attackRange} units");
        Debug.Log($"[EnemyTemplate]   Aggro Range: {aggroRange} units");
        Debug.Log($"[EnemyTemplate]   Alert Range: {alertRange} units");
        
        if (attackRange > aggroRange)
            Debug.LogWarning("[EnemyTemplate] ⚠️ Attack range is larger than aggro range!");
        if (aggroRange > alertRange)
            Debug.LogWarning("[EnemyTemplate] ⚠️ Aggro range is larger than alert range!");
    }
}
