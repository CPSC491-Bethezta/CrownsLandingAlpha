using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private GameObject weaponObject;
    private bool inAttackStance;

    private void Awake()
    {
        if (!animationController)
            animationController = GetComponent<PlayerAnimationController>();
    }

    public void OnPrimaryAction(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        if (!inAttackStance)
            return;
        animationController.PrimaryTrigger();
    }

    public void OnPrimaryHeld(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        if (!inAttackStance)
            return;
        animationController.PrimaryHoldTrigger();
    }

    public void OnSecondaryAction(InputAction.CallbackContext ctx)
    {
        if (animationController == null)
            return;

        if (!inAttackStance)
            return;

        if (ctx.performed)
            animationController.SecondaryTrigger();

        if (ctx.canceled)
            animationController.EndSecondaryTrigger();
    }

    public void OnStance(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        bool nextStance = !inAttackStance;

        if (nextStance)
        {
            Weapon weapon = null;
            if (weaponObject != null)
                weapon = weaponObject.GetComponent<Weapon>();

            if (weapon != null)
                EquipWeapon(weapon);
        }

        if (weaponObject != null)
            weaponObject.SetActive(nextStance);
        SetAttackStance(nextStance);
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (animationController == null || weapon == null || weapon.definition == null)
            return;
        if (weapon.definition.overrideController == null)
            return;
        animationController.SetOverrideController(weapon.definition.overrideController);
    }

    private void SetAttackStance(bool enabled)
    {
        inAttackStance = enabled;
        if (animationController != null)
            animationController.SetAttackStance(enabled);
    }

}
