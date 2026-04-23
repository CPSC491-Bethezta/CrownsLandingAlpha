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
    private bool usingMainSlot0;

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

    private void Update()
    {
        CheckEquipmentSlotHotkeys();
        ValidateCurrentEquippedStillExists();

        if (inAttackStance && usingMainSlot0)
            CheckMainSlot0Changed();
    }

    // -------- Input callbacks --------

    /// <summary>Wired to the Attack action via PlayerInput Unity Events.</summary>
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

        // itemType is the authoritative branch — avoids false negatives from
        // component lookups on inactive children or mis-configured prefabs.
        if (currentEquippedItem.itemType == InventoryItemType.Weapon)
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

        // Consumable / potion path
        lastPrimaryTime = Time.time;

        if (animator != null)
            animator.SetTrigger(useItemTriggerName);

        StartCoroutine(ConsumeAfterDelay());
    }

    /// <summary>Wired to a hold variant of the Attack action via PlayerInput Unity Events.</summary>
    public void OnPrimaryHeld(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;
        if (!inAttackStance)
            return;
        animationController.PrimaryHoldTrigger();
    }

    /// <summary>Wired to the Slam action via PlayerInput Unity Events.</summary>
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

    /// <summary>Wired to the Stance action via PlayerInput Unity Events.</summary>
    public void OnStance(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        bool nextStance = !inAttackStance;

        if (nextStance)
        {
            RefreshEquippedFromMainSlot0();

            if (currentEquippedItem == null || currentEquippedVisual == null)
                return;

            ApplyWeaponOverride(currentEquippedVisual);

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

    // -------- Override controller --------

    /// <summary>Applies the weapon's AnimatorOverrideController to the Animator.</summary>
    public void ApplyWeaponOverride(GameObject visual)
    {
        if (animationController == null || visual == null)
            return;

        Weapon weapon = visual.GetComponent<Weapon>();
        if (weapon == null)
            weapon = visual.GetComponentInChildren<Weapon>(true);

        if (weapon == null)
        {
            Debug.LogWarning($"[PlayerCombatController] No Weapon component on '{visual.name}'");
            return;
        }

        if (weapon.definition == null)
        {
            Debug.LogWarning($"[PlayerCombatController] Weapon.definition is null on '{visual.name}'");
            return;
        }

        if (weapon.definition.overrideController == null)
        {
            Debug.LogWarning($"[PlayerCombatController] overrideController is null on definition '{weapon.definition.name}'");
            return;
        }

        animationController.SetOverrideController(weapon.definition.overrideController);
    }

    // -------- Stance --------

    private void SetAttackStance(bool enabled)
    {
        inAttackStance = enabled;

        if (animationController != null)
            animationController.SetAttackStance(enabled);
    }

    // -------- Equipped item management --------

    private void CheckMainSlot0Changed()
    {
        if (InventoryManager.Instance == null)
            return;

        InventoryItem slot0Item = InventoryManager.Instance.GetItemAtSlot(0);
        if (slot0Item == currentEquippedItem)
            return;

        Debug.Log($"[PlayerCombatController] Slot 0 changed: '{currentEquippedItem?.itemName}' → '{slot0Item?.itemName}'");
        RefreshEquippedFromMainSlot0();
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

        if (usingMainSlot0 && slot0Item == currentEquippedItem && currentEquippedVisual != null)
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

        ApplyWeaponOverride(currentEquippedVisual);

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

        ApplyWeaponOverride(currentEquippedVisual);

        hitDetection = null;
        warnedMissingHitDetection = false;
        ResolveHitDetection();

        inAttackStance = true;
        SetAttackStance(true);
    }

    private void ValidateCurrentEquippedStillExists()
    {
        if (InventoryManager.Instance == null)
            return;

        if (usingMainSlot0)
        {
            if (InventoryManager.Instance.GetItemAtSlot(0) == null)
                ClearEquippedItemState();
        }
        else if (selectedEquipmentSlot >= 0)
        {
            if (InventoryManager.Instance.GetEquipmentItemAtSlot(selectedEquipmentSlot) == null)
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

    // -------- Equipment slot hotkeys --------

    private void CheckEquipmentSlotHotkeys()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            RefreshEquippedFromEquipmentSlot(0);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            RefreshEquippedFromEquipmentSlot(1);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            RefreshEquippedFromEquipmentSlot(2);

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
            RefreshEquippedFromEquipmentSlot(3);
    }

    // -------- Hit detection --------

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
        Debug.LogWarning("[PlayerCombatController] No HitDetection component found.");
    }

    private IEnumerator DelayedPrimaryHit()
    {
        yield return new WaitForSeconds(primaryHitDelay);

        if (hitDetection != null)
            hitDetection.Fire();
    }

    // -------- Consumable --------

    private IEnumerator ConsumeAfterDelay()
    {
        yield return new WaitForSeconds(1.25f);
        ConsumeCurrentItem();
    }

private void ConsumeCurrentItem()
{
    if (currentEquippedItem == null)
        return;

    // Check if it's a weapon (don't consume)
    Weapon weapon = null;
    if (currentEquippedVisual != null)
    {
        weapon = currentEquippedVisual.GetComponent<Weapon>();
        if (weapon == null)
            weapon = currentEquippedVisual.GetComponentInChildren<Weapon>(true);
    }

    if (weapon != null)
        return;

    StatsProfile stats = GetComponent<StatsProfile>();
    if (stats != null)
    {
        stats.Heal(currentEquippedItem.HealAmount);
    }

    if (InventoryManager.Instance != null && selectedEquipmentSlot >= 0)
        InventoryManager.Instance.RemoveEquipmentItemAtSlot(selectedEquipmentSlot);

    ClearEquippedItemState();
}

    // -------- Misc public triggers --------

    public void UseItemTrigger()
    {
        if (animator != null)
            animator.SetTrigger("use_item");
    }

    public void InteractTrigger()
    {
        if (animator != null)
            animator.SetTrigger("Interact");
    }

    private void AddStartingWeaponToInventory()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[PlayerCombatController] No InventoryManager in scene.");
            return;
        }

        if (startingItem == null)
        {
            Debug.LogWarning("[PlayerCombatController] No startingItem assigned.");
            return;
        }

        InventoryManager.Instance.AddItem(startingItem);
    }
}
