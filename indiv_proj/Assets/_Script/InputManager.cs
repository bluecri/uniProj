using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

    //awake param
    public static InputManager instance = null;

    public const int CAM_MODE_UPVIEW = 0;
    public const int CAM_MODE_CHARVIEW = 1;

    public static int s_cameraMode = CAM_MODE_UPVIEW;

    public IEnumerator mainInputCorutine = null;

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
    //start param
    private Player playerScript;
    // update param
    void Start () {
        playerScript = SceneManager.playerInstance;

        switch (UIManager.s_uiStatus)
        {
            case UIManager.UI_GAME_VIEW:
                mainInputCorutine = InputGameViewCorutine();
                StartCoroutine(mainInputCorutine);
                break;
            case UIManager.UI_INVEN_VIEW:
                mainInputCorutine = InputInvenViewCorutine();
                StartCoroutine(mainInputCorutine);
                break;
            case UIManager.UI_CHEST_VIEW:
                mainInputCorutine = InputChestViewCorutine();
                StartCoroutine(mainInputCorutine);
                break;
            case UIManager.UI_MENU_VIEW:
                mainInputCorutine = InputMenuViewCorutine();
                StartCoroutine(mainInputCorutine);
                break;
        }
    }
    void FixedUpdate()
    {
        
    }

    // Update is called once per frame
    void Update () {
        
    }

    //used in UIManager
    public void ChangeMainInputCorutine(int UIManagerUIMode)
    {
        switch (UIManagerUIMode)
        {
            case UIManager.UI_GAME_VIEW:
                StopCoroutine(mainInputCorutine);
                mainInputCorutine = InputGameViewCorutine();
                StartCoroutine(mainInputCorutine);
                break;
            case UIManager.UI_INVEN_VIEW:
                StopCoroutine(mainInputCorutine);
                mainInputCorutine = InputInvenViewCorutine();
                StartCoroutine(mainInputCorutine);
                break;
            case UIManager.UI_CHEST_VIEW:
                StopCoroutine(mainInputCorutine);
                mainInputCorutine = InputChestViewCorutine();
                StartCoroutine(mainInputCorutine);
                break;
            case UIManager.UI_MENU_VIEW:
                StopCoroutine(mainInputCorutine);
                mainInputCorutine = InputMenuViewCorutine();
                StartCoroutine(mainInputCorutine);
                break;
        }
    }

    IEnumerator InputGameViewCorutine()
    {
        while (true)
        {
            //horizontal : charactor rotation
            //vertical : camera move
            float mouseVerticalMovement = Input.GetAxis("Mouse Y");
            switch (s_cameraMode)
            {
                //mouse movement rotation
                case CAM_MODE_UPVIEW:
                    playerScript.mouseHorizontalMovement = Input.GetAxis("Mouse X");
                    break;
                case CAM_MODE_CHARVIEW:
                    playerScript.mouseHorizontalMovement = Input.GetAxis("Mouse X");
                    playerScript.mouseVerticalMovement = Input.GetAxis("Mouse Y");
                    break;
            }
            //mouse
            if (Input.GetMouseButtonDown(0))
            {
                //left key down
                playerScript.leftMouseKeyDown = true;
            }
            if (Input.GetMouseButton(0))
            {
                //left key down
                playerScript.leftMouseKey = true;
            }
            else
            {
                playerScript.leftMouseKey = false;
            }

            if (Input.GetMouseButtonUp(0))
            {
                //left key down
                playerScript.leftMouseKeyUp = true;
            }

            if (Input.GetMouseButtonDown(1))
            {
                //right key down
                playerScript.rightMouseKeyDown = true;
            }
            if (Input.GetMouseButton(1))
            {
                //right key down
                playerScript.rightMouseKey = true;
            }
            else
            {
                playerScript.rightMouseKey = false;
            }
            if (Input.GetMouseButtonUp(1))
            {
                //right key down
                playerScript.rightMouseKeyup = true;
            }

            //move
            if (Input.GetKey(KeyCode.A))
            {
                //up
                playerScript.leftKey = true;
            }
            else
            {
                playerScript.leftKey = false;
            }

            if (Input.GetKey(KeyCode.D))
            {
                //up
                playerScript.rightKey = true;
            }
            else
            {
                playerScript.rightKey = false;
            }

            if (Input.GetKey(KeyCode.W))
            {
                //up
                playerScript.upKey = true;
            }
            else
            {
                playerScript.upKey = false;
            }

            if (Input.GetKey(KeyCode.S))
            {
                //down
                playerScript.downKey = true;
            }
            else
            {
                playerScript.downKey = false;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                //RUN
                playerScript.runKey = true;
            }
            else
            {
                playerScript.runKey = false;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                //jump
                playerScript.jumpKey = true;
            }
            else
            {
                playerScript.jumpKey = false;
            }

            if (Input.GetKey(KeyCode.F))
            {
                //jump
                playerScript.slideKey = true;
            }
            else
            {
                playerScript.slideKey = false;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                //change camera
                if (s_cameraMode == CAM_MODE_UPVIEW)
                {
                    s_cameraMode = CAM_MODE_CHARVIEW;
                    SceneManager.playerInstance.charViewCamera.enabled = true;
                    SceneManager.playerInstance.upViewCamera.enabled = false;
                }
                else
                {
                    s_cameraMode = CAM_MODE_UPVIEW;
                    SceneManager.playerInstance.charViewCamera.enabled = false;
                    SceneManager.playerInstance.upViewCamera.enabled = true;
                }


            }
            if (Input.GetKey(KeyCode.O))
            {
                //change UI
            }
            yield return null;
        }
    }

    IEnumerator InputInvenViewCorutine()
    {
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
        yield return null;
    }

    IEnumerator InputChestViewCorutine()
    {
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
        yield return null;
    }

    IEnumerator InputMenuViewCorutine()
    {
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

        yield return null;
    }
}
