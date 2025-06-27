using UnityEngine;

[CreateAssetMenu(fileName = "LimbTemplate", menuName = "ScriptableObjects/Enemy/LimbTemplate")]
public class LimbTemplate : ScriptableObject
{
    public int maxHealth = 100;
    public int damageMultiplier = 1;

    public bool canBeDismembered = true;
}
