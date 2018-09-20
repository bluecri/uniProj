using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamMove : MonoBehaviour {
    public const int height = 100;
    private void LateUpdate()
    {
        Vector3 playerPos = SceneManager.playerInstance.transform.position;
        Vector3 targetPos = playerPos;
        targetPos.y = height;

        transform.position = targetPos;
    }
}
