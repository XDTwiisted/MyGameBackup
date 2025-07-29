using UnityEngine;
using UnityEngine.UI;

public class TimelineScroller : MonoBehaviour
{
    public RawImage rawImage;
    public float cycleDurationSeconds = 12f * 3600f; // Full day/night cycle in real seconds

    private float startTime;

    void Start()
    {
        if (rawImage == null)
            rawImage = GetComponent<RawImage>();

        startTime = Time.unscaledTime;

        // Show 6 hours worth of the 12-hour texture (50%)
        rawImage.uvRect = new Rect(0, 0, 0.5f, 1);
    }

    void Update()
    {
        if (rawImage == null || rawImage.texture == null) return;

        float elapsed = Time.unscaledTime - startTime;

        // Percentage of the 12-hour cycle completed
        float scrollPercent = (elapsed % cycleDurationSeconds) / cycleDurationSeconds;

        // Shift UV rect leftward (moving image to the left)
        float uvStartX = scrollPercent;

        rawImage.uvRect = new Rect(uvStartX, 0, 0.5f, 1);
    }
}
