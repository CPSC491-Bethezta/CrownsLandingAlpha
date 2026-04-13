using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        transform.LookAt(transform.position + cam.transform.forward);
    }
}
