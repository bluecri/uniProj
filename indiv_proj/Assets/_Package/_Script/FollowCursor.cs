using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCursor : MonoBehaviour {
    private RectTransform rect = null;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        //start follow cursor
        StartCoroutine(FollowCursorCoroutine());
    }

    private void OnDisable()
    {
        StopCoroutine(FollowCursorCoroutine());
    }

    IEnumerator FollowCursorCoroutine()
    {
        while (true)
        {
            // ref : https://sushanta1991.blogspot.kr/2016/04/how-to-move-unity-ui-using-mouse.html
            float width = Screen.width * rect.anchorMin.x;
            float height = Screen.height * rect.anchorMin.y;

            float xoffset = 0;
            float yoffset = 0;

            if (Screen.width > 1024)
            {
                float difference = Screen.width - 1024;
                float percentage = (Input.mousePosition.x / (float)Screen.width) * 100;
                xoffset = (percentage * difference) / 100.0f;
            }
            if (Screen.height > 768)
            {
                float difference = Screen.height - 768;
                float percentage = ((float)(Screen.height - Input.mousePosition.y) / (float)Screen.height) * 100;
                yoffset = (percentage * difference) / 100.0f;
            }

            if (Screen.width < 1024)
            {
                float difference = 1024 - Screen.width;
                float percentage = (Input.mousePosition.x / (float)Screen.width) * 100;
                xoffset = -(percentage * difference) / 100.0f;
            }

            if (Screen.height < 768)
            {
                float difference = 768 - Screen.height;
                float percentage = ((float)(Screen.height - Input.mousePosition.y) / (float)Screen.height) * 100;
                yoffset = -(percentage * difference) / 100.0f;
            }

            rect.anchoredPosition = new Vector2(Input.mousePosition.x - width - xoffset, Input.mousePosition.y - height + yoffset);
            
            rect.anchoredPosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            yield return null;
        }
    }
}
