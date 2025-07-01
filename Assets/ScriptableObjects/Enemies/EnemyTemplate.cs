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
    [BoxGroup("Movement/Detection Ranges")]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float alertRange = 15f;
    
    [TabGroup("Movement")]
    [BoxGroup("Movement/Detection Ranges")]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float aggroRange = 10f;
    
    [TabGroup("Movement")]
    [BoxGroup("Movement/Detection Ranges")]
    [MinValue(0f)]
    [SuffixLabel("units")]
    public float attackRange = 2f;

    [TabGroup("Movement")]
    [BoxGroup("Movement/Speed Settings")]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float patrolSpeed = 1f;
    
    [TabGroup("Movement")]
    [BoxGroup("Movement/Speed Settings")]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float chaseSpeed = 3f;
    
    [TabGroup("Movement")]
    [BoxGroup("Movement/Rotation")]
    [Range(1f, 100f)]
    [SuffixLabel("degrees/sec")]
    public float rotationSpeed = 20f; // Speed at which the enemy turns towards the player

    [TabGroup("Effects")]
    [AssetsOnly]
    public GameObject bloodEffectPrefab;

    [TabGroup("Timing")]
    [BoxGroup("Timing/Alert Settings")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float alertDuration = 5f; // Duration for which the enemy remains alert

    [TabGroup("Timing")]
    [BoxGroup("Timing/Turn Animations")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float turn180Phase1Duration = 0.7f; // Duration of the first phase of 180° turning (AlertState)

    [TabGroup("Timing")]
    [BoxGroup("Timing/Turn Animations")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float turn180Phase2Duration = 0.6f; // Duration of the second phase of 180° turning (AlertState)

    [TabGroup("Timing")]
    [BoxGroup("Timing/Aggro Animations")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float aggro180Phase1Duration = 0.5f; // Duration of the first phase of Aggro180 turn animation

    [TabGroup("Timing")]
    [BoxGroup("Timing/Aggro Animations")]
    [MinValue(0.1f)]
    [SuffixLabel("seconds")]
    public float aggro180Phase2Duration = 0.5f; // Duration of the second phase of Aggro180 turn animation

    [Button("Preview Range Visualization")]
    [InfoBox("This will show you the relative sizes of your detection ranges")]
    private void PreviewRanges()
    {
        Debug.Log($"Detection Ranges for {name}:");
        Debug.Log($"  Attack Range: {attackRange} units");
        Debug.Log($"  Aggro Range: {aggroRange} units");
        Debug.Log($"  Alert Range: {alertRange} units");
        
        if (attackRange > aggroRange)
            Debug.LogWarning("⚠️ Attack range is larger than aggro range!");
        if (aggroRange > alertRange)
            Debug.LogWarning("⚠️ Aggro range is larger than alert range!");
    }
}
