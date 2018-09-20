//reference : http://wiki.unity3d.com/index.php?title=FramesPerSecond

using UnityEngine;
using System.Collections;
 
public class FPSCheck : MonoBehaviour
{
    float accDeltaTime = 0.0f;
    int frameCount = 0;
    int printFrameCount = 0;



    void Update()
    {
        accDeltaTime += Time.deltaTime;
        frameCount += 1;

        if (accDeltaTime >= 1.0f)
        {
            printFrameCount = frameCount;
            frameCount = 0;
            accDeltaTime -= 1.0f;
        }
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;

        GUIStyle style = new GUIStyle();

        Rect rect = new Rect(0, 0, w, h * 2 / 100);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = h * 2 / 100;
        style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
        
        string text = string.Format("{0}fps", printFrameCount);
        GUI.Label(rect, text, style);
    }
}