using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;


    // Swords: Light Attack, Bows: Shoot,
    public void LightLeftClick()
    {
        animator.SetTrigger("LightAttack");
    }

    // Swords: Heavy Attack, Bows: Nothing
    public void HeavyLeftClick()
    {
        animator.SetTrigger("HeavyAttack");
    }

    // Swords: Block, Bows: Aim, Shield: Block
    public void RightClick()
    {
        animator.SetBool("IsBlocking", true);
    }

    public void StopBlock()
    {
        animator.SetBool("IsBlocking", false);
    }

    public void SetAttackStance(bool inStance)
    {
        animator.SetTrigger("TriggerAttackStance");
        animator.SetBool("AttackStance", inStance);
    }

    public void SetOverrideController(AnimatorOverrideController overrideController)
    {
        Debug.Log("Overriding.");
        animator.runtimeAnimatorController = overrideController;
    }
}
