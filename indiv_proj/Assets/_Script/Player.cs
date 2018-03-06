using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    
    public Camera charViewCamera;
    public Camera upViewCamera;
    public Rigidbody playerRigidbody;

    public float horizontalRotationSpeed = 10.0f;
    public float verticalRotationSpeed = 10.0f;
    public float minCharViewVerticalRotation = -90;
    public float maxCharViewVerticalRotation = 90;
    public float noRunSpeed = 1.5f;
    public float runSpeed = 2.5f;
    public float jumpMoveSpeed = 1.5f;

    public const float COMP_FLOAT_ERR = 0.0001f; 


    //from input manager
    public float mouseVerticalMovement = 0.0f;
    public float mouseHorizontalMovement = 0.0f;
    public bool upKey = false, downKey = false, leftKey = false, rightKey = false;
    public bool runKey = false;
    public bool jumpKey = false;
    public bool slideKey = false;
    public bool leftMouseKey = false;
    public bool leftMouseKeyUp = false;
    public bool leftMouseKeyDown = false;
    public bool rightMouseKey = false;
    public bool rightMouseKeyup = false;
    public bool rightMouseKeyDown = false;

    //state & anim
    public Animator playerAnimator;
    public enum PlayerState
    {
        AnimLongIdleStartCorutine, AnimLongIdleEndCorutine, AnimMoveCorutine, AnimJumpStartCorutine, AnimJumpAirCorutine, AnimJumpEndCorutine, AnimSlideCorutine,
        AnimAttackCorutine, AnimMiningStartCorutine, AnimMiningEndCorutine, AnimStunCorutine, AnimKnockoutCorutine, AnimFindStartCorutine,  AnimFindAirCorutine, AnimFindEndCorutine,
        AnimInteractStartCorutine, AnimInteractEndCorutine
    }
    public bool isNewState = false;
    public PlayerState curState;


    //used local variable.
    Vector3 lastMoveVector = new Vector3(0.0f, 0.0f, 0.0f);

    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        charViewCamera = transform.Find("CharCamera").GetComponent<Camera>();
        upViewCamera = transform.Find("UpViewCamera").GetComponent<Camera>();

        if(InputManager.s_cameraMode == InputManager.CAM_MODE_UPVIEW)
        {
            charViewCamera.enabled = false;
            upViewCamera.enabled = true;
        }
        else
        {
            charViewCamera.enabled = true;
            upViewCamera.enabled = false;
        }


        curState = PlayerState.AnimMoveCorutine;    //ser default state
    }

    void OnEnable()
    {
        StartCoroutine("FSMMain");
    }

    void FixedUpdate()
    {
        playerRigidbody.AddForce(Physics.gravity * playerRigidbody.mass);   //2G gravity
    }
    // Use this for initialization
    void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(0, mouseHorizontalMovement, 0);

        Vector3 curEulerRotation = charViewCamera.transform.rotation.eulerAngles;   //cur camera rotation
        curEulerRotation.x += -mouseVerticalMovement;  //rotate camera x
        curEulerRotation.y = COMP_FLOAT_ERR;
        curEulerRotation.z = COMP_FLOAT_ERR;    //prevent invert camera
        Mathf.Clamp(curEulerRotation.x, minCharViewVerticalRotation, maxCharViewVerticalRotation);
        charViewCamera.transform.rotation = Quaternion.Euler(curEulerRotation);

        

    }


    IEnumerator FSMMain()
    {
        while (Application.isPlaying)
        {
            isNewState = false;
            switch (curState)
            {
                case PlayerState.AnimLongIdleStartCorutine:
                    yield return StartCoroutine(AnimLongIdleStartCorutine());
                    break;
                case PlayerState.AnimLongIdleEndCorutine:
                    yield return StartCoroutine(AnimLongIdleEndCorutine());
                    break;
                case PlayerState.AnimMoveCorutine:
                    yield return StartCoroutine(AnimMoveCorutine());
                    break;
                case PlayerState.AnimJumpStartCorutine:
                    yield return StartCoroutine(AnimJumpStartCorutine());
                    break;
                case PlayerState.AnimJumpAirCorutine:
                    yield return StartCoroutine(AnimJumpAirCorutine());
                    break;
                case PlayerState.AnimJumpEndCorutine:
                    yield return StartCoroutine(AnimJumpEndCorutine());
                    break;
                case PlayerState.AnimAttackCorutine:
                    yield return StartCoroutine(AnimAttackCorutine());
                    break;
                case PlayerState.AnimMiningStartCorutine:
                    yield return StartCoroutine(AnimMiningStartCorutine());
                    break;
                case PlayerState.AnimMiningEndCorutine:
                    yield return StartCoroutine(AnimMiningEndCorutine());
                    break;
                case PlayerState.AnimStunCorutine:
                    yield return StartCoroutine(AnimMiningEndCorutine());
                    break;
                case PlayerState.AnimKnockoutCorutine:
                    yield return StartCoroutine(AnimMiningEndCorutine());
                    break;
                case PlayerState.AnimFindStartCorutine:
                    yield return StartCoroutine(AnimFindStartCorutine());
                    break;
                case PlayerState.AnimFindAirCorutine:
                    yield return StartCoroutine(AnimFindAirCorutine());
                    break;
                case PlayerState.AnimFindEndCorutine:
                    yield return StartCoroutine(AnimFindEndCorutine());
                    break;
                case PlayerState.AnimSlideCorutine:
                    yield return StartCoroutine(AnimSlideCorutine());
                    break;
                default:
                    Debug.Log("out of anim switch!. check FSMMain");
                    yield return StartCoroutine(AnimMoveCorutine());
                    break;

            }
            
        }
    }

    void SetNewState(PlayerState newState)
    {
        isNewState = true;
        curState = newState;
    }

    //anim corutine list
    IEnumerator AnimLongIdleStartCorutine()
    {
        //move to idle
        float idleAnimationTime = 3.8f;
        float accPlayTime = 0.0f;
        playerAnimator.SetTrigger(PlayerHash.IdleLongStartID);
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.
            accPlayTime += Time.deltaTime;
            
            //out idle animation
            if (upKey || downKey || leftKey || rightKey)
            {
                SetNewState(PlayerState.AnimLongIdleEndCorutine);
            }
            else if (jumpKey || leftMouseKey || rightMouseKey || slideKey)
            {
                SetNewState(PlayerState.AnimLongIdleEndCorutine);
            }
            else if(accPlayTime > idleAnimationTime)    //end animation time
            {
                SetNewState(PlayerState.AnimLongIdleEndCorutine);
            }
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimLongIdleEndCorutine()
    {
        playerAnimator.SetTrigger(PlayerHash.IdleLongEndID);
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.
            

            SetNewState(PlayerState.AnimMoveCorutine);
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }


    IEnumerator AnimMoveCorutine()
    {
        float longIdleWaikTime = 5.0f;
        float accNotWalkTime = 0.0f;
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            if (upKey || downKey || leftKey || rightKey)
            {
                accNotWalkTime = 0.0f;  //reset acc

                lastMoveVector.Set(0.0f, 0.0f, 0.0f);

                if (upKey)
                {
                    lastMoveVector.z = 1.0f;
                }
                if (downKey)
                {
                    lastMoveVector.z = -1.0f;
                }
                if (leftKey)
                {
                    lastMoveVector.x = -1.0f;
                }
                if (rightKey)
                {
                    lastMoveVector.x = 1.0f;
                }

                lastMoveVector.Normalize();
                
                if (runKey)
                {
                    playerAnimator.SetFloat(PlayerHash.SpeedID, 1.1f);
                    transform.position = transform.position += (transform.rotation * lastMoveVector) * runSpeed * Time.deltaTime;
                }
                else
                {
                    playerAnimator.SetFloat(PlayerHash.SpeedID, 0.6f);
                    transform.position = transform.position += (transform.rotation * lastMoveVector) * noRunSpeed * Time.deltaTime;
                }
                
            }
            else
            {
                playerAnimator.SetFloat(PlayerHash.SpeedID, 0.0f);
                accNotWalkTime += Time.deltaTime;
            }


            //change to long idle animation
            if(accNotWalkTime > longIdleWaikTime)
            {
                SetNewState(PlayerState.AnimLongIdleStartCorutine);
            }

            if (jumpKey)
            {
                SetNewState(PlayerState.AnimJumpStartCorutine);
            }

            if (leftMouseKey)
            {
                //TODO : if has tool or weapon
                //SetNewState(PlayerState.AnimAttackCorutine);
                SetNewState(PlayerState.AnimMiningStartCorutine);
                
            }

            if (rightMouseKey)
            {
                //TODO : if has tool or weapon
                SetNewState(PlayerState.AnimFindStartCorutine);
               
            }

            if (slideKey)
            {
                //TODO : if has tool or weapon
                SetNewState(PlayerState.AnimSlideCorutine);
                //SetNewState(PlayerState.AnimMiningStartCorutine);
            }

            //TODO : Stun, Knockout
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimJumpStartCorutine()
    {
        Vector3 jumpForce = new Vector3(0.0f, 480.0f, 0.0f);
        
        playerAnimator.SetTrigger(PlayerHash.JumpStartID);

        float animationPlayTime = 0.8f;
        float addForceTime = 0.2f;
        float curTime = 0.0f;

        bool forceAdded = false;
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            if(curTime > addForceTime)
            {
                if (!forceAdded)
                {
                    playerRigidbody.AddForce(jumpForce);
                    forceAdded = true;
                }
                else
                {
                    lastMoveVector.Set(0.0f, 0.0f, 0.0f);
                    if (upKey)
                    {
                        lastMoveVector.z = 1.0f;
                    }
                    if (downKey)
                    {
                        lastMoveVector.z = -1.0f;
                    }
                    if (leftKey)
                    {
                        lastMoveVector.x = -1.0f;
                    }
                    if (rightKey)
                    {
                        lastMoveVector.x = 1.0f;
                    }
                    lastMoveVector.Normalize();
                    lastMoveVector *= jumpMoveSpeed * Time.deltaTime;
                    transform.position = transform.position += (transform.rotation * lastMoveVector);
                }

            }
            if(curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimJumpAirCorutine);
            }

            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimJumpAirCorutine()
    {
        float playerYDelta;
        float playerLastYPos = transform.position.y;

        float curTime = 0.0f;
        float maxJumpTime = 10.0f;

        int landCheckFrameCount = 1;
        int curLandCheckFrameCount = 0;
        playerAnimator.SetTrigger(PlayerHash.JumpAirID);

        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            //land check
            playerYDelta = transform.position.y - playerLastYPos;
            if(playerYDelta < COMP_FLOAT_ERR)
            {
                curLandCheckFrameCount += 1;
            }

            if(curLandCheckFrameCount > landCheckFrameCount)
            {
                SetNewState(PlayerState.AnimJumpEndCorutine);
            }

            if(curTime > maxJumpTime)
            {
                SetNewState(PlayerState.AnimJumpEndCorutine);
            }

            //move jump
            lastMoveVector.Set(0.0f, 0.0f, 0.0f);
            if (upKey)
            {
                lastMoveVector.z = 1.0f;
            }
            if (downKey)
            {
                lastMoveVector.z = -1.0f;
            }
            if (leftKey)
            {
                lastMoveVector.x = -1.0f;
            }
            if (rightKey)
            {
                lastMoveVector.x = 1.0f;
            }
            lastMoveVector.Normalize();
            lastMoveVector *= jumpMoveSpeed * Time.deltaTime;
            transform.position = transform.position += (transform.rotation * lastMoveVector);

            curTime += Time.deltaTime;

        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimJumpEndCorutine()
    {
        playerAnimator.SetTrigger(PlayerHash.JumpEndID);

        float animationPlayTime = 1.0f;
        float curTime = 0.0f;
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.
            
            if(curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimMoveCorutine);
                jumpKey = false;    // for remove jumpkey input before jump end!
            }
            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimAttackCorutine()
    {
        playerAnimator.SetTrigger(PlayerHash.AttackID);

        float animationPlayTime = 0.8f;
        float curTime = 0.0f;
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            if (curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimMoveCorutine);
                leftMouseKey = false;    // for remove jumpkey input before jump end!
            }
            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    /*
     * Do Mining Animation. If block hp = 0, then get Item & destroy block
    */
    IEnumerator AnimMiningStartCorutine()
    {
        float animIntervalHitTime = 0.6f;
        float animIntervalTime = 0.8f;
        float curTime = 0.0f;
        int blockHitCount = 0;
        int blockHPCount = 0;

        RaycastHit hit;
        Ray ray;
        Vector3 monitorCenterViewport = new Vector3(0.5f, 0.5f, 0f);    //ray with center of camera
        float blockCheckDistance = 1.5f;                                //pick distance
        GameObject recentBlockGameobject = null;                        
        bool isBlockHPReset = true;
        bool isBlockChecked = false;

        playerAnimator.SetTrigger(PlayerHash.MiningStartID);
        do
        {
            yield return null;
            if (isNewState) { break; }      //check if state is changed before start.

            // start block check
            if (curTime > animIntervalHitTime && !isBlockChecked)
            {
                ray = charViewCamera.ViewportPointToRay(monitorCenterViewport);
                if (Physics.Raycast(ray, out hit, blockCheckDistance))
                {
                    Transform objectHit = hit.transform;

                    if(recentBlockGameobject == null || recentBlockGameobject != objectHit.gameObject)
                    {
                        recentBlockGameobject = objectHit.gameObject;
                        blockHitCount = 1;
                        blockHPCount = recentBlockGameobject.GetComponent<BlockPrefab>().m_hp;
                        isBlockHPReset = recentBlockGameobject.GetComponent<BlockPrefab>().m_hpReset;

                        if (!isBlockHPReset)
                        {
                            recentBlockGameobject.GetComponent<BlockPrefab>().m_hp -= 1;
                        }
                    }
                    else
                    {
                        //same block.
                        blockHitCount++;
                        if (!isBlockHPReset)
                        {
                            recentBlockGameobject.GetComponent<BlockPrefab>().m_hp -= 1;
                        }
                        if (blockHitCount >= blockHPCount)
                        {
                            //remove block and make item.
                            SceneManager.instance.DeleteBlock(recentBlockGameobject.transform.position);
                            //TODO : item to inventory
                            
                        }
                    }
                }
                isBlockChecked = true;
            }
            else if(curTime > animIntervalTime)
            {
                curTime = 0.0f;
                isBlockChecked = false;
            }

            curTime += Time.deltaTime;
            
            if (!leftMouseKey)
            {
                SetNewState(PlayerState.AnimMiningEndCorutine);
            }
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimMiningEndCorutine()
    {
        playerAnimator.SetTrigger(PlayerHash.MiningEndID);
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.
            leftMouseKey = false;
            SetNewState(PlayerState.AnimMoveCorutine);
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }


    /*
     * Do interact motion. Setup block or open chest
    */
    IEnumerator AnimInteractStartCorutine()
    {
        float animIntervalHitTime = 0.6f;
        float animIntervalTime = 0.8f;
        float curTime = 0.0f;
        int blockHitCount = 0;
        int blockHPCount = 0;

        RaycastHit hit;
        Ray ray;
        Vector3 monitorCenterViewport = new Vector3(0.5f, 0.5f, 0f);    //ray with center of camera
        float blockCheckDistance = 1.5f;                                //pick distance
        GameObject recentBlockGameobject = null;
        bool isBlockHPReset = true;
        bool isBlockChecked = false;

        playerAnimator.SetTrigger(PlayerHash.MiningStartID);
        do
        {
            yield return null;
            if (isNewState) { break; }      //check if state is changed before start.

            // start block check
            if (curTime > animIntervalHitTime && !isBlockChecked)
            {
                ray = charViewCamera.ViewportPointToRay(monitorCenterViewport);
                if (Physics.Raycast(ray, out hit, blockCheckDistance))
                {
                    Transform objectHit = hit.transform;

                    if (recentBlockGameobject == null || recentBlockGameobject != objectHit.gameObject)
                    {
                        recentBlockGameobject = objectHit.gameObject;
                        blockHitCount = 1;
                        blockHPCount = recentBlockGameobject.GetComponent<BlockPrefab>().m_hp;
                        isBlockHPReset = recentBlockGameobject.GetComponent<BlockPrefab>().m_hpReset;

                        if (!isBlockHPReset)
                        {
                            recentBlockGameobject.GetComponent<BlockPrefab>().m_hp -= 1;
                        }
                    }
                    else
                    {
                        //same block.
                        blockHitCount++;
                        if (!isBlockHPReset)
                        {
                            recentBlockGameobject.GetComponent<BlockPrefab>().m_hp -= 1;
                        }
                        if (blockHitCount >= blockHPCount)
                        {
                            //remove block and make item.
                            SceneManager.instance.DeleteBlock(recentBlockGameobject.transform.position);
                            //TODO : item to inventory

                        }
                    }
                }
                isBlockChecked = true;
            }
            else if (curTime > animIntervalTime)
            {
                curTime = 0.0f;
                isBlockChecked = false;
            }

            curTime += Time.deltaTime;

            if (!leftMouseKey)
            {
                SetNewState(PlayerState.AnimMiningEndCorutine);
            }
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimInteractEndCorutine()
    {
        playerAnimator.SetTrigger(PlayerHash.MiningEndID);
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.
            leftMouseKey = false;
            SetNewState(PlayerState.AnimMoveCorutine);
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }


    IEnumerator AnimSlideCorutine()
    {
        float animationPlayTime = 1.4f;
        float curTime = 0.0f;
        float slideSpeed = 1.0f;

        playerAnimator.SetTrigger(PlayerHash.SlideID);
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            lastMoveVector.Set(0.0f, 0.0f, 1.0f);
            lastMoveVector *= slideSpeed * Time.deltaTime;
            transform.position = transform.position += (transform.rotation * lastMoveVector);

            if (curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimMoveCorutine);
            }
            leftMouseKey = false;

            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimStunCorutine()
    {
        float animationPlayTime = 1.1f;
        float curTime = 0.0f;

        playerAnimator.SetTrigger(PlayerHash.StunID);
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            if (curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimMoveCorutine);
            }
            leftMouseKey = false;

            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimKnockoutCorutine()
    {
        float animationPlayTime = 3.5f;
        float curTime = 0.0f;

        playerAnimator.SetTrigger(PlayerHash.KnockoutID);
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            if (curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimMoveCorutine);
            }
            leftMouseKey = false;

            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }


    IEnumerator AnimFindStartCorutine()
    {
        
        playerAnimator.SetTrigger(PlayerHash.FindStartID);

        float animationPlayTime = 0.7f;
        float curTime = 0.0f;
        
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.
            
            if (curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimFindAirCorutine);
            }

            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimFindAirCorutine()
    {
        float maxAirTime = 10.0f;
        float curTime = 0.0f;
        playerAnimator.SetTrigger(PlayerHash.FindAirID);

        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.
            
            //TODO : if find end
            /*if ()
            {
                SetNewState(PlayerState.AnimJumpEndCorutine);
            }
            */

            if (curTime > maxAirTime)
            {
                SetNewState(PlayerState.AnimFindEndCorutine);
            }
            
            curTime += Time.deltaTime;

        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimFindEndCorutine()
    {
        playerAnimator.SetTrigger(PlayerHash.FindEndID);

        float animationPlayTime = 0.95f;
        float curTime = 0.0f;
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            if (curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimMoveCorutine);
                rightMouseKey = false;    // for remove jumpkey input before jump end!
            }
            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }
}

//base anim corutine
/*
IEnumerator AnimMoveCorutine()
    {
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }
*/
