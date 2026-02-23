using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void EquipWeapon(WeaponDefinition weapon)
    {
        animator.runtimeAnimatorController = weapon.overrideController;
    }

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
}
