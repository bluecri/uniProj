using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemAddToPlayer : MonoBehaviour {
    [System.NonSerialized]
    public BlockPrefab blockPrefabInfo;
    public float distanceDouble = 1.4f;
    public bool isBlockAdded = false;

    private void Awake()
    {
        blockPrefabInfo = GetComponent<BlockPrefab>();
    }

    private void Start()
    {
        isBlockAdded = false;
        StartCoroutine(ItemCheckPlayerCorutine());
    }

    private void Update()
    {
        if(isBlockAdded == true)
        {
            Destroy(gameObject);
        }
    }

    IEnumerator ItemCheckPlayerCorutine()
    {
        while (true)
        {
            if (Vector3.SqrMagnitude(transform.position - SceneManager.playerInstance.transform.position) < distanceDouble)
            {
                break;
            }
            else
            {
                yield return new WaitForSeconds(1.0f);
            }
        }
        yield return StartCoroutine(ItemFlyToPlayerCorutine());
    }

    IEnumerator ItemFlyToPlayerCorutine()
    {
        float flyTimeInterval = 3.0f;
        float curTime = 0.0f;
        float blockMagneticSpeed = 1.0f;
        while (true)
        {
            if(curTime > flyTimeInterval)
            {
                break;
            }
            transform.position = Vector3.Lerp(transform.position, SceneManager.playerInstance.transform.position, blockMagneticSpeed * Time.deltaTime);
            curTime += Time.deltaTime;
            yield return null;
        }
        SceneManager.playerInstance.PlayAudioClip(SceneManager.playerInstance.playerSoundClip.pickUpSound);
        
        yield return StartCoroutine(ItemAddToPlayerCorutine());
    }

    IEnumerator ItemAddToPlayerCorutine()
    {
        isBlockAdded = true;
        SceneManager.playerInstance.playerInventory.AddBlockWithPickUp(blockPrefabInfo.m_geoCode, 1);
        yield return null;
    }
}
