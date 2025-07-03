using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "PlayerTemplate", menuName = "ScriptableObjects/PlayerTemplate")]
public class PlayerTemplate : ScriptableObject
{
    [TabGroup("Health")]
    [MinValue(1)]
    [SuffixLabel("HP")]
    public int maxHealth = 100;

    [TabGroup("Movement")]
    [BoxGroup("Speed Settings", ShowLabel = true)]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float strafeSpeed = 2f;
    
    [TabGroup("Movement")]
    [BoxGroup("Speed Settings")]
    [MinValue(0.1f)]
    [SuffixLabel("units/sec")]
    public float sprintSpeed = 5f;

    [TabGroup("Movement")]
    [BoxGroup("Physics Settings", ShowLabel = true)]
    [Range(-20f, -5f)]
    [SuffixLabel("m/s²")]
    [InfoBox("Negative values pull the player downward. Standard Earth gravity is -9.81 m/s²")]
    public float gravity = -9.81f;

    [TabGroup("Debug")]
    [Button("Validate Settings")]
    [InfoBox("Validates template settings and checks for potential configuration issues")]
    private void ValidateSettings()
    {
        Debug.Log($"[PlayerTemplate] Player Settings for {name}:");
        Debug.Log($"[PlayerTemplate]   Max Health: {maxHealth} HP");
        Debug.Log($"[PlayerTemplate]   Strafe Speed: {strafeSpeed} units/sec");
        Debug.Log($"[PlayerTemplate]   Sprint Speed: {sprintSpeed} units/sec");
        Debug.Log($"[PlayerTemplate]   Gravity: {gravity} m/s²");
        
        if (sprintSpeed <= strafeSpeed)
            Debug.LogWarning("[PlayerTemplate] ⚠️ Sprint speed should be faster than strafe speed!");
            
        if (gravity > -5f)
            Debug.LogWarning("[PlayerTemplate] ⚠️ Gravity seems too weak (should be more negative)!");
            
        if (gravity < -20f)
            Debug.LogWarning("[PlayerTemplate] ⚠️ Gravity seems too strong (player will fall too fast)!");
    }
}
