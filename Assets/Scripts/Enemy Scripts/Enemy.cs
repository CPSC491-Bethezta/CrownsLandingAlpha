using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private DamagePopup damagePopupPrefab;

    private SkeletonBehavior skeleton;

    private void Awake()
    {
        skeleton = GetComponentInParent<SkeletonBehavior>();

        if (skeleton == null)
        {
            Debug.LogWarning($"Enemy script on {name} but no SkeletonBehavior found in parent hierarchy.");
        }
    }

    public void TakeDamage(float d)
    {
        // Popup
        if (damagePopupPrefab != null)
        {
            DamagePopup popup = Instantiate(
                damagePopupPrefab,
                transform.position + Vector3.up,
                Quaternion.identity
            );
            popup.Setup(d);
        }

        Debug.Log("fire successful");

        if (skeleton != null)
        {
            skeleton.TakeDamage(d);
        }
    }
}