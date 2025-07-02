using System.Collections;
using UnityEngine;

public class DeathState : EnemyState
{
    private bool hasActivatedRagdoll = false;

    public DeathState(
        Enemy enemy,
        StateMachine<EnemyState> stateMachine,
        AnimationManager animationManager,
        string animationName
    ) : base(
        enemy,
        stateMachine,
        animationManager,
        animationName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Debug.Log($"[{enemy.gameObject.name}] DeathState.Enter(): Entering Death state");

        // Disable AI movement immediately
        if (enemy.AIDestinationSetter != null)
        {
            enemy.AIDestinationSetter.enabled = false;
        }

        // Stop any ongoing movement
        if (followerEntity != null)
        {
            followerEntity.maxSpeed = 0f;
        }

        // Go straight to ragdoll for immediate physics response
        ActivateRagdoll();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        if (!hasActivatedRagdoll)
        {
            ActivateRagdoll();
        }
    }

    private void ActivateRagdoll()
    {
        if (hasActivatedRagdoll) return;

        if (enemy.PuppetMaster != null)
        {
            Debug.Log($"[{enemy.gameObject.name}] DeathState.ActivateRagdoll(): Activating ragdoll");

            // Activate ragdoll physics
            enemy.PuppetMaster.state = RootMotion.Dynamics.PuppetMaster.State.Dead;
            hasActivatedRagdoll = true;
        }
        else
        {
            Debug.LogWarning($"[{enemy.gameObject.name}] DeathState.ActivateRagdoll(): No PuppetMaster found, cannot activate ragdoll");
        }
    }
}
