using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private GameObject weaponObject;
    [SerializeField] private HitDetection hitDetection;

    [Header("Timing")]
    [SerializeField] private float primaryHitDelay = 0.35f;
    [SerializeField] private float primaryCooldown = 0.5f;
    [SerializeField] private float secondaryCooldown = 1.25f;
    [SerializeField] private bool secondaryUsesAoe = true;

    private bool inAttackStance;
    private float lastPrimaryTime = -999f;
    private float lastSecondaryTime = -999f;
    private bool warnedMissingHitDetection;

    private void Awake()
    {
        if (!animationController)
            animationController = GetComponent<PlayerAnimationController>();
        ResolveHitDetection();
    }

    public void OnPrimaryAction(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        if (!inAttackStance)
            return;
        if (animationController == null)
            return;
        if (Time.time < lastPrimaryTime + primaryCooldown)
            return;

        ResolveHitDetection();
        if (hitDetection == null)
        {
            WarnMissingHitDetection();
            return;
        }

        lastPrimaryTime = Time.time;
        animationController.PrimaryTrigger();

        if (primaryHitDelay <= 0f)
            hitDetection.Fire();
        else
            StartCoroutine(DelayedPrimaryHit());
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
        {
            animationController.SecondaryTrigger();
            if (!secondaryUsesAoe)
                return;
            if (Time.time < lastSecondaryTime + secondaryCooldown)
                return;

            ResolveHitDetection();
            if (hitDetection == null)
            {
                WarnMissingHitDetection();
                return;
            }

            lastSecondaryTime = Time.time;
            hitDetection.StartAoe();
        }

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

            // Weapon can change hit source; resolve again on stance entry.
            ResolveHitDetection();
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

    private IEnumerator DelayedPrimaryHit()
    {
        yield return new WaitForSeconds(primaryHitDelay);
        if (hitDetection != null)
            hitDetection.Fire();
    }

    private void ResolveHitDetection()
    {
        if (hitDetection != null)
            return;

        hitDetection = GetComponent<HitDetection>();
        if (hitDetection != null)
            return;

        if (weaponObject != null)
        {
            hitDetection = weaponObject.GetComponent<HitDetection>();
            if (hitDetection == null)
                hitDetection = weaponObject.GetComponentInChildren<HitDetection>(true);
            if (hitDetection != null)
                return;
        }

        hitDetection = GetComponentInChildren<HitDetection>(true);
    }

    private void WarnMissingHitDetection()
    {
        if (warnedMissingHitDetection)
            return;
        warnedMissingHitDetection = true;
        Debug.LogWarning("PlayerCombatController could not find a HitDetection component.");
    }

}
