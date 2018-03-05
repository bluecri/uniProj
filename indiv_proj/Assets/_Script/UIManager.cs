using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager instance = null;

    public const int UI_GAME_VIEW = 0;
    public const int UI_MENU_VIEW = 1;
    public const int UI_INVEN_VIEW = 2;
    public const int UI_CHEST_VIEW = 3;

    public static int s_uiStatus = UI_GAME_VIEW;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    //change UI & change input Manager input corutine
    void ChangeUIStatus(int mode)
    {
        s_uiStatus = mode;
        InputManager.instance.ChangeMainInputCorutine(s_uiStatus);
    }
    
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
