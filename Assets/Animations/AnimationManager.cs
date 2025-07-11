using UnityEngine;

public class AnimationManager
{
    public Animator animator { get; private set; }
    private const float blendSpeed = 10f;

    public AnimationManager(Animator animator)
    {
        this.animator = animator;
    }

    public void PlayAnimation(string animationName, int layerIndex)
    {
        animator.Play(animationName, layerIndex, 0f);
    }


    public void StopAnimation()
    {
        animator.StopPlayback();
    }

    public bool IsAnimationPlaying(string animationName, int layerIndex)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
        return stateInfo.IsName(animationName) && stateInfo.normalizedTime < 1.0f;
    }

    public void SetIsIdle(bool isIdle)
    {
        animator.SetBool("IsIdle", isIdle);
    }

    public void SetMoveParams(float moveX, float moveZ)
    {
        animator.SetFloat("MoveX", moveX);
        animator.SetFloat("MoveZ", moveZ);
    }

    public void SetIsSprinting(bool isSprinting)
    {
        animator.SetBool("IsSprinting", isSprinting);
    }

    public void SetIsStrafing(bool isStrafing)
    {
        animator.SetBool("IsStrafing", isStrafing);
    }

    public void SetIsAiming(bool isAiming)
    {
        animator.SetBool("IsAiming", isAiming);
    }

    public void SetIsShooting(bool isShooting)
    {
        animator.SetBool("IsShooting", isShooting);
    }

    public void SetIsMoving(bool isMoving)
    {
        animator.SetBool("IsMoving", isMoving);
    }

    public void SetIsLeftFootPlanted(bool isLeftFootPlanted)
    {
        animator.SetBool("IsLeftFootPlanted", isLeftFootPlanted);
    }

    public void SetIsRightFootPlanted(bool isRightFootPlanted)
    {
        animator.SetBool("IsRightFootPlanted", isRightFootPlanted);
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
        animator.SetInteger("StopDirection", directionIndex);
    }

    public void SetMoveCommited(bool moveCommited)
    {
        animator.SetBool("MoveCommited", moveCommited);
    }

    public void SetLayerWeight(int layerIndex, float weight)
    {
        animator.SetLayerWeight(layerIndex, weight);
    }

    public void BlendLayerWeight(int layerIndex, float targetWeight)
    {
        float currentWeight = animator.GetLayerWeight(layerIndex);
        float newWeight = Mathf.MoveTowards(currentWeight, targetWeight, Time.deltaTime * blendSpeed);
        animator.SetLayerWeight(layerIndex, newWeight);
    }

    public void SetAlertState(bool isAlert)
    {
        animator.SetBool("IsAlert", isAlert);
    }

    public void SetTurnAngle(float angle)
    {
        animator.SetFloat("TurnAngle", angle);
    }

    public void SetIsAggro(bool isAggro)
    {
        animator.SetBool("IsAggro", isAggro);
    }

    public void SetTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public void ResetTrigger(string triggerName)
    {
        animator.ResetTrigger(triggerName);
    }

    public void SetIsTurning(bool isTurning)
    {
        animator.SetBool("IsTurning", isTurning);
    }

    public void SetIsDead(bool isDead)
    {
        animator.SetBool("IsDead", isDead);
    }

    public void SetIsAttacking(bool isAttacking)
    {
        animator.SetBool("IsAttacking", isAttacking);
    }

    public void SetIsInAttackRange(bool isInAttackRange)
    {
        animator.SetBool("IsInAttackRange", isInAttackRange);
    }

    public bool SetHasAgroAnimationFinished(bool hasFinished)
    {
        animator.SetBool("HasAggroAnimFinished", hasFinished);
        return hasFinished;
    }
}
