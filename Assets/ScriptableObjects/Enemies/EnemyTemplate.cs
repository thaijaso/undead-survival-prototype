using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTemplate", menuName = "ScriptableObjects/Enemy/EnemyTemplate")]
public class EnemyTemplate : ScriptableObject
{
    public int maxHealth = 100;

    public float alertRange = 15f;
    public float patrolSpeed = 1f;
    public float chaseSpeed = 3f;

    public GameObject bloodEffectPrefab;

    public float alertDuration = 5f; // Duration for which the enemy remains alert

    public float rotationSpeed = 20f; // Speed at which the enemy turns towards the player
    
    public float turnTheshold = 5f; // Threshold angle for turning towards the player

    public float aggroRange = 10f;
}
