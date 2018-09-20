using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    //Component
    public Rigidbody playerRigidbody;
    public AudioSource playerAudioSource;
    public SoundClipList playerSoundClip;
   
    //Player State Param
    public float horizontalRotationSpeed = 10.0f;
    public float verticalRotationSpeed = 10.0f;
    public float noRunSpeed = 1.5f;
    public float runSpeed = 2.5f;
    public float jumpMoveSpeed = 2.0f;
    public float chaseDistance = 8.0f;

    //Const
    public const float COMP_FLOAT_ERR = 0.0001f;

    //right hand
    [SerializeField]
    public GameObject playerRightHandGameObject;
    [System.NonSerialized]
    public BlockPrefab playerRightHandBlockPrefabInfo = null;

    //Key Input from Input Manager

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
    public bool inventoryKey = false;
    public int inputNumKey = -1;

    public int enemyHP = 100;


    public Queue<int> damagedIntList = new Queue<int>();

    //Player State & Animation
    public Animator playerAnimator;
    public enum PlayerState
    {
        AnimLongIdleStartCorutine, AnimLongIdleEndCorutine, AnimMoveCorutine, AnimJumpStartCorutine, AnimJumpAirCorutine, AnimJumpEndCorutine, AnimSlideCorutine,
        AnimAttackCorutine, AnimMiningStartCorutine, AnimMiningEndCorutine, AnimStunCorutine, AnimKnockoutCorutine, AnimFindStartCorutine, AnimFindAirCorutine, AnimFindEndCorutine,
        AnimInteractStartCorutine, AnimInteractEndCorutine, AnimChangeRightHandItem, AnimChaseCorutine
    }
    public bool isNewState = false;
    public PlayerState curState;
    public GameObject targetEnemy;  //target of this
    public Quaternion targetQuaternion;


    //Used for local variable
    Vector3 lastMoveVector = new Vector3(0.0f, 0.0f, 0.0f);

    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        playerSoundClip = GetComponent<SoundClipList>();
        playerAudioSource = GetComponent<AudioSource>();
        
        curState = PlayerState.AnimMoveCorutine;    //ser default state
        targetQuaternion = transform.rotation;  //quat init
    }

    void OnEnable()
    {
        StartCoroutine("FSMMain");
    }

    void FixedUpdate()
    {
        //playerRigidbody.AddForce(Physics.gravity * playerRigidbody.mass);   //2G gravity
    }

    // Update is called once per frame
    void Update()
    { 

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
                
                case PlayerState.AnimSlideCorutine:
                    yield return StartCoroutine(AnimSlideCorutine());
                    break;
                case PlayerState.AnimChangeRightHandItem:
                    yield return StartCoroutine(AnimChangeRightHandItem());
                    break;
                case PlayerState.AnimChaseCorutine:
                    yield return StartCoroutine(AnimChaseCorutine());
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
            else if (accPlayTime > idleAnimationTime)    //end animation time
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

    IEnumerator AnimChaseCorutine()
    {
        float targetQuaternionCalcInterval = 0.5f;
        float curTargetQuaternionCalcInterval = 0.0f;   //when enemy set,  init this too.

        float speedDuration = 1.0f; //how much move exist
        float curSpeedDuration = 0.0f;
        int speedMode = 0;  // 0.stand 1.walk 2.run
        float speed = 0.0f;

        float curMoveSoundTime = 0.0f;
        float runSoundPlayTime = 0.43f;
        float walkSoundPlayTime = 0.49f;

        float distanceToTarget = 20.0f;
        
        //Random value for slide, weapon change... etx
        int randomVal = Random.Range(0, 1000);

        do
        {
            //calc target Quaternion
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            // Update Rotation to Player
            if (curTargetQuaternionCalcInterval > targetQuaternionCalcInterval)
            {
                CalcTargetQuaternion();
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, targetQuaternion, Time.deltaTime * horizontalRotationSpeed);

            // If target is too far, enemy reset;
            distanceToTarget = Vector3.SqrMagnitude(transform.position - targetEnemy.transform.position);
            

            //Select Speed & duration
            if (curSpeedDuration > speedDuration)
            {
                GetRandomSpeedMode(out speed, out speedDuration, out speedMode);
                curSpeedDuration = 0.0f;
            }

            switch (speedMode)
            {
                case 0:
                    playerAnimator.SetFloat(PlayerHash.SpeedID, 0.0f);
                    curMoveSoundTime = 0.0f;
                    break;
                case 1:
                    playerAnimator.SetFloat(PlayerHash.SpeedID, 0.6f);
                    transform.position = transform.position += transform.forward * noRunSpeed * Time.deltaTime;
                    if (curMoveSoundTime > walkSoundPlayTime)
                    {
                        PlayAudioClip(playerSoundClip.runSound);
                        curMoveSoundTime = curMoveSoundTime - walkSoundPlayTime;
                    }
                    curMoveSoundTime += Time.deltaTime;
                    break;
                case 2:
                    playerAnimator.SetFloat(PlayerHash.SpeedID, 1.1f);
                    transform.position = transform.position += transform.forward * runSpeed * Time.deltaTime;
                    if (curMoveSoundTime > runSoundPlayTime)
                    {
                        PlayAudioClip(playerSoundClip.runSound);
                        curMoveSoundTime = runSoundPlayTime - curMoveSoundTime;
                    }
                    curMoveSoundTime += Time.deltaTime;
                    break;
            }

            
            // if block is in front of this, jump!
            if (jumpKey)
            {
                SetNewState(PlayerState.AnimJumpStartCorutine);
            }

            if (playerRightHandBlockPrefabInfo != null)
            {
                if (distanceToTarget < playerRightHandBlockPrefabInfo.m_blockDigDistance - 0.15f)
                {
                    SetNewState(PlayerState.AnimMiningStartCorutine);
                }
            }
            else
            {
                if (distanceToTarget < 0.9f)
                {
                    SetNewState(PlayerState.AnimMiningStartCorutine);
                }
            }
            
            if (damagedIntList.Count > 0)
            {
                PlayAudioClip(playerSoundClip.damagedSound);
                if (randomVal < 600) //50%
                {
                    SetNewState(PlayerState.AnimStunCorutine);
                }
                if (randomVal < 100)  //10%
                {
                    SetNewState(PlayerState.AnimKnockoutCorutine);
                }
                randomVal = Random.Range(0, 1000);
                enemyHP -= damagedIntList.Dequeue();
            }
            else if (randomVal < 15) //slide 1 time per 16 sec
            {
                SetNewState(PlayerState.AnimSlideCorutine);
                randomVal = Random.Range(0, 1000);
            }
            else if (randomVal < 5)  //change 1 time per 32 sec
            {
                SetNewState(PlayerState.AnimChangeRightHandItem);
                randomVal = Random.Range(0, 1000);
            }
            else if (distanceToTarget > chaseDistance)
            {
                targetEnemy = null;
                SetNewState(PlayerState.AnimMoveCorutine);
            }

            curSpeedDuration += Time.deltaTime;
            curTargetQuaternionCalcInterval += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimMoveCorutine()
    {
        float speedDuration = 1.0f; //how much move exist
        float curSpeedDuration = 0.0f;
        int speedMode = 0;  // 0.stand 1.walk 2.run
        float speed = 0.0f;

        float curMoveSoundTime = 0.0f;
        float runSoundPlayTime = 0.43f;
        float walkSoundPlayTime = 0.49f;

        float longIdleWaikTime = 2.0f;
        float accNotWalkTime = 0.0f;

        float randomQuatTime = 3.0f;
        float curRandomQuatTime = 0.0f;
        
        do
        {
            //calc target Quaternion
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            
            // Update Rotation Random
            if(curRandomQuatTime > randomQuatTime)
            {
                curRandomQuatTime = 0.0f;
                randomQuatTime = Random.Range(2.0f, 5.0f);
                CalcRandomQuaternion();
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, targetQuaternion, Time.deltaTime * horizontalRotationSpeed);
            
            //Select Speed & duration
            if (curSpeedDuration > speedDuration)
            {
                GetRandomSpeedMode(out speed, out speedDuration, out speedMode);
                curSpeedDuration = 0.0f;
            }

            switch (speedMode)
            {
                case 0:
                    playerAnimator.SetFloat(PlayerHash.SpeedID, 0.0f);
                    accNotWalkTime += Time.deltaTime;
                    curMoveSoundTime = 0.0f;
                    break;
                case 1:
                    accNotWalkTime = 0.0f;
                    playerAnimator.SetFloat(PlayerHash.SpeedID, 0.6f);
                    transform.position = transform.position += transform.forward * noRunSpeed * Time.deltaTime;
                    if (curMoveSoundTime > walkSoundPlayTime)
                    {
                        PlayAudioClip(playerSoundClip.runSound);
                        curMoveSoundTime = curMoveSoundTime - walkSoundPlayTime;
                    }
                    curMoveSoundTime += Time.deltaTime;
                    break;
                case 2:
                    accNotWalkTime = 0.0f;
                    playerAnimator.SetFloat(PlayerHash.SpeedID, 1.1f);
                    transform.position = transform.position += transform.forward * runSpeed * Time.deltaTime;
                    if (curMoveSoundTime > runSoundPlayTime)
                    {
                        PlayAudioClip(playerSoundClip.runSound);
                        curMoveSoundTime = runSoundPlayTime - curMoveSoundTime;
                    }
                    curMoveSoundTime += Time.deltaTime;
                    break;
            }

            //change to long idle animation
            if (accNotWalkTime > longIdleWaikTime)
            {
                SetNewState(PlayerState.AnimLongIdleStartCorutine);
            }

            // if block is in front of this, jump!
            if (jumpKey)
            {
                SetNewState(PlayerState.AnimJumpStartCorutine);
            }
            
            ProcessDamagedMoveToChase();

            curRandomQuatTime += Time.deltaTime;
            curSpeedDuration += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimJumpStartCorutine()
    {
        Vector3 jumpForce = new Vector3(0.0f, 500.0f, 0.0f);

        playerAnimator.SetTrigger(PlayerHash.JumpStartID);

        float animationPlayTime = 0.8f;
        float addForceTime = 0.2f;
        float curTime = 0.0f;

        float jumpSoundIntervalTime = 0.13f;
        bool isJumpSoundIsPlayed = false;

        bool forceAdded = false;

        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            ProcessDamagaeOnly();

            if (curTime > jumpSoundIntervalTime && !isJumpSoundIsPlayed)
            {
                PlayAudioClip(playerSoundClip.jumpSound);
                isJumpSoundIsPlayed = true;
            }

            if (curTime > addForceTime)
            {
                if (!forceAdded)
                {
                    playerRigidbody.AddForce(jumpForce);
                    forceAdded = true;
                }
                else
                {
                    transform.position = transform.position += transform.forward * jumpMoveSpeed * Time.deltaTime;
                }
            }
            if (curTime > animationPlayTime)
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

            ProcessDamagaeOnly();

            //land check
            playerYDelta = transform.position.y - playerLastYPos;
            if (playerYDelta < COMP_FLOAT_ERR)
            {
                curLandCheckFrameCount += 1;
            }

            if (curLandCheckFrameCount > landCheckFrameCount)
            {
                SetNewState(PlayerState.AnimJumpEndCorutine);
            }

            if (curTime > maxJumpTime)
            {
                SetNewState(PlayerState.AnimJumpEndCorutine);
            }

            //move jump
            transform.position = transform.position += transform.forward * jumpMoveSpeed * Time.deltaTime;

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

            if (damagedIntList.Count > 0)
            {
                PlayAudioClip(playerSoundClip.damagedSound);
                enemyHP -= damagedIntList.Dequeue();
            }

            if (curTime > animationPlayTime)
            {
                if (targetEnemy == null)
                {
                    SetNewState(PlayerState.AnimMoveCorutine);
                }
                else
                {
                    SetNewState(PlayerState.AnimChaseCorutine);
                }
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
        float missAudioIntervalTime = 0.24f;
        float curTime = 0.0f;

        RaycastHit hit;

        Vector3 monitorCenterViewport = new Vector3(0.5f, 0.5f, 0f);    //ray with center of camera
        //float blockCheckDistance = 1.5f;                                //pick distance
        
        bool isRayChecked = false;
        bool isMissAudioPlayed = false;

        int rayHItWithPlayer = (1 << PlayerHash.PlayerLayer);

        bool rightHandCanDig = true;
        bool rightHandCanAttack = true;
        int rightHandBlockDamage = 0;
        float rightHandBlockDigDistance = 0.0f;
        int rightHandUnitDamage = 0;

        //setting right hand param
        if (playerRightHandBlockPrefabInfo == null)
        {
            rightHandCanDig = true;
            rightHandCanAttack = true;
            rightHandBlockDamage = 1;
            rightHandBlockDigDistance = 1.5f;
            rightHandUnitDamage = 1;
        }
        else
        {
            rightHandCanDig = playerRightHandBlockPrefabInfo.m_canDig;
            rightHandCanAttack = playerRightHandBlockPrefabInfo.m_canAttack;
            rightHandBlockDamage = playerRightHandBlockPrefabInfo.m_blockDamage;
            rightHandBlockDigDistance = playerRightHandBlockPrefabInfo.m_blockDigDistance;
            rightHandUnitDamage = playerRightHandBlockPrefabInfo.m_unitDamage;
        }

        playerAnimator.SetTrigger(PlayerHash.MiningStartID);
        do
        {
            yield return null;
            if (isNewState) { break; }      //check if state is changed before start.

            ProcessDamagaeTransitionToSpecialState();

            if (curTime > missAudioIntervalTime && !isMissAudioPlayed)
            {
                PlayAudioClip(playerSoundClip.attackMissSound);
                isMissAudioPlayed = true;   //False when animation 1 loop end
            }
            // start block check
            if (curTime > animIntervalHitTime && !isRayChecked)
            {
                isRayChecked = true;
                if (Physics.Raycast(transform.position, targetEnemy.transform.position - transform.position, out hit, rightHandBlockDigDistance, rayHItWithPlayer))
                {
                    Transform objectPlayerHit = hit.transform;

                    if (rightHandCanAttack)
                    {
                        PlayAudioClip(playerSoundClip.attckHitSound);
                        GameObject playerGameObject = objectPlayerHit.gameObject;
                        Debug.Log("hit enemy to player");
                    }
                }
                else
                {
                    //no block, no enemy hit. init block info.
                }

            }
            else if (curTime > animIntervalTime)
            {
                //init curTime & Re Loop Raycheck
                curTime = 0.0f;
                isRayChecked = false;
                isMissAudioPlayed = false;
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
            SetNewState(PlayerState.AnimChaseCorutine);
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    IEnumerator AnimSlideCorutine()
    {
        float animationPlayTime = 1.4f;
        float curTime = 0.0f;
        float slideSpeed = 2.0f;

        float slideSoundIntervalTime = 0.2f;
        float curSlideSoundPlayTime = 0.0f;

        playerAnimator.SetTrigger(PlayerHash.SlideID);
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            if (damagedIntList.Count > 0)
            {
                PlayAudioClip(playerSoundClip.attackMissSound);
                damagedIntList.Dequeue();
            }
            
            if (curSlideSoundPlayTime > slideSoundIntervalTime)
            {
                PlayAudioClip(playerSoundClip.slideSound);
                curSlideSoundPlayTime = 0.0f;
            }
            
            transform.position = transform.position += transform.forward * slideSpeed * Time.deltaTime;

            if (curTime > animationPlayTime)
            {
                if(targetEnemy == null)
                {
                    SetNewState(PlayerState.AnimMoveCorutine);
                }
                else
                {
                    SetNewState(PlayerState.AnimChaseCorutine);
                }
                
            }
            leftMouseKey = false;

            curTime += Time.deltaTime;
            curSlideSoundPlayTime += Time.deltaTime;
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

            if (damagedIntList.Count > 0)
            {
                PlayAudioClip(playerSoundClip.damagedSound);
                enemyHP -= damagedIntList.Dequeue();
            }

            if (curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimChaseCorutine);
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

            if (damagedIntList.Count > 0)
            {
                PlayAudioClip(playerSoundClip.damagedSound);
                enemyHP -= damagedIntList.Dequeue();
            }

            if (curTime > animationPlayTime)
            {
                SetNewState(PlayerState.AnimChaseCorutine);
            }
            leftMouseKey = false;

            curTime += Time.deltaTime;
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }
    
    IEnumerator AnimChangeRightHandItem()
    {
        do
        {
            yield return null;
            if (isNewState) { break; } //check if state is changed before start.

            PlayerHandItemChange(Random.Range(101, 104));

            if (targetEnemy == null)
            {
                SetNewState(PlayerState.AnimMoveCorutine);
            }
            else
            {
                SetNewState(PlayerState.AnimChaseCorutine);
            }
        }
        while (!isNewState);    //check if state is changed in this corutine.
    }

    public void PlayAudioClip(AudioClip ac)
    {
        playerAudioSource.clip = ac;
        playerAudioSource.Play();
    }
    
    public void PlayerHandItemChange(int geoCode)
    {
        for (int childIndex = 1; childIndex < playerRightHandGameObject.transform.childCount; childIndex++)
        {
            ItemPool.instance.ReturnHandGameObjectToPool(playerRightHandGameObject.transform.GetChild(childIndex).gameObject);
        }
        if (geoCode != 0)
        {
            GameObject handItem = ItemPool.instance.GetItemHandBlockGameObjectFromPool(geoCode);
            handItem.transform.SetParent(playerRightHandGameObject.transform, false);
            playerRightHandBlockPrefabInfo = handItem.GetComponent<BlockPrefab>();
        }
        else
        {
            playerRightHandBlockPrefabInfo = null;  //handPrefab
        }
    }
    public void CalcTargetQuaternion()
    {
        if(targetEnemy != null)
        {
            Vector3 relativeVec3 = targetEnemy.transform.position - transform.position;
            relativeVec3.y = 0;
            targetQuaternion = Quaternion.LookRotation(relativeVec3);
        }
    }

    public void CalcRandomQuaternion()
    {
        if (targetEnemy == null)
        {
            Vector3 relativeVec3 = new Vector3(Random.Range(-1.0f, 0.0f), 0, Random.Range(-1.0f, 0.0f));
            targetQuaternion = Quaternion.LookRotation(relativeVec3);
        }
    }

    public void GetRandomSpeedMode(out float speed, out float speedDuration, out int speedMode)
    {
        speedMode = Random.Range(0, 3);
        speedDuration = Random.Range(2.0f, 4.0f);
        speed = 0;
        switch (speedMode)
        {
            case 0:
                speed = 0.0f;
                break;
            case 1:
                speed = Random.Range(0.5f, noRunSpeed);
                break;
            case 2:
                speed = Random.Range(2.0f, runSpeed);
                break;
        }
    }

    public void ProcessDamagedMoveToChase()
    {
        int randomVal = Random.Range(0, 1000);
        
        if (damagedIntList.Count > 0)
        {
            PlayAudioClip(playerSoundClip.damagedSound);
            SetNewState(PlayerState.AnimChaseCorutine);
            if (randomVal < 600) //50%
            {
                SetNewState(PlayerState.AnimStunCorutine);
            }
            if (randomVal < 100)  //10%
            {
                SetNewState(PlayerState.AnimKnockoutCorutine);
            }
            //TODO : play particle
            targetEnemy = SceneManager.playerInstance.gameObject;
            CalcTargetQuaternion();
            enemyHP -= damagedIntList.Dequeue();
        }
    }

    // only get damage
    public void ProcessDamagaeOnly()
    {
        if (damagedIntList.Count > 0)
        {
            PlayAudioClip(playerSoundClip.damagedSound);
            SetNewState(PlayerState.AnimChaseCorutine);

            //TODO : play particle
            targetEnemy = SceneManager.playerInstance.gameObject;
            CalcTargetQuaternion();
            enemyHP -= damagedIntList.Dequeue();
        }
    }

    public void ProcessDamagaeTransitionToSpecialState()
    {
        int randomVal = Random.Range(0, 1000);
        if (damagedIntList.Count > 0)
        {
            PlayAudioClip(playerSoundClip.damagedSound);
            
            if (randomVal < 600) //50%
            {
                SetNewState(PlayerState.AnimStunCorutine);
            }
            if (randomVal < 100)  //10%
            {
                SetNewState(PlayerState.AnimKnockoutCorutine);
            }
            //TODO : play particle
            targetEnemy = SceneManager.playerInstance.gameObject;
            CalcTargetQuaternion();
            enemyHP -= damagedIntList.Dequeue();
        }
    }
}
