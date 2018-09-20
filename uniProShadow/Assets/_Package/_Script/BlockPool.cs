using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPool : MonoBehaviour {
    public static BlockPool instance = null;

    [SerializeField]
    public List<GameObject> blockPrefabList;  //for editor
    public Dictionary<int, GameObject> blockPrefabInfo_dictionary;    //block init info. [GeoCode]-> BlockInfo

    public static int geoPrefabCount;
    public int defaultPoolCount = 20;
    public int incPoolCount = 10;
    private Dictionary<int, Queue<GameObject>> blockPoolDictionary = null;  //pool

 
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else if (instance != this)
        {
            Destroy(this.gameObject);
        }

        //make block info dictionary from editor
        MakeBlockInfoDictionary();
        

        GameObject instantGameObj = null;

        geoPrefabCount = blockPrefabInfo_dictionary.Count;    //set block total count
        blockPoolDictionary = new Dictionary<int, Queue<GameObject>>();

        foreach(KeyValuePair<int , GameObject> entry in blockPrefabInfo_dictionary)
        {
            blockPoolDictionary.Add(entry.Key, new Queue<GameObject>());

            for (int loop = 0; loop < defaultPoolCount; loop++)
            {
                instantGameObj = Instantiate(entry.Value);
                instantGameObj.SetActive(false);
                blockPoolDictionary[entry.Key].Enqueue(instantGameObj);
            }
        }
        
    }

    public GameObject GetGameObjectFromPool(int geoCode, Vector3 targetPosition, Quaternion targetQuaternion)
    {
        GameObject instantGameObj = null;
        if(geoCode == 0)
        {
            Debug.Log("air block. do not call getGameObjectFromPool!");
            return null;
        }

        if(blockPoolDictionary[geoCode].Count != 0)
        {
            //return pool gameobject
            GameObject gob = blockPoolDictionary[geoCode].Dequeue();
            gob.transform.position = targetPosition;
            gob.transform.rotation = targetQuaternion;
            gob.SetActive(true);
            return gob;
        }
        else
        {
            //create pool object;
            for (int loop = 0; loop < incPoolCount; loop++)
            {
                instantGameObj = Instantiate(blockPrefabInfo_dictionary[geoCode]);
                instantGameObj.transform.position = targetPosition;
                instantGameObj.transform.rotation = targetQuaternion;
                blockPoolDictionary[geoCode].Enqueue(instantGameObj);
            }
            //return pool gameobject
            GameObject gob = blockPoolDictionary[geoCode].Dequeue();
            instantGameObj.SetActive(true);
            return gob;
        }
    }


    public void ReturnGameObjectToPool(GameObject gob)
    {
        
        gob.SetActive(false);
        blockPoolDictionary[gob.GetComponent<BlockPrefab>().m_geoCode].Enqueue(gob);
    }
    
    public void MakeBlockInfoDictionary()
    {
        blockPrefabInfo_dictionary = new Dictionary<int, GameObject>();

        foreach (GameObject entry in blockPrefabList)
        {
            blockPrefabInfo_dictionary.Add(entry.GetComponent<BlockPrefab>().m_geoCode, entry);
        }
    }
}
