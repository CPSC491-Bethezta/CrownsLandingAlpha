using UnityEngine;

public class HitDetection : MonoBehaviour
{
     [Header("Hitscan")]
    [SerializeField] private float range = 5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private LayerMask hitMask;

    [Header("AOE")]
    [SerializeField] private float aoeRadius = 3f;
    [SerializeField] private float aoeHitDelay = 0.15f;
public void Fire()
{
    // start ray at chest height instead of camera from the old script
    Vector3 origin = transform.position + Vector3.up * 1.0f;
    Vector3 direction = transform.forward;

if (Physics.Raycast(origin, direction, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
{
    // Don't hit yourself
    if (hit.transform.root == transform.root)
        return;

    if (hit.collider.GetComponentInParent<IDamageable>() is IDamageable target)
    {
        target.TakeDamage(damage);
    }
}

    // optional: visualize in Scene view - chat gpt
    Debug.DrawRay(origin, direction * range, Color.red, 0.1f);
}

public void StartAoe()
    {
        StartCoroutine(AoeDelayedHit());
    }
private System.Collections.IEnumerator AoeDelayedHit()
{
    yield return new WaitForSeconds(aoeHitDelay);

    Vector3 center = transform.position + Vector3.up * 1.0f;

    Collider[] hits = Physics.OverlapSphere(center, aoeRadius, hitMask, QueryTriggerInteraction.Ignore);

for (int i = 0; i < hits.Length; i++)
{
    if (hits[i].transform.root == transform.root)
        continue;

    if (hits[i].GetComponentInParent<IDamageable>() is IDamageable target)
    {
        target.TakeDamage(damage);
    }
}

}

}
