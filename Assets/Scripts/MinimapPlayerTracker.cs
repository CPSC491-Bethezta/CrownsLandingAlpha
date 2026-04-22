using UnityEngine;

public class MinimapPlayerTracker : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public RectTransform playerMarker;
    public RectTransform mapRect;

    [Header("World Bounds")]
    public float worldMinX = -50f;
    public float worldMaxX = 50f;
    public float worldMinZ = -50f;
    public float worldMaxZ = 50f;

    void Update()
    {
        UpdatePlayerMarker();
    }

    void UpdatePlayerMarker()
    {
        if (player == null || playerMarker == null || mapRect == null)
            return;

        float normalizedX = Mathf.InverseLerp(worldMinX, worldMaxX, player.position.x);
        float normalizedZ = Mathf.InverseLerp(worldMinZ, worldMaxZ, player.position.z);

        float mapWidth = mapRect.rect.width;
        float mapHeight = mapRect.rect.height;

        float markerX = (normalizedX * mapWidth) - (mapWidth / 2f);
        float markerY = (normalizedZ * mapHeight) - (mapHeight / 2f);

        playerMarker.anchoredPosition = new Vector2(markerX, markerY);

        float rotationY = player.eulerAngles.y;
        playerMarker.localRotation = Quaternion.Euler(0f, 0f, -rotationY);
    }
}