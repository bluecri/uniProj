using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class SceneManager : MonoBehaviour {
    public static SceneManager instance = null;
    public static Player playerInstance = null;
    public GameObject playerPrefab;
    public GameObject[] geoPrefab;

    public const int fileXNum = 16;
    public const int fileZNum = 16;
    public const int fileYNum = 128;
    public const int fileBlockTypeSize = sizeof(int) + sizeof(bool);
    public const int fileBlockSize = fileBlockTypeSize * fileXNum * fileZNum * fileYNum;



    public int[,] preLoadOnPrefabGeometryIndex;
    public int[,] preLoadOnMemGeometryIndex;
    public int[,] preLoadOnTempGeometryIndex;

    public const int prefabGeoIndexSize = 3;    //3x3
    public const int memGeoIndexSize = 5;    //3x3
    public const int tempGeoIndexSize = 7;   //5x5

    public HashSet<KeyValuePair<int, int>> curLoadedTempFilePairList = new HashSet<KeyValuePair<int, int>>();

    public HashSet<KeyValuePair<int, int>> curLoadedMemPairList = new HashSet<KeyValuePair<int, int>>();
    public Dictionary<KeyValuePair<int, int>, byte[]> curLoadedMem = new Dictionary<KeyValuePair<int, int>, byte[]>();

    public HashSet<KeyValuePair<int, int>> curLoadedPrefabPairList = new HashSet<KeyValuePair<int, int>>();
    public Dictionary<KeyValuePair<int, int>, Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]>> curLoadedPrefab = new Dictionary<KeyValuePair<int, int>, Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]>> ();


    public int playerBlockOriginTargetX = 0;
    public int playerBlockOriginTargetZ = 0;

    private IEnumerator corutineLoadGeometry;
    private IEnumerator corutineSaveGeometry;
    private IEnumerator corutineMainGeometry;

    private PlayerData tempPlayerLoadedDataForInstantize;

    public void Awake()
    {
        //singleton

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        //constant
        preLoadOnPrefabGeometryIndex = new int[prefabGeoIndexSize * prefabGeoIndexSize, 2];
        preLoadOnMemGeometryIndex = new int[memGeoIndexSize * memGeoIndexSize, 2];
        preLoadOnTempGeometryIndex = new int[tempGeoIndexSize * tempGeoIndexSize, 2];
        int index = 0;
        for (int i = 0; i < prefabGeoIndexSize; i++)
        {
            int inVal1 = -(prefabGeoIndexSize / 2) + i;
            for (int k = 0; k < prefabGeoIndexSize; k++)
            {
                int inVal2 = -(prefabGeoIndexSize / 2) + k;
                preLoadOnPrefabGeometryIndex[index, 0] = inVal1;
                preLoadOnPrefabGeometryIndex[index, 1] = inVal2;
                index++;
            }
        }
        index = 0;
        for (int i=0; i< memGeoIndexSize; i++)
        {
            int inVal1 = -(memGeoIndexSize / 2) + i;
            for (int k = 0; k < memGeoIndexSize; k++)
            {
                int inVal2 = -(memGeoIndexSize / 2) + k;
                preLoadOnMemGeometryIndex[index, 0] = inVal1;
                preLoadOnMemGeometryIndex[index, 1] = inVal2;
                index++;
            }
        }
        index = 0;
        for (int i = 0; i < tempGeoIndexSize; i++)
        {
            int inVal1 = -(tempGeoIndexSize/2) + i;
            for (int k = 0; k < tempGeoIndexSize; k++)
            {
                int inVal2 = -(tempGeoIndexSize / 2) + k;
                preLoadOnTempGeometryIndex[index, 0] = inVal1;
                preLoadOnTempGeometryIndex[index, 1] = inVal2;
                index++;
            }
        }

        //user info load & player instatitate
        LoadPlayerInfo();

        GetPlayerTarget(new Vector3(tempPlayerLoadedDataForInstantize.posX, tempPlayerLoadedDataForInstantize.posY, tempPlayerLoadedDataForInstantize.posZ), out playerBlockOriginTargetX, out playerBlockOriginTargetZ);

        LoadOriginGeometryWithUserPos(playerBlockOriginTargetX, playerBlockOriginTargetZ);
        LoadTempGeometryWithUserPos(playerBlockOriginTargetX, playerBlockOriginTargetZ);
        LoadMemWithUserPos(playerBlockOriginTargetX, playerBlockOriginTargetZ);

        InstantizePlayer();

        //set corutine
        corutineLoadGeometry = LoadGeometryUpdateCorutine();
        corutineSaveGeometry = SaveGeometryUpdateCorutine();
        corutineMainGeometry = GeomeTryUpdateCorutine();

        StartCoroutine(corutineMainGeometry);

    }
    IEnumerator GeomeTryUpdateCorutine()
    {
        while (true)
        {
            Vector3 playerPosition = playerInstance.transform.position;
            int playerBlockTargetX = ((int)playerPosition.x);
            int playerBlockTargetZ = ((int)playerPosition.z);
            if (playerBlockTargetX < 0)
            {
                playerBlockTargetX -= (fileXNum - 1);
            }
            if (playerBlockTargetZ < 0)
            {
                playerBlockTargetZ -= (fileZNum - 1);
            }
            playerBlockTargetX /= fileXNum;
            playerBlockTargetZ /= fileZNum;

            if (playerBlockTargetX != playerBlockOriginTargetX || playerBlockTargetZ != playerBlockOriginTargetZ)
            {
                //stop 2 corutine
                StopCoroutine(corutineLoadGeometry);
                //StopCoroutine(corutineSaveGeometry);

                //origin block target X, Z update
                playerBlockOriginTargetX = playerBlockTargetX;
                playerBlockOriginTargetZ = playerBlockTargetZ;

                //start 2 corutine
                corutineLoadGeometry = LoadGeometryUpdateCorutine();
                corutineSaveGeometry = SaveGeometryUpdateCorutine();

                StartCoroutine(corutineLoadGeometry);
                StartCoroutine(corutineSaveGeometry);

            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    IEnumerator LoadGeometryUpdateCorutine()
    {
        HashSet<KeyValuePair<int, int>> needToLoadTempGeoSetForLoop = new HashSet<KeyValuePair<int, int>>();
        //HashSet<KeyValuePair<int, int>> needToLoadTempGeoSetForCompare; //change to list is better performance?

        HashSet<KeyValuePair<int, int>> needToLoadMemGeoSetForLoop = new HashSet<KeyValuePair<int, int>>();
        //HashSet<KeyValuePair<int, int>> needToLoadMemGeoSetForCompare; //change to list is better performance?

        HashSet<KeyValuePair<int, int>> needToLoadPrefabGeoSetForLoop = new HashSet<KeyValuePair<int, int>>();
        //HashSet<KeyValuePair<int, int>> needToLoadPrefabGeoSetForCompare; //change to list is better performance?


        for (int i = 0; i < preLoadOnTempGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnTempGeometryIndex[i, 0];
            int accZ = preLoadOnTempGeometryIndex[i, 1];
            int targetX = accX + playerBlockOriginTargetX;
            int targetZ = accZ + playerBlockOriginTargetZ;
            needToLoadTempGeoSetForLoop.Add(new KeyValuePair<int, int>(targetX, targetZ));
        }
        needToLoadTempGeoSetForLoop.ExceptWith(curLoadedTempFilePairList); //target temp geo - cur loaded temp geo = need to load temp geo
        //needToLoadTempGeoSetForCompare = new HashSet<KeyValuePair<int, int>>(needToLoadTempGeoSetForLoop);
        IEnumerator<KeyValuePair<int, int>> tempGeoEnumerator = needToLoadTempGeoSetForLoop.GetEnumerator();
        tempGeoEnumerator.Reset();
        bool isTempGeoLoop = tempGeoEnumerator.MoveNext();


        for (int i = 0; i < preLoadOnMemGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnMemGeometryIndex[i, 0];
            int accZ = preLoadOnMemGeometryIndex[i, 1];
            int targetX = accX + playerBlockOriginTargetX;
            int targetZ = accZ + playerBlockOriginTargetZ;
            needToLoadMemGeoSetForLoop.Add(new KeyValuePair<int, int>(targetX, targetZ));
        }
        needToLoadMemGeoSetForLoop.ExceptWith(curLoadedMemPairList); //target temp geo - cur loaded temp geo = need to load temp geo
        //needToLoadMemGeoSetForCompare = new HashSet<KeyValuePair<int, int>>(needToLoadMemGeoSetForLoop);
        IEnumerator<KeyValuePair<int, int>> memGeoEnumerator = needToLoadMemGeoSetForLoop.GetEnumerator();
        memGeoEnumerator.Reset();
        bool isMemGeoLoop = memGeoEnumerator.MoveNext();


        for (int i = 0; i < preLoadOnPrefabGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnPrefabGeometryIndex[i, 0];
            int accZ = preLoadOnPrefabGeometryIndex[i, 1];
            int targetX = accX + playerBlockOriginTargetX;
            int targetZ = accZ + playerBlockOriginTargetZ;
            needToLoadPrefabGeoSetForLoop.Add(new KeyValuePair<int, int>(targetX, targetZ));
        }
        needToLoadPrefabGeoSetForLoop.ExceptWith(curLoadedPrefabPairList); //target temp geo - cur loaded temp geo = need to load temp geo
        //needToLoadPrefabGeoSetForCompare = new HashSet<KeyValuePair<int, int>>(needToLoadPrefabGeoSetForLoop);
        IEnumerator<KeyValuePair<int, int>> prefabGeoEnumerator = needToLoadPrefabGeoSetForLoop.GetEnumerator();
        prefabGeoEnumerator.Reset();
        bool isPrefabGeoLoop = prefabGeoEnumerator.MoveNext();


        while (isTempGeoLoop)
        {
            if (!curLoadedTempFilePairList.Contains(new KeyValuePair<int, int>(tempGeoEnumerator.Current.Key, tempGeoEnumerator.Current.Value)))    // has not temp file
            {
                //load origin to temp or create temp.
                LoadOriginGeometry(tempGeoEnumerator.Current.Key, tempGeoEnumerator.Current.Value);
            }
            else
            {
                //already has temp.
            }
            isTempGeoLoop = tempGeoEnumerator.MoveNext();
            yield return new WaitForSeconds(0.1f);

            while (isMemGeoLoop)
            {
                //check already in mem
                if (!curLoadedMemPairList.Contains(new KeyValuePair<int, int>(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value)))
                {
                    if (curLoadedTempFilePairList.Contains(new KeyValuePair<int, int>(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value)))
                    {
                        //load temp to mem
                        LoadTempGeometry(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value);
                    }
                    else
                    {
                        //not in temp. Need to wait temp load.
                        break;
                    }
                }
                else
                {
                    //already has mem
                }
                isMemGeoLoop = memGeoEnumerator.MoveNext();
                yield return new WaitForSeconds(0.1f);


                while (isPrefabGeoLoop)
                {
                    //check already in prefab
                    if (!curLoadedPrefabPairList.Contains(new KeyValuePair<int, int>(prefabGeoEnumerator.Current.Key, prefabGeoEnumerator.Current.Value)))
                    {
                        //check mem exists
                        if (curLoadedMemPairList.Contains(new KeyValuePair<int, int>(prefabGeoEnumerator.Current.Key, prefabGeoEnumerator.Current.Value)))
                        {
                            //load mem to prefab
                            LoadMemGeometry(prefabGeoEnumerator.Current.Key, prefabGeoEnumerator.Current.Value);
                        }
                        else
                        {
                            //not in temp. Need to wait temp load.
                            break;
                        }
                    }
                    else
                    {
                        //already has mem
                    }
                    isPrefabGeoLoop = prefabGeoEnumerator.MoveNext();
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }

        yield return new WaitForSeconds(1.0f);
    }

    IEnumerator SaveGeometryUpdateCorutine()
    {
        HashSet<KeyValuePair<int, int>> needToLoadMemGeoSetForLoop = new HashSet<KeyValuePair<int, int>>();
        HashSet<KeyValuePair<int, int>> needToLoadPrefabGeoSetForLoop = new HashSet<KeyValuePair<int, int>>();

        HashSet<KeyValuePair<int, int>> needToSaveMemGeoSetForLoop;
        HashSet<KeyValuePair<int, int>> needToSaavePrefabGeoSetForLoop;

        for (int i = 0; i < preLoadOnMemGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnMemGeometryIndex[i, 0];
            int accZ = preLoadOnMemGeometryIndex[i, 1];
            int targetX = accX + playerBlockOriginTargetX;
            int targetZ = accZ + playerBlockOriginTargetZ;
            needToLoadMemGeoSetForLoop.Add(new KeyValuePair<int, int>(targetX, targetZ));
        }
        //need to save mem = cur loaded mem - need to load mem
        needToSaveMemGeoSetForLoop = new HashSet<KeyValuePair<int, int>>(curLoadedMemPairList);
        needToSaveMemGeoSetForLoop.ExceptWith(needToLoadMemGeoSetForLoop);
        IEnumerator<KeyValuePair<int, int>> memGeoEnumerator = needToSaveMemGeoSetForLoop.GetEnumerator();
        memGeoEnumerator.Reset();
        bool isMemGeoLoop = memGeoEnumerator.MoveNext();


        for (int i = 0; i < preLoadOnPrefabGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnPrefabGeometryIndex[i, 0];
            int accZ = preLoadOnPrefabGeometryIndex[i, 1];
            int targetX = accX + playerBlockOriginTargetX;
            int targetZ = accZ + playerBlockOriginTargetZ;
            needToLoadPrefabGeoSetForLoop.Add(new KeyValuePair<int, int>(targetX, targetZ));
        }
        //need to save prefab = cur loaded prefab - need to load prefab
        needToSaavePrefabGeoSetForLoop = new HashSet<KeyValuePair<int, int>>(curLoadedPrefabPairList);
        needToSaavePrefabGeoSetForLoop.ExceptWith(needToLoadPrefabGeoSetForLoop);
        IEnumerator<KeyValuePair<int, int>> prefabGeoEnumerator = needToSaavePrefabGeoSetForLoop.GetEnumerator();
        prefabGeoEnumerator.Reset();
        bool isPrefabGeoLoop = prefabGeoEnumerator.MoveNext();


        while (isMemGeoLoop)
        {
            //check has mem
            if(curLoadedMemPairList.Contains(new KeyValuePair<int, int>(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value))){
                //check not in prefab. Only in mem.
                if (!curLoadedPrefabPairList.Contains(new KeyValuePair<int, int>(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value)))
                {
                    SaveMemGeometry(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value);
                }
                else
                {
                    //need to wait.(wait : prefab -> mem)
                }
            }
            else
            {
                //already deleted from mem to temp
            }
            memGeoEnumerator.MoveNext();
            yield return new WaitForSeconds(0.1f);

            //check has prefab
            if (curLoadedPrefabPairList.Contains(new KeyValuePair<int, int>(prefabGeoEnumerator.Current.Key, prefabGeoEnumerator.Current.Value)))
            {
                SavePrefabGeometry(prefabGeoEnumerator.Current.Key, prefabGeoEnumerator.Current.Value);
            }
            else
            {
                //already deleted prefab to mem
            }
            prefabGeoEnumerator.MoveNext();
            yield return new WaitForSeconds(0.1f);
        }




        yield return new WaitForSeconds(1.0f);
    }

    


    public void SavePlayerInfo()
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Create);

            PlayerData playerData = new PlayerData();
            Vector3 playerPos = playerInstance.transform.position;
            playerData.posX =playerPos.x;
            playerData.posY =playerPos.y;
            playerData.posZ =playerPos.z;

            Quaternion playerQuat = playerInstance.transform.rotation;
            playerData.quatX = playerQuat.x;
            playerData.quatY = playerQuat.y;
            playerData.quatZ = playerQuat.z;
            playerData.quatW = playerQuat.w;
            
            bf.Serialize(file, playerData);

            file.Close();
        }
        catch (IOException e)
        {
            Debug.Log(e.ToString() + "SavePlayerInfo save error");
        }
    }


    public void LoadPlayerInfo()
    {
        if(File.Exists(Application.persistentDataPath + "/playerInfo.dat"))
        {
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Open);
                tempPlayerLoadedDataForInstantize = (PlayerData)bf.Deserialize(file);
                file.Close();
            }
            catch (IOException e)
            {
                Debug.Log(e.ToString() + "LoadPlayerInfo Load error");
                tempPlayerLoadedDataForInstantize = new PlayerData();
                tempPlayerLoadedDataForInstantize.posX = 0;
                tempPlayerLoadedDataForInstantize.posY = 100;
                tempPlayerLoadedDataForInstantize.posZ = 0;

                Quaternion tempQuat = new Quaternion();
                tempPlayerLoadedDataForInstantize.quatX = tempQuat.x;
                tempPlayerLoadedDataForInstantize.quatY = tempQuat.y;
                tempPlayerLoadedDataForInstantize.quatZ = tempQuat.z;
                tempPlayerLoadedDataForInstantize.quatW = tempQuat.w;
            }
        }
        else
        {
            tempPlayerLoadedDataForInstantize = new PlayerData();
            tempPlayerLoadedDataForInstantize.posX = 0;
            tempPlayerLoadedDataForInstantize.posY = 100;
            tempPlayerLoadedDataForInstantize.posZ = 0;

            Quaternion tempQuat = new Quaternion();
            tempPlayerLoadedDataForInstantize.quatX = tempQuat.x;
            tempPlayerLoadedDataForInstantize.quatY = tempQuat.y;
            tempPlayerLoadedDataForInstantize.quatZ = tempQuat.z;
            tempPlayerLoadedDataForInstantize.quatW = tempQuat.w;
        }
    }
    public void InstantizePlayer()
    {
        Vector3 playerPosVec = new Vector3(tempPlayerLoadedDataForInstantize.posX, tempPlayerLoadedDataForInstantize.posY, tempPlayerLoadedDataForInstantize.posZ);
        Quaternion playerQuat = new Quaternion(tempPlayerLoadedDataForInstantize.quatX, tempPlayerLoadedDataForInstantize.quatY, tempPlayerLoadedDataForInstantize.quatZ, tempPlayerLoadedDataForInstantize.quatW);
        playerInstance = Instantiate(playerPrefab, playerPosVec, playerQuat).GetComponent<Player>();
    }

    public void LoadOriginGeometryWithUserPos(int playerBlockTargetX, int playerBlockTargetZ)
    {
        for (int i = 0; i < preLoadOnTempGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnTempGeometryIndex[i, 0];
            int accZ = preLoadOnTempGeometryIndex[i, 1];
            int targetX = accX + playerBlockTargetX;
            int targetZ = accZ + playerBlockTargetZ;
            if (!curLoadedTempFilePairList.Contains(new KeyValuePair<int, int>(targetX, targetZ)))    // has not temp file
            { 
                LoadOriginGeometry(targetX, targetZ);
            }
        }
        
    }

    public void LoadTempGeometryWithUserPos(int playerBlockTargetX, int playerBlockTargetZ)
    {
        for (int i = 0; i < preLoadOnMemGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnMemGeometryIndex[i, 0];
            int accZ = preLoadOnMemGeometryIndex[i, 1];
            int targetX = accX + playerBlockTargetX;
            int targetZ = accZ + playerBlockTargetZ;
            if(curLoadedTempFilePairList.Contains(new KeyValuePair<int, int>(targetX, targetZ)))    // has temp file
            {
                if(!curLoadedMemPairList.Contains(new KeyValuePair<int, int>(targetX, targetZ))) //not has mem
                {
                    LoadTempGeometry(targetX, targetZ);
                }
                else
                {
                    //already in mem.
                    //skip
                }
                
            }
            else
            {
                //not yet loaded temp file
                //wait
            }
        }
    }

    public void LoadMemWithUserPos(int playerBlockTargetX, int playerBlockTargetZ)
    {
        for (int i = 0; i < preLoadOnPrefabGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnPrefabGeometryIndex[i, 0];
            int accZ = preLoadOnPrefabGeometryIndex[i, 1];
            int targetX = accX + playerBlockTargetX;
            int targetZ = accZ + playerBlockTargetZ;
            if (curLoadedMemPairList.Contains(new KeyValuePair<int, int>(targetX, targetZ)))    // has temp file
            {
                if (!curLoadedPrefabPairList.Contains(new KeyValuePair<int, int>(targetX, targetZ))) //not has prefab
                {
                    LoadMemGeometry(targetX, targetZ);
                }
                else
                {
                    //already has prefab.
                    //skip
                }

            }
            else
            {
                //not yet loaded temp file
                //wait
            }
        }
    }
    
    
    //load origin geometry- > temp geometry
    //register to map.
    public void LoadOriginGeometry(int targetX, int targetZ)
    {
        byte[] tempGeometry = null;
        tempGeometry = new byte[fileBlockSize];

        BinaryReader br;
        try
        {
            //read origin file
            br = new BinaryReader(new FileStream(Application.persistentDataPath + "/map_" + targetX + "_" + targetZ + ".dat", FileMode.Open));
            br.Read(tempGeometry, 0, fileBlockSize);

            //write to temp file
            BinaryWriter bw;
            try
            {
                bw = new BinaryWriter(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.CreateNew));
                bw.Write(tempGeometry, 0, fileBlockSize);
                bw.Close();
                curLoadedTempFilePairList.Add(new KeyValuePair<int, int>(targetX, targetZ));
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n LoadOriginGeometry : temp is already exist. use temp file!");
            }

            //tempGeoMap.Add(new KeyValuePair<int, int>(targetX, targetZ), tempGeometry);
            br.Close();
        }
        catch (IOException e)
        {
            Debug.Log(e.Message + "\n Cannot open origin file. Create temp geometry file!!");
            bool tf = CreateTempMap(targetX, targetZ, ref tempGeometry);
            // tempGeoMap.Add(new KeyValuePair<int, int>(targetX, targetZ), tempGeometry);
        }
    }

    
    //temp to mem
    public void LoadTempGeometry(int targetX, int targetZ)
    {
        byte[] tempGeometry = null;
        tempGeometry = new byte[fileBlockSize];
        BinaryReader br;
        try
        {
            br = new BinaryReader(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.Open));
            br.Read(tempGeometry, 0, fileBlockSize);
            curLoadedMem.Add(new KeyValuePair<int, int>(targetX, targetZ), tempGeometry);
            br.Close();
            curLoadedMemPairList.Add(new KeyValuePair<int, int>(targetX, targetZ));
        }
        catch (IOException e)
        {
            Debug.Log(e.Message + "\n Cannot read temp file. LoadTempGeometry");
        }
    }

    //mem to prefab
    public void LoadMemGeometry(int targetX, int targetZ)
    {
        Byte[] bytes;
        curLoadedMem.TryGetValue(new KeyValuePair<int, int>(targetX, targetZ), out bytes);
        //instance
        int startIndex = 0;
        Quaternion targetQuat = new Quaternion();
        Vector3 targetPosition = new Vector3();

        Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]> targetXZDictionary = new Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]>();

        for (int x = targetX * fileXNum; x < targetX * fileXNum + fileXNum; x++)
        {
            targetPosition.x = x;
            for (int z = targetZ * fileZNum; z < targetZ * fileZNum + fileZNum; z++)
            {
                targetPosition.z = z;
                BlockPrefabClass[] blockPrefabClassArray = new BlockPrefabClass[fileYNum];
                for (int y = 0; y < fileYNum; y++)
                {
                    blockPrefabClassArray[y] = new BlockPrefabClass();
                    targetPosition.y = y;
                    int loadedGeoType = BitConverter.ToInt32(bytes, startIndex);    //todo : invalid int
                    startIndex += 4;
                    bool loadedInstant = BitConverter.ToBoolean(bytes, startIndex);    //todo : invalid int
                    startIndex += 1;
                    if (loadedGeoType == 0)
                    {

                        //air
                        blockPrefabClassArray[y].isAir = true;
                        blockPrefabClassArray[y].blockType = loadedGeoType;
                        blockPrefabClassArray[y].gameObject = null;
                        blockPrefabClassArray[y].isInstantize = false;
                        blockPrefabClassArray[y].isRender = false;
                    }
                    else
                    {
                        //dirt

                        blockPrefabClassArray[y].isAir = false;
                        blockPrefabClassArray[y].blockType = loadedGeoType;

                        if (loadedInstant)
                        {
                            blockPrefabClassArray[y].gameObject = Instantiate(geoPrefab[loadedGeoType], targetPosition, targetQuat);
                            MeshRenderer meshRenderer = blockPrefabClassArray[y].gameObject.GetComponent<MeshRenderer>();
                            MeshCollider meshCollider = blockPrefabClassArray[y].gameObject.GetComponent<MeshCollider>();


                            meshRenderer.enabled = true;
                            meshCollider.enabled = true;
                            blockPrefabClassArray[y].isInstantize = true;
                            blockPrefabClassArray[y].isRender = true;
                        }
                        else
                        {
                            blockPrefabClassArray[y].gameObject = null;
                            blockPrefabClassArray[y].isInstantize = false;
                            blockPrefabClassArray[y].isRender = false;
                        }
                    }
                }
                targetXZDictionary.Add(new KeyValuePair<int, int>(x, z), blockPrefabClassArray);
            }


        }
        //public Dictionary<KeyValuePair<int, int>,  Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]>  > curLoadedPrefab
        //curLoadedPrefab.Add(new KeyValuePair<int, int>(targetX, targetZ), targetXZDictionary);    //prefab list add

        curLoadedPrefab.Add(new KeyValuePair<int, int>(targetX, targetZ), targetXZDictionary);    //add dictionary(<realx, realz>, array)
        curLoadedPrefabPairList.Add(new KeyValuePair<int, int>(targetX, targetZ));  //pair list add
    }



    public void SavePrefabGeometry(int targetX, int targetZ)
    {
        Byte[] bytes;
        curLoadedMem.TryGetValue(new KeyValuePair<int, int>(targetX, targetZ), out bytes);

        //instance
        int startIndex = 0;
        Quaternion targetQuat = new Quaternion();
        Vector3 targetPosition = new Vector3();

        Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]> targetXZDictionary;
        curLoadedPrefab.TryGetValue(new KeyValuePair<int, int>(targetX, targetZ), out targetXZDictionary);

        for (int x = targetX * fileXNum; x < targetX * fileXNum + fileXNum; x++)
        {
            targetPosition.x = x;
            for (int z = targetZ * fileZNum; z < targetZ * fileZNum + fileZNum; z++)
            {
                targetPosition.z = z;
                
                BlockPrefabClass[] blockPrefabClassArray;
                targetXZDictionary.TryGetValue(new KeyValuePair<int, int>(x, z), out blockPrefabClassArray);
                for (int y = 0; y < fileYNum; y++)
                {
                    targetPosition.y = y;
                    BlockPrefabClass bpc = blockPrefabClassArray[y];

                    Byte[] saveBytes = BitConverter.GetBytes(bpc.blockType);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);
                    for(int i=0; i<saveBytes.Length; i++, startIndex++)
                    {
                        bytes[startIndex] = saveBytes[i];
                    }

                    saveBytes = BitConverter.GetBytes(bpc.isInstantize);
                    if (!BitConverter.IsLittleEndian)
                        Array.Reverse(bytes);
                    for (int i = 0; i < saveBytes.Length; i++, startIndex++)
                    {
                        bytes[startIndex] = saveBytes[i];
                    }

                    if(bpc.gameObject != null)
                    {
                        //remove gameobjcet
                        Destroy(bpc.gameObject);
                    }
                    
                }
                blockPrefabClassArray = null;
                targetXZDictionary.Remove(new KeyValuePair<int, int>(x, z));
            }
        }

        targetXZDictionary = null;

        curLoadedPrefab.Remove(new KeyValuePair<int, int>(targetX, targetZ));
        curLoadedPrefabPairList.Remove(new KeyValuePair<int, int>(targetX, targetZ));
    }


    //temp to mem
    public void SaveMemGeometry(int targetX, int targetZ)
    {
        byte[] tempGeometry = null;
        curLoadedMem.TryGetValue(new KeyValuePair<int, int>(targetX, targetZ), out tempGeometry);
        //tempGeometry = new byte[fileBlockSize * fileXNum * fileZNum * fileYNum];
        BinaryWriter bw;
        try
        {
            bw = new BinaryWriter(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.Create));
            bw.Write(tempGeometry, 0, fileBlockSize);
            curLoadedMem.Remove(new KeyValuePair<int, int>(targetX, targetZ));
            bw.Close();
            curLoadedMemPairList.Remove(new KeyValuePair<int, int>(targetX, targetZ));
        }
        catch (IOException e)
        {
            Debug.Log(e.Message + "\n Cannot read temp file. LoadTempGeometry");
        }
    }

    public void SaveAllTempGeometry()
    {
        IEnumerator<KeyValuePair<int, int>> enumerator = curLoadedTempFilePairList.GetEnumerator();
        byte[] tempGeometry = new byte[fileBlockSize]; ;
        while (enumerator.MoveNext())
        {
            int targetX = enumerator.Current.Key;
            int targetZ = enumerator.Current.Value;

            try
            {
                BinaryReader br = new BinaryReader(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.Open));
                br.Read(tempGeometry, 0, fileBlockSize);
                br.Close();
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot open temp file. SaveAllTempGeometry");
            }

            try
            {
                BinaryWriter bw = new BinaryWriter(new FileStream(Application.persistentDataPath + "/map_" + targetX + "_" + targetZ + ".dat", FileMode.Create));
                bw.Write(tempGeometry, 0, fileBlockTypeSize * fileXNum * fileZNum * fileYNum);
                bw.Close();
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot create origin file. SaveAllTempGeometry");
            }
        }
    }

    public void DeletAddTempGeometry()
    {
        IEnumerator<KeyValuePair<int, int>> enumerator = curLoadedTempFilePairList.GetEnumerator();

        
        while (enumerator.MoveNext())
        {
            int targetX = enumerator.Current.Key;
            int targetZ = enumerator.Current.Value;

            File.Delete(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat");
            
        }
    }


    public void SaveGame()
    {
        SavePlayerInfo();

        HashSet<KeyValuePair<int, int>> test = new HashSet<KeyValuePair<int, int>>(curLoadedPrefabPairList);
        IEnumerator<KeyValuePair<int, int>> enumerator = new HashSet<KeyValuePair<int, int>>(curLoadedPrefabPairList).GetEnumerator();
        bool isEnumNotEmpty = enumerator.MoveNext();
        while (isEnumNotEmpty)
        {
            SavePrefabGeometry(enumerator.Current.Key, enumerator.Current.Value);
            isEnumNotEmpty = enumerator.MoveNext();
        }

        enumerator = new HashSet<KeyValuePair<int, int>>(curLoadedMemPairList).GetEnumerator();
        isEnumNotEmpty = enumerator.MoveNext();
        while (isEnumNotEmpty)
        {
            SaveMemGeometry(enumerator.Current.Key, enumerator.Current.Value);
            isEnumNotEmpty = enumerator.MoveNext();
        }
        
        SaveAllTempGeometry();

        DeletAddTempGeometry();
    }



    

    //create new temp map
    public bool CreateTempMap(int targetX, int targetZ, ref byte[] outByte)
    {
        int air = 0;
        int dirt = 1;
        
        int index = 0;
        for (int x = 0; x < fileXNum; x++)
        {
            for (int z = 0; z < fileZNum; z++)
            {
                int blockType = 0;
                int y = 0;

                blockType = dirt;
                byte[] bytes = BitConverter.GetBytes(blockType);
                bool isInstance = false;
                byte[] boolByte = BitConverter.GetBytes(isInstance);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                for (; y < 39; y++)
                {
                    for (int i = 0; i < bytes.Length; i++, index++)
                    {
                        outByte[index] = bytes[i];
                    }
                    for (int i = 0; i < boolByte.Length; i++, index++)
                    {
                        outByte[index] = boolByte[i];
                    }

                }
                isInstance = true; //최상위 block
                boolByte = BitConverter.GetBytes(isInstance);
                for (; y<40; y++)
                {
                    for (int i = 0; i < bytes.Length; i++, index++)
                    {
                        outByte[index] = bytes[i];
                    }
                    for (int i = 0; i < boolByte.Length; i++, index++)
                    {
                        outByte[index] = boolByte[i];
                    }
                }

                blockType = air;
                bytes = BitConverter.GetBytes(blockType);
                isInstance = false;
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(bytes);
                for (; y < fileYNum; y++)
                {
                    for (int i = 0; i < bytes.Length; i++, index++)
                    {
                        outByte[index] = bytes[i];
                    }
                    for (int i = 0; i < boolByte.Length; i++, index++)
                    {
                        outByte[index] = boolByte[i];
                    }
                }
            }
        }

        BinaryWriter bw;
        try
        {
            bw = new BinaryWriter(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.CreateNew));
            bw.Write(outByte, 0, fileBlockSize);
            bw.Close();
            curLoadedTempFilePairList.Add(new KeyValuePair<int, int>(targetX, targetZ));
        }
        catch (IOException e)
        {
            Debug.Log(e.Message + "\n Cannot create new temp file.");
            return false;
        }
        return true;
    }


    public void GetPlayerTarget(Vector3 userPosition, out int outPlayerBlockTargetX, out int outPlayerBlockTargetZ)
    {
        outPlayerBlockTargetX = ((int)userPosition.x);
        outPlayerBlockTargetZ = ((int)userPosition.z);
        if (outPlayerBlockTargetX < 0)
        {
            outPlayerBlockTargetX -= (fileXNum - 1);
        }
        if (outPlayerBlockTargetZ < 0)
        {
            outPlayerBlockTargetZ -= (fileZNum - 1);
        }
        outPlayerBlockTargetX /= fileXNum;
        outPlayerBlockTargetZ /= fileZNum;
        
    }

    void OnApplicationQuit()
    {
        StopCoroutine(corutineMainGeometry);
        StopCoroutine(corutineLoadGeometry);
        StopCoroutine(corutineSaveGeometry);
        SaveGame();
        //Debug.Log("Application ending after " + Time.time + " seconds");
    }

    //just check block is exist in x,y,z
    public bool IsBlockObjectExists(int x, int y, int z)
    {
        int targetX = x, targetZ = z;

        if (targetX < 0)
        {
            targetX -= (fileXNum - 1);
        }
        if (targetZ < 0)
        {
            targetZ -= (fileZNum - 1);
        }
        targetX /= fileXNum;
        targetZ /= fileZNum;

        //if block targetx, targety in prefabList
        Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]> targetBlockDictionary;
        BlockPrefabClass[] yBlockClassesWithXZ;
        if (curLoadedPrefab.TryGetValue(new KeyValuePair<int, int>(targetX, targetZ), out targetBlockDictionary))
        {
            if (targetBlockDictionary.TryGetValue(new KeyValuePair<int, int>(x, z), out yBlockClassesWithXZ))
            {
                if(yBlockClassesWithXZ[y].gameObject != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Debug.Log("error in AddBlock, targetBlockDictionary.TryGetValue(new KeyValuePair<int, int>(x, z), out yBlockClassesWithXZ)");
            }
        }
        else
        {
            //no in prefab list.
            Debug.Log("not in prefab list. IsBlockObjectExists");
        }
        return false;
    }

    //get block x,y,z ref
    public bool IsBlockObjectExists(int x, int y, int z, out BlockPrefabClass bpcRef)
    {
        int targetX = x, targetZ = z;

        if (targetX < 0)
        {
            targetX -= (fileXNum - 1);
        }
        if (targetZ < 0)
        {
            targetZ -= (fileZNum - 1);
        }
        targetX /= fileXNum;
        targetZ /= fileZNum;

        //if block targetx, targety in prefabList
        Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]> targetBlockDictionary;
        BlockPrefabClass[] yBlockClassesWithXZ;
        if (curLoadedPrefab.TryGetValue(new KeyValuePair<int, int>(targetX, targetZ), out targetBlockDictionary))
        {
            if (targetBlockDictionary.TryGetValue(new KeyValuePair<int, int>(x, z), out yBlockClassesWithXZ))
            {
                bpcRef = yBlockClassesWithXZ[y];
                if (yBlockClassesWithXZ[y].gameObject != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Debug.Log("error in AddBlock, targetBlockDictionary.TryGetValue(new KeyValuePair<int, int>(x, z), out yBlockClassesWithXZ)");
            }
        }
        else
        {
            //no in prefab list.
            Debug.Log("not in prefab list. IsBlockObjectExists");
        }
        bpcRef = null;
        return false;
    }

    private void InstantizeBlock(int x, int y, int z)
    {
        BlockPrefabClass bpc;
        if (!IsBlockObjectExists(x , y, z, out bpc))
        {
            if(bpc.blockType != 0)
            {
                bpc.gameObject = Instantiate(geoPrefab[bpc.blockType], new Vector3(x, y, z), new Quaternion());
                MeshRenderer meshRenderer = bpc.gameObject.GetComponent<MeshRenderer>();
                MeshCollider meshCollider = bpc.gameObject.GetComponent<MeshCollider>();

                meshRenderer.enabled = true;
                meshCollider.enabled = true;
                bpc.isInstantize = true;
                bpc.isRender = true;
            }
        }
    }

    private void UnInstantizeCannotSeeBlock(int x, int y, int z)
    {
        BlockPrefabClass bpc;
        if(IsBlockObjectExists(x - 1, y, z, out bpc))
        {
            /*
            if(bpc.blockType == cannot see){
                //TODO : block is FULL block check
                //do nothing
            }
            else{
                
                return;
            }
            */

        }
        else
        {
            return;
        }
        if (IsBlockObjectExists(x + 1, y, z, out bpc))
        {
            /*
            if(bpc.blockType == cannot see){
                //TODO : block is FULL block check
                //do nothing
            }
            else{
                
                return;
            }
            */

        }
        else
        {
            return;
        }
        if (IsBlockObjectExists(x, y - 1, z, out bpc))
        {
            /*
            if(bpc.blockType == cannot see){
                //TODO : block is FULL block check
                //do nothing
            }
            else{
                
                return;
            }
            */

        }
        else
        {
            return;
        }
        if (IsBlockObjectExists(x, y + 1, z, out bpc))
        {
            /*
            if(bpc.blockType == cannot see){
                //TODO : block is FULL block check
                //do nothing
            }
            else{
                
                return;
            }
            */

        }
        else
        {
            return;
        }
        if (IsBlockObjectExists(x, y, z - 1, out bpc))
        {
            /*
            if(bpc.blockType == cannot see){
                //TODO : block is FULL block check
                //do nothing
            }
            else{
                
                return;
            }
            */

        }
        else
        {
            return;
        }
        if (IsBlockObjectExists(x, y, z + 1, out bpc))
        {
            /*
            if(bpc.blockType == cannot see){
                //TODO : block is FULL block check
                //do nothing
            }
            else{
                
                return;
            }
            */

        }
        else
        {
            return;
        }
        //all block is full block. 
        if(IsBlockObjectExists(x - 1, y, z, out bpc))
        {
            if(bpc.gameObject != null)
            {
                Destroy(bpc.gameObject);
                bpc.isInstantize = false;
                bpc.gameObject = null;
            }
        }
        return;
    }

    public void AddBlock(int x, int y, int z, int blockGeoType)
    {
        
        BlockPrefabClass bpc;
        if (IsBlockObjectExists(x, y, z, out bpc))
        {
            if (bpc.gameObject == null)
            {
                //add block

                bpc.isAir = false;
                bpc.blockType = blockGeoType;

                bpc.gameObject = Instantiate(geoPrefab[blockGeoType], new Vector3(x, y, z), new Quaternion());
                MeshRenderer meshRenderer = bpc.gameObject.GetComponent<MeshRenderer>();
                MeshCollider meshCollider = bpc.gameObject.GetComponent<MeshCollider>();

                meshRenderer.enabled = true;
                meshCollider.enabled = true;
                bpc.isInstantize = true;
                bpc.isRender = true;

                //check adj 6 block for destory prefab.

                //TODO : if blockGeoType not FULL Block... can see adj block. Do not call UnInstantizeCannotSeeBlock.
                /*
                if(blockGeoType == ...){
                    break;
                }
                 */
                UnInstantizeCannotSeeBlock(x - 1, y, z);
                UnInstantizeCannotSeeBlock(x + 1, y, z);
                UnInstantizeCannotSeeBlock(x, y + 1, z);
                UnInstantizeCannotSeeBlock(x, y - 1, z);
                UnInstantizeCannotSeeBlock(x, y, z - 1);
                UnInstantizeCannotSeeBlock(x, y, z + 1);

            }
            else
            {
                //no in prefab list.
                Debug.Log("tried add block but already another block exists. AddBlock");
            }
        }
        else
        {
            Debug.Log("IsBlockObjectExists return false. AddBlock");
        }
    }

    public void DeleteBlock(int x, int y, int z)
    {
        BlockPrefabClass bpc;
        if (IsBlockObjectExists(x, y, z, out bpc))
        {
            if (bpc.gameObject != null)
            {
                //instatize adj 6 blocks
                //TODO : if deleted block is not FULL block... do not below call 6 func
                InstantizeBlock(x - 1, y, z);
                InstantizeBlock(x + 1, y, z);
                InstantizeBlock(x, y + 1, z);
                InstantizeBlock(x, y - 1, z);
                InstantizeBlock(x, y, z - 1);
                InstantizeBlock(x, y, z + 1);
                //delete block

                bpc.isAir = true;
                bpc.blockType = 0;

                Destroy(bpc.gameObject);
                bpc.gameObject = null;
                
                bpc.isInstantize = false;
                bpc.isRender = false;
                
            }
            else
            {
                //no in prefab list.
                Debug.Log("tried add block but already another block exists. DeleteBlock");
            }
        }
        else
        {
            Debug.Log("IsBlockObjectExists return false. DeleteBlock");
        }
    }
}

public class BlockPrefabClass
{
    public bool isAir;      //remove
    public bool isInstantize;   //remove
    public bool isRender;
    public int blockType;
    public GameObject gameObject;
}


[Serializable]
public class PlayerData
{
    public float posX, posY, posZ;
    //public Vector3 seraillizePos;
    public float quatX, quatY, quatZ, quatW;
    //public Quaternion seraillzeQuat = new Quaternion();
    
}

/*  self reference
  while (isMemGeoLoop)
            {
                //check object will be loaded to mem is in temp
                if (!curLoadedMemPairList.Contains(new KeyValuePair<int, int>(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value)))
                {
                    if (curLoadedTempFilePairList.Contains(new KeyValuePair<int, int>(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value)))
                    {
                        //load mem
                        LoadTempGeometry(memGeoEnumerator.Current.Key, memGeoEnumerator.Current.Value);
                    }
                    else
                    {
                        //not in temp. Need to wait temp load.
                        break;
                    }
                }
                else
                {
                    //already has mem
                }
                isMemGeoLoop = memGeoEnumerator.MoveNext();
                yield return new WaitForSeconds(0.2f);
            }
 */
