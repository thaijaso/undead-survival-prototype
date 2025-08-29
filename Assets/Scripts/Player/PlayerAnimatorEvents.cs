using UndeadSurvivalGame.Player;
using UndeadSurvivalGame.Player.States;
using UnityEngine;


public class PlayerAnimatorEvents : MonoBehaviour
{
    private Player player;

    public enum Foot { Left, Right }
    public Foot lastPlantedFoot = Foot.Right; // Default

    void Awake()
    {
        Debug.Log("[PlayerAnimatorEvents] Awake() called.");
        player = GetComponent<Player>();

        if (player == null)
        {
            Debug.LogError("[PlayerAnimatorEvents] Player component not found on this GameObject.");
        }
    }

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

    public void OnKnockbackFinished()
    {
        Debug.Log("[PlayerAnimatorEvents] OnKnockbackFinished() called.");
        if (player.stateMachine.currentState is HitReactionState hitReactionState)
        {
            Debug.Log("[PlayerAnimatorEvents] Delegating to HitReactionState.OnKnockbackFinished()");
            hitReactionState.OnKnockbackFinished();
        }
        else
        {
            Debug.LogWarning("[PlayerAnimatorEvents] OnKnockbackFinished called but not in HitReactionState.");
        }
    }

    public void OnChamberLoaded()
    {
        Debug.Log("[PlayerAnimatorEvents] OnChamberLoaded() called.");
        if (player.stateMachine.currentState is ReloadState reloadState)
        {
            Debug.Log("[PlayerAnimatorEvents] Delegating to ReloadState.OnChamberLoaded()");
            reloadState.OnChamberLoaded();
        }
        else
        {
            Debug.LogWarning("[PlayerAnimatorEvents] OnChamberLoaded called but not in ReloadState.");
        }
    }
}

