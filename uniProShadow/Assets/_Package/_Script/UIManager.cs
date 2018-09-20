using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public static UIManager instance = null;

    public GameObject chestInventoryUI;
    public GameObject mainInventoryUI;
    public GameObject fastInventoryUI;
    public GameObject menuUI;
    
    public const int UI_GAME_VIEW = 0;
    public const int UI_MENU_VIEW = 1;
    public const int UI_INVEN_VIEW = 2;
    public const int UI_CHEST_VIEW = 3;

    public static int s_uiStatus = UI_GAME_VIEW;

    [SerializeField]
    public GameObject cursorSlot;

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
    
    public void CursorEnable(bool tf)
    {
        Cursor.visible = tf;
        cursorSlot.SetActive(tf);
    }
}
