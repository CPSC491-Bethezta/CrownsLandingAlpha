using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationController animationController;
    [SerializeField] private GameObject weaponObject;
    [SerializeField] private HitDetection hitDetection;

    [Header("Inventory")]
    [SerializeField] private InventoryItem startingItem;
    [SerializeField] private Transform weaponHoldPoint;
    [SerializeField] private Vector3 equippedLocalPosition;
    [SerializeField] private Vector3 equippedLocalRotation;
    [SerializeField] private string useItemTriggerName = "use_item";

    [Header("Timing")]
    [SerializeField] private float primaryHitDelay = 0.35f;
    [SerializeField] private float primaryCooldown = 0.5f;
    [SerializeField] private float secondaryCooldown = 1.25f;
    [SerializeField] private bool secondaryUsesAoe = true;

    private bool inAttackStance;
    private float lastPrimaryTime = -999f;
    private float lastSecondaryTime = -999f;
    private bool warnedMissingHitDetection;

    private int selectedEquipmentSlot = -1;
    private bool usingMainSlot0 = false;

    private GameObject currentEquippedVisual;
    private InventoryItem currentEquippedItem;
    private Animator animator;

    private void Awake()
    {
        if (!animationController)
            animationController = GetComponent<PlayerAnimationController>();

        animator = GetComponent<Animator>();
        ResolveHitDetection();
    }

    private void Start()
    {
        AddStartingWeaponToInventory();
        RefreshEquippedFromMainSlot0();
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

        RefreshCurrentEquippedItem();

        if (currentEquippedItem == null || currentEquippedVisual == null)
            return;

        Weapon weapon = currentEquippedVisual.GetComponent<Weapon>();
        if (weapon == null)
            weapon = currentEquippedVisual.GetComponentInChildren<Weapon>(true);

        // Actual weapon object = attack animation
        if (weapon != null)
        {
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

            return;
        }

        // Otherwise treat it as a consumable/potion
        lastPrimaryTime = Time.time;

        if (animator != null)
            animator.SetTrigger(useItemTriggerName);

        StartCoroutine(ConsumeAfterDelay());
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

        RefreshCurrentEquippedItem();

        if (currentEquippedItem == null || currentEquippedVisual == null)
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

        // Entering stance should ALWAYS come from main inventory slot 0
        if (nextStance)
        {
            RefreshEquippedFromMainSlot0();

            if (currentEquippedItem == null || currentEquippedVisual == null)
                return;

            Weapon weapon = null;

            if (weaponObject != null)
                weapon = weaponObject.GetComponent<Weapon>();

            if (weapon == null && currentEquippedVisual != null)
                weapon = currentEquippedVisual.GetComponentInChildren<Weapon>(true);

            if (weapon != null)
                EquipWeapon(weapon);

            hitDetection = null;
            warnedMissingHitDetection = false;
            ResolveHitDetection();
        }

        if (weaponObject != null)
            weaponObject.SetActive(nextStance);

        if (currentEquippedVisual != null)
            currentEquippedVisual.SetActive(nextStance);

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

    private void Update()
    {
        CheckEquipmentSlotHotkeys();
        ValidateCurrentEquippedStillExists();
    }

    private void AddStartingWeaponToInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("No InventoryManager found in scene.");
            return;
        }

        if (startingItem == null)
        {
            Debug.LogWarning("No startingItem assigned in PlayerCombatController.");
            return;
        }

        InventoryManager.Instance.AddItem(startingItem);
    }

    private void RefreshCurrentEquippedItem()
    {
        if (usingMainSlot0)
            RefreshEquippedFromMainSlot0();
        else if (selectedEquipmentSlot >= 0)
            RefreshEquippedFromEquipmentSlot(selectedEquipmentSlot);
    }

    private void RefreshEquippedFromMainSlot0()
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryItem slot0Item = InventoryManager.Instance.GetItemAtSlot(0);

        if (slot0Item == null || slot0Item.equippedObject == null)
        {
            ClearEquippedItemState();
            return;
        }

        if (usingMainSlot0 &&
            slot0Item == currentEquippedItem &&
            currentEquippedVisual != null)
            return;

        if (currentEquippedVisual != null)
            Destroy(currentEquippedVisual);

        currentEquippedVisual = null;
        currentEquippedItem = slot0Item;
        usingMainSlot0 = true;
        selectedEquipmentSlot = -1;

        Transform parent = weaponHoldPoint != null ? weaponHoldPoint : transform;

        currentEquippedVisual = Instantiate(slot0Item.equippedObject, parent);
        currentEquippedVisual.transform.localPosition = equippedLocalPosition;
        currentEquippedVisual.transform.localRotation = Quaternion.Euler(equippedLocalRotation);
        currentEquippedVisual.transform.localScale = Vector3.one;
        currentEquippedVisual.SetActive(inAttackStance);

        weaponObject = currentEquippedVisual;

        hitDetection = null;
        warnedMissingHitDetection = false;
        ResolveHitDetection();
    }

    private void RefreshEquippedFromEquipmentSlot(int slotIndex)
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryItem selectedItem = InventoryManager.Instance.GetEquipmentItemAtSlot(slotIndex);

        if (selectedItem == null || selectedItem.equippedObject == null)
        {
            ClearEquippedItemState();
            return;
        }

        if (!usingMainSlot0 &&
            selectedEquipmentSlot == slotIndex &&
            selectedItem == currentEquippedItem &&
            currentEquippedVisual != null)
            return;

        if (currentEquippedVisual != null)
            Destroy(currentEquippedVisual);

        currentEquippedVisual = null;
        currentEquippedItem = selectedItem;
        usingMainSlot0 = false;
        selectedEquipmentSlot = slotIndex;

        Transform parent = weaponHoldPoint != null ? weaponHoldPoint : transform;

        currentEquippedVisual = Instantiate(selectedItem.equippedObject, parent);
        currentEquippedVisual.transform.localPosition = equippedLocalPosition;
        currentEquippedVisual.transform.localRotation = Quaternion.Euler(equippedLocalRotation);
        currentEquippedVisual.transform.localScale = Vector3.one;
        currentEquippedVisual.SetActive(true);

        weaponObject = currentEquippedVisual;

        hitDetection = null;
        warnedMissingHitDetection = false;
        ResolveHitDetection();

        // hotkeys are immediate equip/use stance on
        inAttackStance = true;
        SetAttackStance(true);
    }

    private void ValidateCurrentEquippedStillExists()
    {
        if (InventoryManager.Instance == null)
            return;

        if (usingMainSlot0)
        {
            InventoryItem slot0Item = InventoryManager.Instance.GetItemAtSlot(0);
            if (slot0Item == null)
                ClearEquippedItemState();
        }
        else if (selectedEquipmentSlot >= 0)
        {
            InventoryItem equipmentItem = InventoryManager.Instance.GetEquipmentItemAtSlot(selectedEquipmentSlot);
            if (equipmentItem == null)
                ClearEquippedItemState();
        }
    }

    private void ClearEquippedItemState()
    {
        if (currentEquippedVisual != null)
            Destroy(currentEquippedVisual);

        currentEquippedVisual = null;
        currentEquippedItem = null;
        weaponObject = null;
        hitDetection = null;
        warnedMissingHitDetection = false;
        usingMainSlot0 = false;
        selectedEquipmentSlot = -1;

        if (inAttackStance)
            SetAttackStance(false);
    }

    private void EquipItemFromEquipmentSlot(int slotIndex)
    {
        RefreshEquippedFromEquipmentSlot(slotIndex);
    }

    private void CheckEquipmentSlotHotkeys()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            EquipItemFromEquipmentSlot(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            EquipItemFromEquipmentSlot(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            EquipItemFromEquipmentSlot(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            EquipItemFromEquipmentSlot(3);
    }

    private IEnumerator ConsumeAfterDelay()
    {
        yield return new WaitForSeconds(1.25f);
        ConsumeCurrentItem();
    }

    private void ConsumeCurrentItem()
    {
        if (currentEquippedItem == null)
            return;

        Weapon weapon = null;
        if (currentEquippedVisual != null)
        {
            weapon = currentEquippedVisual.GetComponent<Weapon>();
            if (weapon == null)
                weapon = currentEquippedVisual.GetComponentInChildren<Weapon>(true);
        }

        // never consume weapons
        if (weapon != null)
            return;

        // potions live in equipment slots only
        if (InventoryManager.Instance != null && !usingMainSlot0 && selectedEquipmentSlot >= 0)
            InventoryManager.Instance.RemoveEquipmentItemAtSlot(selectedEquipmentSlot);

        ClearEquippedItemState();
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