using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float lifetime = 0.7f;
    public float floatSpeed = 1.5f;

    private float timer;
    private TextMeshPro text;
    private Color startColor;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();

        startColor = text.color;
    }

    public void Setup(float damage)
    {
        Debug.Log("DamagePopup Setup " + damage);
        text.text = damage.ToString();
    }

    void Update()
    {
        timer += Time.deltaTime;

        // float up
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        // fade out
        float t = timer / lifetime;
        text.color = new Color(startColor.r, startColor.g, startColor.b, 1f - t);

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

