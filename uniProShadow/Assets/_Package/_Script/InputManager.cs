using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //awake param
    public static InputManager instance = null;

    public const int CAM_MODE_UPVIEW = 0;
    public const int CAM_MODE_CHARVIEW = 1;

    public static int s_cameraMode = CAM_MODE_UPVIEW;

    public IEnumerator mainInputCorutine = null;

    public bool inStateChanged = false;

    public GameObject cursorObject;

    private Player playerInstance;
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

        playerInstance = SceneManager.playerInstance;
    }
    //start param


    private void OnEnable()
    {
        StartCoroutine(MainInputCoroutine());
    }

    public void SetNewInputState(int state)
    {
        UIManager.s_uiStatus = state;
        inStateChanged = true;
    }

    IEnumerator MainInputCoroutine()
    {
        while (Application.isPlaying)
        {
            inStateChanged = false;
            switch (UIManager.s_uiStatus)
            {
                case UIManager.UI_GAME_VIEW:
                    yield return StartCoroutine(InputGameViewCorutine());
                    break;
                case UIManager.UI_INVEN_VIEW:
                    yield return StartCoroutine(InputInvenViewCorutine());
                    break;
                case UIManager.UI_CHEST_VIEW:
                    yield return StartCoroutine(InputChestViewCorutine());
                    break;
                case UIManager.UI_MENU_VIEW:
                    yield return StartCoroutine(InputMenuViewCorutine());
                    break;
            }
        }
    }

    IEnumerator InputGameViewCorutine()
    {
        UIManager.instance.CursorEnable(false);
        do
        {
            yield return null;

            if (inStateChanged)
            {
                break;
            }
            //horizontal : charactor rotation
            //vertical : camera move
            float mouseVerticalMovement = Input.GetAxis("Mouse Y");
            switch (s_cameraMode)
            {
                //mouse movement rotation
                case CAM_MODE_UPVIEW:
                    playerInstance.mouseHorizontalMovement = Input.GetAxis("Mouse X");
                    break;
                case CAM_MODE_CHARVIEW:
                    playerInstance.mouseHorizontalMovement = Input.GetAxis("Mouse X");
                    playerInstance.mouseVerticalMovement = Input.GetAxis("Mouse Y");
                    break;
            }
            //mouse
            if (Input.GetMouseButtonDown(0))
            {
                //left key down
                playerInstance.leftMouseKeyDown = true;
            }
            if (Input.GetMouseButton(0))
            {
                //left key down
                playerInstance.leftMouseKey = true;
            }
            else
            {
                playerInstance.leftMouseKey = false;
            }

            if (Input.GetMouseButtonUp(0))
            {
                //left key down
                playerInstance.leftMouseKeyUp = true;
            }

            if (Input.GetMouseButtonDown(1))
            {
                //right key down
                playerInstance.rightMouseKeyDown = true;
            }
            if (Input.GetMouseButton(1))
            {
                //right key down
                playerInstance.rightMouseKey = true;
            }
            else
            {
                playerInstance.rightMouseKey = false;
            }
            if (Input.GetMouseButtonUp(1))
            {
                //right key down
                playerInstance.rightMouseKeyup = true;
            }

            //move
            if (Input.GetKey(KeyCode.A))
            {
                //up
                playerInstance.leftKey = true;
            }
            else
            {
                playerInstance.leftKey = false;
            }

            if (Input.GetKey(KeyCode.D))
            {
                //up
                playerInstance.rightKey = true;
            }
            else
            {
                playerInstance.rightKey = false;
            }

            if (Input.GetKey(KeyCode.W))
            {
                //up
                playerInstance.upKey = true;
            }
            else
            {
                playerInstance.upKey = false;
            }

            if (Input.GetKey(KeyCode.S))
            {
                //down
                playerInstance.downKey = true;
            }
            else
            {
                playerInstance.downKey = false;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                //RUN
                playerInstance.runKey = true;
            }
            else
            {
                playerInstance.runKey = false;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                //jump
                playerInstance.jumpKey = true;
            }
            else
            {
                playerInstance.jumpKey = false;
            }

            if (Input.GetKey(KeyCode.F))
            {
                //jump
                playerInstance.slideKey = true;
            }
            else
            {
                playerInstance.slideKey = false;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                //change camera
                if (s_cameraMode == CAM_MODE_UPVIEW)
                {
                    s_cameraMode = CAM_MODE_CHARVIEW;
                    SceneManager.playerInstance.ChangePlayerCameraToCharViewFromInputManager();
                }
                else
                {
                    s_cameraMode = CAM_MODE_UPVIEW;
                    SceneManager.playerInstance.ChangePlayerCameraToUpViewFromInputManager();
                }
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                playerInstance.inventoryKey = true;
            }

            InputNumProcess();  //number key process
        } while (!inStateChanged);

        //init input to Player!
        playerInstance.mouseHorizontalMovement = 0.0f;
        playerInstance.mouseHorizontalMovement = 0.0f;
        playerInstance.mouseVerticalMovement = 0.0f;
        playerInstance.leftMouseKey = false;
        playerInstance.rightMouseKey = false;
        playerInstance.leftKey = false;
        playerInstance.rightKey = false;
        playerInstance.upKey = false;
        playerInstance.downKey = false;
        playerInstance.jumpKey = false;
        playerInstance.slideKey = false;
        playerInstance.runKey = false;
        playerInstance.inventoryKey = false;
    }

    IEnumerator InputInvenViewCorutine()
    {
        UIManager.instance.mainInventoryUI.SetActive(true);
        UIManager.instance.CursorEnable(true);
        do
        {
            yield return null;
            if (inStateChanged) { break; };

            if (Input.GetKeyDown(KeyCode.I))
            {
                //change UI
                SetNewInputState(UIManager.UI_GAME_VIEW);
                UIManager.instance.mainInventoryUI.SetActive(false);
            }
        } while (!inStateChanged);

    }

    IEnumerator InputChestViewCorutine()
    {
        UIManager.instance.CursorEnable(true);
        do
        {
            yield return null;
            if (inStateChanged) { break; };

            //mouse
            if (Input.GetMouseButtonDown(0))
            {
                //left key down
            }
            if (Input.GetMouseButton(0))
            {
                //left key down
            }
            if (Input.GetMouseButtonUp(0))
            {
                //left key down
            }

            if (Input.GetMouseButtonDown(1))
            {
                //right key down
            }
            if (Input.GetMouseButton(1))
            {
                //right key down
            }
            if (Input.GetMouseButtonUp(1))
            {
                //right key down
            }

        } while (!inStateChanged);
    }

    IEnumerator InputMenuViewCorutine()
    {
        UIManager.instance.CursorEnable(true);
        do
        {
            yield return null;
            if (inStateChanged) { break; };

            //mouse
            if (Input.GetMouseButtonDown(0))
            {
                //left key down
            }
            if (Input.GetMouseButton(0))
            {
                //left key down
            }
            if (Input.GetMouseButtonUp(0))
            {
                //left key down
            }

            if (Input.GetMouseButtonDown(1))
            {
                //right key down
            }
            if (Input.GetMouseButton(1))
            {
                //right key down
            }
            if (Input.GetMouseButtonUp(1))
            {
                //right key down
            }
        } while (!inStateChanged);
    }

    public void InputNumProcess()
    {

        if (Input.GetKey(KeyCode.Alpha0))
        {
            SceneManager.playerInstance.inputNumKey = 0;
        }
        else if (Input.GetKey(KeyCode.Alpha1))
        {
            SceneManager.playerInstance.inputNumKey = 1;
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            SceneManager.playerInstance.inputNumKey = 2;
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            SceneManager.playerInstance.inputNumKey = 3;
        }
        else if (Input.GetKey(KeyCode.Alpha4))
        {
            SceneManager.playerInstance.inputNumKey = 4;
        }
        else if (Input.GetKey(KeyCode.Alpha5))
        {
            SceneManager.playerInstance.inputNumKey = 5;
        }
        else if (Input.GetKey(KeyCode.Alpha6))
        {
            SceneManager.playerInstance.inputNumKey = 6;
        }
        else if (Input.GetKey(KeyCode.Alpha7))
        {
            SceneManager.playerInstance.inputNumKey = 7;
        }
        else if (Input.GetKey(KeyCode.Alpha8))
        {
            SceneManager.playerInstance.inputNumKey = 8;
        }
        else if (Input.GetKey(KeyCode.Alpha9))
        {
            SceneManager.playerInstance.inputNumKey = 9;
        }
        else
        {
            SceneManager.playerInstance.inputNumKey = -1;
        }
    }
}
