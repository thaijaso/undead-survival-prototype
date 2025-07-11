using UnityEngine;


public class PlayerAnimatorEvents : MonoBehaviour
{
    public enum Foot { Left, Right, Both }
    public Foot lastPlantedFoot = Foot.Both; // Default

    public void OnRightFootPlant()
    {
        Debug.Log("[PlayerAnimatorEvents] OnRightFootPlant() called.");
        lastPlantedFoot = Foot.Right;
    }

    public void OnLeftFootPlant()
    {
        Debug.Log("[PlayerAnimatorEvents] OnLeftFootPlant() called.");
        lastPlantedFoot = Foot.Left;
    }
}

