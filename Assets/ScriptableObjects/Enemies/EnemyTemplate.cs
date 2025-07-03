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
    [BoxGroup("Detection Ranges", ShowLabel = true)]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float alertRange = 15f;
    
    [TabGroup("Movement")]
    [BoxGroup("Detection Ranges")]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float aggroRange = 10f;
    
    [TabGroup("Movement")]
    [BoxGroup("Detection Ranges")]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float attackRange = 2f;

    [TabGroup("Movement")]
    [BoxGroup("Speed Settings", ShowLabel = true)]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float patrolSpeed = 1f;
    
    [TabGroup("Movement")]
    [BoxGroup("Speed Settings")]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float chaseSpeed = 3f;
    
    [TabGroup("Movement")]
    [BoxGroup("Rotation", ShowLabel = true)]
    [Range(1f, 100f)]
    [SuffixLabel("degrees/sec")]
    public float rotationSpeed = 20f; // Speed at which the enemy turns towards the player

    [TabGroup("Effects")]
    [AssetsOnly]
    public GameObject bloodEffectPrefab;

    [TabGroup("Timing")]
    [BoxGroup("Alert Settings", ShowLabel = true)]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float alertDuration = 5f; // Duration for which the enemy remains alert

    [TabGroup("Timing")]
    [BoxGroup("Turn Animations", ShowLabel = true)]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float turn180Phase1Duration = 0.7f; // Duration of the first phase of 180° turning (AlertState)

    [TabGroup("Timing")]
    [BoxGroup("Turn Animations")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float turn180Phase2Duration = 0.6f; // Duration of the second phase of 180° turning (AlertState)

    [TabGroup("Timing")]
    [BoxGroup("Aggro Animations", ShowLabel = true)]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float aggro180Phase1Duration = 0.5f; // Duration of the first phase of Aggro180 turn animation

    [TabGroup("Timing")]
    [BoxGroup("Aggro Animations")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float aggro180Phase2Duration = 0.5f; // Duration of the second phase of Aggro180 turn animation

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
