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

    public void OnLightAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        if (!inAttackStance)
            return;
        Debug.Log("PlayerCombatController: Light attack called.");
        animationController.LightLeftClick();
    }

    public void OnHeavyAttack(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        if (!inAttackStance)
            return;
        Debug.Log("PlayerCombatController: Heavy attack called.");
        animationController.HeavyLeftClick();
    }

    public void OnBlock(InputAction.CallbackContext ctx)
    {
        if (animationController == null)
            return;

        if (!inAttackStance)
            return;

        if (ctx.performed)
            animationController.RightClick();

        if (ctx.canceled)
            animationController.StopBlock();
    }

    public void OnStance(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        bool nextStance = !inAttackStance;

        if (nextStance)
        {
            var weapon = weaponObject.GetComponent<Weapon>();
            
            if (weapon != null)
                EquipWeapon(weapon);
        }
        weaponObject.SetActive(nextStance);
        SetAttackStance(nextStance);
    }

    public void EquipWeapon(Weapon weapon)
    {
        if (animationController == null || weapon == null || weapon.definition == null)
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
