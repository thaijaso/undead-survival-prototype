using UnityEngine;

public class HitReactionState : EnemyState
{
    private float duration;
    private float timer;
    private Limb limb;

    private int damage; // Damage to apply

    private Vector3 hitDirection; // Direction of the hit

    private float impactForce; // Force of the impact

    public HitReactionState(
        Enemy enemy,
        StateMachine<EnemyState> stateMachine,
        AnimationManager animationManager,
        string animationName,
        Limb limb,
        int damage,
        Vector3 hitDirection,
        float impactForce,
        float duration = 0.15f
    ) : base(
        enemy,
        stateMachine,
        animationManager,
        animationName)
    {
        this.duration = duration;
        this.limb = limb;
        this.damage = damage;
        this.hitDirection = hitDirection;
        this.impactForce = impactForce;
    }

    public override void Enter()
    {
        base.Enter();

        timer = 0f;

        animationManager.SetIsHit(true);
        animationManager.SetHitReactionParams(90f, 1f);

        if (limb != null)
        {
            limb.TakeDamage(damage); // Apply damage to the limb
            limb.AddForce(hitDirection, impactForce); // Apply force to the limb
        }
        else
        {
            Debug.LogWarning("Limb is null in HitReactionState. Is there a limb component on the hitbox?");
        }
    }

    public override void Exit(EnemyState nextState)
    {
        base.Exit(nextState);

        animationManager.SetIsHit(false);
        animationManager.SetHitReactionParams(0f, 0f);

        if (limb != null)
        {

        }
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        timer += Time.deltaTime;

        // Only transition if both the timer and animation are finished
        if (timer >= duration)
        {
            Debug.Log($"[{enemy.gameObject.name}] HitReactionState: Timer expired ({timer:F2}s), transitioning to Aggro state...");
            
            // Safety check for null state before transitioning
            if (enemy.Aggro == null)
            {
                Debug.LogError($"[{enemy.gameObject.name}] CRITICAL: enemy.Aggro is NULL! Cannot transition from HitReactionState!");
                Debug.LogError($"[{enemy.gameObject.name}] Available states: Idle={enemy.Idle != null}, Alert={enemy.Alert != null}, Patrol={enemy.Patrol != null}, Aggro={enemy.Aggro != null}");
                return;
            }
            
            stateMachine.SetState(enemy.Aggro);
        }
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
    }
}