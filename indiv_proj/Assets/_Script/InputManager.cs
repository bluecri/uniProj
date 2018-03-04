using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
    public static InputManager instance = null;

    public const int CAM_MODE_UPVIEW = 0;
    public const int CAM_MODE_CHARVIEW = 1;

    public static int s_cameraMode = CAM_MODE_UPVIEW;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
