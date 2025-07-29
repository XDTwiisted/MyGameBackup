using UnityEngine;

public class ScrollingForeground : MonoBehaviour
{
    public float scrollSpeed = 1f;
    public float tileWidth = 19.2f;
    public bool isScrolling = false;
    public bool reverseDirection = false;

    private Transform[] tiles;

    private void Start()
    {
        tiles = new Transform[transform.childCount];
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = transform.GetChild(i);
        }
    }

    private void Update()
    {
        if (!isScrolling) return;

        float direction = reverseDirection ? 1f : -1f;

        foreach (Transform tile in tiles)
        {
            tile.position += Vector3.right * direction * scrollSpeed * Time.deltaTime;

            if (!reverseDirection && tile.position.x <= -tileWidth)
            {
                float rightmostX = GetRightmostTileX();
                tile.position = new Vector3(rightmostX + tileWidth, tile.position.y, tile.position.z);
            }
            else if (reverseDirection && tile.position.x >= tileWidth)
            {
                float leftmostX = GetLeftmostTileX();
                tile.position = new Vector3(leftmostX - tileWidth, tile.position.y, tile.position.z);
            }
        }
    }

    private float GetRightmostTileX()
    {
        float maxX = float.MinValue;
        foreach (Transform tile in tiles)
        {
            if (tile.position.x > maxX)
                maxX = tile.position.x;
        }
        return maxX;
    }

    private float GetLeftmostTileX()
    {
        float minX = float.MaxValue;
        foreach (Transform tile in tiles)
        {
            if (tile.position.x < minX)
                minX = tile.position.x;
        }
        return minX;
    }

    public void SetDirection(bool reverse)
    {
        reverseDirection = reverse;
    }
}
