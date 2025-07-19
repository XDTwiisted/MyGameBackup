using UnityEngine;
using TMPro;
using System;

public class RealTimeClock : MonoBehaviour
{
    public TextMeshProUGUI clockText;

    void Update()
    {
        DateTime now = DateTime.Now;
        string timeString = now.ToString("hh:mm tt"); // 12-hour format with AM/PM
        clockText.text = timeString;
    }
}
