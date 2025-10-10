using UnityEngine;

public class AnimationManager
{
    public Animator Animator { get; private set; }
    public int UpperBodyLayerIndex { get; } = 2;
    private const float blendSpeed = 10f;


    public AnimationManager(Animator animator)
    {
        this.Animator = animator;
    }

    public void PlayAnimation(string animationName, int layerIndex)
    {
        Animator.Play(animationName, layerIndex, 0f);
    }

    public void StopAnimation()
    {
        Animator.StopPlayback();
    }

    public bool IsAnimationPlaying(string animationName, int layerIndex)
    {
        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.IsName(animationName) && stateInfo.normalizedTime < 1.0f;
    }

    public void SetIsIdle(bool isIdle)
    {
        Animator.SetBool("IsIdle", isIdle);
    }

    public void SetMoveParams(float moveX, float moveZ)
    {
        Animator.SetFloat("MoveX", moveX);
        Animator.SetFloat("MoveZ", moveZ);
    }

    public void SetIsSprinting(bool isSprinting)
    {
        Animator.SetBool("IsSprinting", isSprinting);
    }

    public void SetIsStrafing(bool isStrafing)
    {
        Animator.SetBool("IsStrafing", isStrafing);
    }

    public void SetIsAiming(bool isAiming)
    {
        Animator.SetBool("IsAiming", isAiming);
    }

    public void SetIsShooting(bool isShooting)
    {
        Animator.SetBool("IsShooting", isShooting);
    }

    public void SetIsMoving(bool isMoving)
    {
        Animator.SetBool("IsMoving", isMoving);
    }

    public void SetIsLeftFootPlanted(bool isLeftFootPlanted)
    {
        Animator.SetBool("IsLeftFootPlanted", isLeftFootPlanted);
    }

    public void SetIsRightFootPlanted(bool isRightFootPlanted)
    {
        Animator.SetBool("IsRightFootPlanted", isRightFootPlanted);
    }

    /// <summary>
    /// Sets the stop direction for the animator.
    /// Index meanings:
    /// 0 = forward
    /// 1 = right
    /// 2 = down (back)
    /// 3 = left
    /// </summary>
    public void SetStopDirection(int directionIndex)
    {
        Animator.SetInteger("StopDirection", directionIndex);
    }

    public void SetMoveCommited(bool moveCommited)
    {
        Animator.SetBool("MoveCommited", moveCommited);
    }

    public void SetIsWeaponHolstered(bool isHolstered)
    {
        Animator.SetBool("IsWeaponHolstered", isHolstered);
    }

    public void TriggerPistolShootPowerful()
    {
        Animator.SetTrigger("PistolShootPowerful");
    }

    public void SetLayerWeight(int layerIndex, float weight)
    {
        Animator.SetLayerWeight(layerIndex, weight);
    }

    public void BlendLayerWeight(int layerIndex, float targetWeight)
    {
        float currentWeight = Animator.GetLayerWeight(layerIndex);
        float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * blendSpeed);
        Animator.SetLayerWeight(layerIndex, newWeight);
    }

    public void SetAlertState(bool isAlert)
    {
        Animator.SetBool("IsAlert", isAlert);
    }

    public void SetTurnAngle(float angle)
    {
        Animator.SetFloat("TurnAngle", angle);
    }

    public void SetIsAggro(bool isAggro)
    {
        Animator.SetBool("IsAggro", isAggro);
    }

    public void SetTrigger(string triggerName)
    {
        Animator.SetTrigger(triggerName);
    }

    public void ResetTrigger(string triggerName)
    {
        Animator.ResetTrigger(triggerName);
    }

    public void SetIsTurning(bool isTurning)
    {
        Animator.SetBool("IsTurning", isTurning);
    }

    public void SetIsDead(bool isDead)
    {
        Animator.SetBool("IsDead", isDead);
    }

    public void SetIsAttacking(bool isAttacking)
    {
        Animator.SetBool("IsAttacking", isAttacking);
    }

    public void SetIsInAttackRange(bool isInAttackRange)
    {
        Animator.SetBool("IsInAttackRange", isInAttackRange);
    }

    public void SetHasAggroAnimStarted(bool isStarted)
    {
        Animator.SetBool("HasAggroAnimStarted", isStarted);
    }

    public bool SetHasAgroAnimationFinished(bool hasFinished)
    {
        Animator.SetBool("HasAggroAnimFinished", hasFinished);
        return hasFinished;
    }

    public void TriggerForwardKnockback()
    {
        Animator.SetTrigger("ForwardKnockback");
    }

    public void TriggerForwardKnockdown()
    {
        Animator.SetTrigger("ForwardKnockdown");
    }

    public void SetHasAggroed(bool hasAggroed)
    {
        Animator.SetBool("HasAggroed", hasAggroed);
    }

    public void TriggerLegKnockdown()
    {
        Animator.SetTrigger("LegKnockdown");
    }

    public void SetIsChasing(bool isChasing)
    {
        Animator.SetBool("IsChasing", isChasing);
    }

    public void TriggerBackKnockback()
    {
        Debug.Log($"[{Animator.gameObject.name}] Triggering Backward Knockback");
        Animator.SetTrigger("BackKnockback");
    }

    public void TriggerBackKnockdown()
    {
        Debug.Log($"[{Animator.gameObject.name}] Triggering Backward Knockdown");
        Animator.SetTrigger("BackKnockdown");
    }

    public bool IsHitReactionPlaying()
    {
        return IsAnimationPlaying("Leg Knockdown", 0) ||
               IsAnimationPlaying("Forward Knockback", 0) ||
               IsAnimationPlaying("Forward Knockdown", 0) ||
               IsAnimationPlaying("Backward Knockback", 0) ||
               IsAnimationPlaying("Backward Knockdown", 0) ||
               IsAnimationPlaying("Face Up Get Up", 0) ||
               IsAnimationPlaying("Face Down Get Up", 0);
    }

    public void TriggerKnockback()
    {
        Animator.SetTrigger("Knockback");
    }

    public void TriggerRevolverReloadAnimation()
    {
        Animator.SetTrigger("ReloadRevolver");
    }

    public void SetIsReloading(bool isReloading)
    {
        Animator.SetBool("IsReloading", isReloading);
    }

    public void SetSprintStopGracePeriodFinished(bool finished)
    {
        Animator.SetBool("SprintStopGracePeriodFinished", finished);
    }

    public void SetAimPitch(float pitch)
    {
        Animator.SetFloat("AimPitch", pitch);
    }

    public void SetIsUnarmed(bool isUnarmed)
    {
        Animator.SetBool("IsUnarmed", isUnarmed);
    }

    public void SetIsPistolEquipped(bool isPistolEquipped)
    {
        Animator.SetBool("IsPistolEquipped", isPistolEquipped);
    }

    public void SetIsWalking(bool isWalking)
    {
        Animator.SetBool("IsWalking", isWalking);
    }

    public void SetIsRevolverEquipped(bool isRevolverEquipped)
    {
        Animator.SetBool("IsRevolverEquipped", isRevolverEquipped);
    }

    public void SetIsLocke17Equipped(bool isLocke17Equipped)
    {
        Animator.SetBool("IsLocke17Equipped", isLocke17Equipped);
    }

    public void TriggerShootAnimation(string shootTriggerName)
    {
        Animator.SetTrigger(shootTriggerName);
    }

    public void TriggerReloadAnimation(string reloadTriggerName)
    {
        Animator.SetTrigger(reloadTriggerName);
    }

    public void SetIsFacingWall(bool isWallDetected)
    {
        Animator.SetBool("IsFacingWall", isWallDetected);
    }

    public void SetIsOnStairs(bool isOnStairs)
    {
        Animator.SetBool("IsOnStairs", isOnStairs);
    }
}
