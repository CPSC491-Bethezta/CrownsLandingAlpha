using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;


    // Swords: Light Attack, Bows: Shoot,
    public void PrimaryTrigger()
    {
        animator.SetTrigger("PrimaryTrigger");
    }

    // Swords: Heavy Attack, Bows: Nothing
    public void PrimaryHoldTrigger()
    {
        animator.SetTrigger("PrimaryHoldTrigger");
    }

    // Swords: Block, Bows: Aim, Shield: Block
    public void SecondaryTrigger()
    {
        animator.SetBool("IsSecondary", true);
    }

    public void EndSecondaryTrigger()
    {
        animator.SetBool("IsSecondary", false);
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

    public void UseItemTrigger()
    {
        animator.SetTrigger("use_item");
    }

    public void InteractTrigger()
    {
        animator.SetTrigger("Interact");
    }
}
