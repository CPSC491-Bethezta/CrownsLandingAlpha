using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private DamagePopup damagePopupPrefab;
    public float hp = 1000f;
    public void TakeDamage(float d)
    {
        hp -= d;
        DamagePopup popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up, Quaternion.identity);
        popup.Setup(d);
        Debug.Log("fire successful");
        if (hp <= 0)
        {
            //temp for dummy
           hp = 1000;
        }
    }
}
