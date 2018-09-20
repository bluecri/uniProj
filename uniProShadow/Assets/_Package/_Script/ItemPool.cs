using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPool : MonoBehaviour {
    public static ItemPool instance = null;

    public List<GameObject> blockItemPrefabList;  //for editor
    public Dictionary<int, GameObject> blockItemPrefabInfo_dictionary;    //block init info. [GeoCode]-> BlockInfo

    public List<GameObject> blockItem3DPrefabList;  //for editor
    public Dictionary<int, GameObject> blockItem3DPrefabInfo_dictionary;    //block init info. [GeoCode]-> BlockInfo

    public List<GameObject> blockItemHandPrefabList;  //for editor
    public Dictionary<int, GameObject> blockItemHandPrefabInfo_dictionary;    //block init info. [GeoCode]-> BlockInfo

    public int defaultPoolCount = 10;
    public int incPoolCount = 2;
    private Dictionary<int, Queue<GameObject>> blockPoolDictionary = null;  //pool
    private Dictionary<int, Queue<GameObject>> blockPool3DDictionary = null;  //pool
    private Dictionary<int, Queue<GameObject>> blockPoolHandDictionary = null;  //pool


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
        
        blockPoolDictionary = new Dictionary<int, Queue<GameObject>>();

        foreach (KeyValuePair<int, GameObject> entry in blockItemPrefabInfo_dictionary)
        {
            blockPoolDictionary.Add(entry.Key, new Queue<GameObject>());

            for (int loop = 0; loop < defaultPoolCount; loop++)
            {
                instantGameObj = Instantiate(entry.Value);
                instantGameObj.SetActive(false);
                blockPoolDictionary[entry.Key].Enqueue(instantGameObj);
            }
        }

        blockPool3DDictionary = new Dictionary<int, Queue<GameObject>>();

        foreach (KeyValuePair<int, GameObject> entry in blockItem3DPrefabInfo_dictionary)
        {
            blockPool3DDictionary.Add(entry.Key, new Queue<GameObject>());

            for (int loop = 0; loop < defaultPoolCount; loop++)
            {
                instantGameObj = Instantiate(entry.Value);
                instantGameObj.SetActive(false);
                blockPool3DDictionary[entry.Key].Enqueue(instantGameObj);
            }
        }

        blockPoolHandDictionary = new Dictionary<int, Queue<GameObject>>();

        foreach (KeyValuePair<int, GameObject> entry in blockItemHandPrefabInfo_dictionary)
        {
            blockPoolHandDictionary.Add(entry.Key, new Queue<GameObject>());

            for (int loop = 0; loop < defaultPoolCount; loop++)
            {
                instantGameObj = Instantiate(entry.Value);
                instantGameObj.SetActive(false);
                blockPoolHandDictionary[entry.Key].Enqueue(instantGameObj);
            }
        }
    }

    public GameObject GetItemBlockGameObjectFromPool(int geoCode)
    {
        GameObject instantGameObj = null;
        if (geoCode == 0)
        {
            Debug.Log("air block. do not call getGameObjectFromPool!");
            return null;
        }

        if (blockPoolDictionary[geoCode].Count != 0)
        {
            //return pool gameobject
            GameObject gob = blockPoolDictionary[geoCode].Dequeue();
            gob.SetActive(true);
            return gob;
        }
        else
        {
            //create pool object;
            for (int loop = 1; loop < incPoolCount; loop++)
            {
                instantGameObj = Instantiate(blockItemPrefabInfo_dictionary[geoCode].GetComponent<BlockPrefab>().m_geoPrefab);
                blockPoolDictionary[geoCode].Enqueue(instantGameObj);
            }
            //return pool gameobject
            GameObject gob = blockPoolDictionary[geoCode].Dequeue();
            instantGameObj.SetActive(true);
            return gob;
        }
    }

    public GameObject GetItem3DBlockGameObjectFromPool(int geoCode)
    {
        GameObject instantGameObj = null;
        if (geoCode == 0)
        {
            Debug.Log("air block. do not call getGameObjectFromPool!");
            return null;
        }

        if (blockPool3DDictionary[geoCode].Count != 0)
        {
            //return pool gameobject
            GameObject gob = blockPool3DDictionary[geoCode].Dequeue();
            gob.SetActive(true);
            return gob;
        }
        else
        {
            //create pool object;
            for (int loop = 1; loop < incPoolCount; loop++)
            {
                instantGameObj = Instantiate(blockItem3DPrefabInfo_dictionary[geoCode].GetComponent<BlockPrefab>().m_geoPrefab);
                blockPool3DDictionary[geoCode].Enqueue(instantGameObj);
            }
            //return pool gameobject
            GameObject gob = blockPool3DDictionary[geoCode].Dequeue();
            instantGameObj.SetActive(true);
            return gob;
        }
    }

    public GameObject GetItemHandBlockGameObjectFromPool(int geoCode)
    {
        GameObject instantGameObj = null;
        if (geoCode == 0)
        {
            Debug.Log("air block. do not call getGameObjectFromPool!");
            return null;
        }

        if (blockPoolHandDictionary[geoCode].Count != 0)
        {
            //return pool gameobject
            GameObject gob = blockPoolHandDictionary[geoCode].Dequeue();
            gob.SetActive(true);
            return gob;
        }
        else
        {
            //create pool object;
            for (int loop = 1; loop < incPoolCount; loop++)
            {
                instantGameObj = Instantiate(blockItemHandPrefabInfo_dictionary[geoCode].GetComponent<BlockPrefab>().m_geoPrefab);
                blockPoolHandDictionary[geoCode].Enqueue(instantGameObj);
            }
            //return pool gameobject
            GameObject gob = blockPoolHandDictionary[geoCode].Dequeue();
            instantGameObj.SetActive(true);
            return gob;
        }
    }


    public void ReturnGameObjectToPool(GameObject gob)
    {
        gob.SetActive(false);
        blockPoolDictionary[gob.GetComponent<BlockPrefab>().m_geoCode].Enqueue(gob);
    }

    public void Return3DGameObjectToPool(GameObject gob)
    {
        gob.SetActive(false);
        blockPool3DDictionary[gob.GetComponent<BlockPrefab>().m_geoCode].Enqueue(gob);
    }

    public void ReturnHandGameObjectToPool(GameObject gob)
    {
        gob.SetActive(false);
        blockPoolHandDictionary[gob.GetComponent<BlockPrefab>().m_geoCode].Enqueue(gob);
    }

    public void MakeBlockInfoDictionary()
    {
        blockItemPrefabInfo_dictionary = new Dictionary<int, GameObject>();

        foreach (GameObject entry in blockItemPrefabList)
        {
            blockItemPrefabInfo_dictionary.Add(entry.GetComponent<BlockPrefab>().m_geoCode, entry);
        }

        blockItem3DPrefabInfo_dictionary = new Dictionary<int, GameObject>();

        foreach (GameObject entry in blockItem3DPrefabList)
        {
            blockItem3DPrefabInfo_dictionary.Add(entry.GetComponent<BlockPrefab>().m_geoCode, entry);
        }

        blockItemHandPrefabInfo_dictionary = new Dictionary<int, GameObject>();

        foreach (GameObject entry in blockItemHandPrefabList)
        {
            blockItemHandPrefabInfo_dictionary.Add(entry.GetComponent<BlockPrefab>().m_geoCode, entry);
        }
    }
}
