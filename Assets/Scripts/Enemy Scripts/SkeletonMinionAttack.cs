using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]

public class SkeletonMinionAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float engageRange = 2.5f;
    [SerializeField] private float hitForwardOffset = 1.3f; 
    [SerializeField] private float hitHeight = 1.0f; 
    [SerializeField] private float hitRadius = 0.8f;
    [SerializeField] private float attackCooldown = 1.2f;
    [SerializeField] private float windupTime = 0.35f;

    [SerializeField] private LayerMask playerMask;

    [Header("Animation")]
    [SerializeField] private string attackTrigger = "Attack";

    private Animator animator;
    private float nextAttackTime;
    private bool isAttacking;

    private SkeletonBehavior brain;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        brain = GetComponent<SkeletonBehavior>();
    }

    private void Update()
    {
        if (brain == null) return;
        if (Time.time < nextAttackTime) return;
        if (isAttacking) return;

        if (PlayerControllerHub.Instance == null) return;

        Vector3 toPlayer = PlayerControllerHub.Instance.transform.position - transform.position;
        toPlayer.y = 0;

        if (toPlayer.magnitude <= engageRange)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        brain.PauseMovement();
        animator.SetTrigger(attackTrigger);

        yield return new WaitForSeconds(windupTime);
        DoHit();

        yield return new WaitForSeconds(0.1f);
        brain.ResumeChase();
        isAttacking = false;
    }

    private void DoHit()
    {
        Vector3 center = transform.position
               + transform.forward * hitForwardOffset
                + Vector3.up * hitHeight;

        int overlapMask = playerMask.value == 0 ? ~0 : playerMask.value;
        Collider[] hits = Physics.OverlapSphere(center, hitRadius, overlapMask, QueryTriggerInteraction.Ignore);
        Debug.Log($"[Skeleton] OverlapSphere hits: {hits.Length}");
        Debug.DrawLine(transform.position + Vector3.up * hitHeight, center, Color.yellow, 0.25f);
        foreach (var h in hits)
        {
            Debug.Log($"[Skeleton] Hit collider: {h.name} (layer {LayerMask.LayerToName(h.gameObject.layer)})");
            if (h.transform.root == transform.root)
            {
                continue;
            }

            if (h.GetComponentInParent<IDamageable>() is IDamageable target)
            {
                Debug.Log("[Skeleton] Damageable target found -> applying damage");
                target.TakeDamage(damage);
                break;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = transform.position + transform.forward * engageRange * 0.6f + Vector3.up * 1.0f;
        Gizmos.DrawWireSphere(center, hitRadius);
    }
#endif
}
