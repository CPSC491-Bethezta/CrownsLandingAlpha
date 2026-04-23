using UnityEngine;

public class CameraPan : MonoBehaviour
{
    public float panSpeed = 0.5f;
    public float panDistance = 10f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * panSpeed) * panDistance;
        transform.position = new Vector3(
            startPos.x + offset,
            startPos.y,
            startPos.z
        );
    }
}