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
    

    public const int fileBlockSize = sizeof(int) + sizeof(bool);
    

    public int[,] preLoadOnPrefabGeometryIndex;    //3x3
    public int[,] preLoadOnMemGeometryIndex;    //3x3
    public int[,] preLoadOnTempGeometryIndex;   //5x5

    public const int prefabGeoIndexSize = 1;
    public const int memGeoIndexSize = 1;
    public const int tempGeoIndexSize = 3;

    public HashSet<KeyValuePair<int, int>> curLoadedTempFilePairList = new HashSet<KeyValuePair<int, int>>();

    public HashSet<KeyValuePair<int, int>> curLoadedMemPairList = new HashSet<KeyValuePair<int, int>>();
    public Dictionary<KeyValuePair<int, int>, byte[]> curLoadedMem = new Dictionary<KeyValuePair<int, int>, byte[]>();

    public HashSet<KeyValuePair<int, int>> curLoadedPrefabPairList = new HashSet<KeyValuePair<int, int>>();
    public Dictionary<KeyValuePair<int, int>, Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]>> curLoadedPrefab = new Dictionary<KeyValuePair<int, int>, Dictionary<KeyValuePair<int, int>, BlockPrefabClass[]>> ();

    public void Awake()
    {
        //singleton

        if (instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
            DontDestroyOnLoad(instance);
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

        LoadOriginGeometryWithUserPos(playerInstance.transform.position);
        LoadTempGeometryWithUserPos(playerInstance.transform.position);
        LoadPrefabWithUserPos(playerInstance.transform.position);

    }
    public void SavePlayerInfo()
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/playerInfo.dat", FileMode.Create);

            PlayerData playerData = new PlayerData();
            playerData.pos = playerInstance.transform.position;
            playerData.quat = playerInstance.transform.rotation;

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
                PlayerData playerData = (PlayerData)bf.Deserialize(file);
                file.Close();
                playerInstance = Instantiate(playerPrefab, playerData.pos, playerData.quat).GetComponent<Player>();

            }
            catch (IOException e)
            {
                Debug.Log(e.ToString() + "LoadPlayerInfo Load error");
                playerInstance = Instantiate(playerPrefab, new Vector3(0, 0, 0), new Quaternion()).GetComponent<Player>();
            }
        }
        else
        {
            playerInstance = Instantiate(playerPrefab, new Vector3(0, 0, 0), new Quaternion()).GetComponent<Player>();
        }
    }

    public void LoadOriginGeometryWithUserPos(Vector3 userPosition)
    {
        int fileX = ((int)userPosition.x);
        int fileZ = ((int)userPosition.z);
        if (fileX < 0)
        {
            fileX -= (fileXNum-1);
        }
        if (fileZ < 0)
        {
            fileZ -= (fileZNum - 1);
        }
        fileX /= fileXNum;
        fileZ /= fileZNum;

        for (int i = 0; i < preLoadOnTempGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnTempGeometryIndex[i, 0];
            int accZ = preLoadOnTempGeometryIndex[i, 1];
            int targetX = accX + fileX;
            int targetZ = accZ + fileZ;
            if (!curLoadedTempFilePairList.Contains(new KeyValuePair<int, int>(targetX, targetZ)))    // has not temp file
            { 
                LoadOriginGeometry(targetX, targetZ);
            }
        }
        
    }

    public void LoadTempGeometryWithUserPos(Vector3 userPosition)
    {
        int fileX = ((int)userPosition.x);
        int fileZ = ((int)userPosition.z);
        if (fileX < 0)
        {
            fileX -= (fileXNum - 1);
        }
        if (fileZ < 0)
        {
            fileZ -= (fileZNum - 1);
        }
        fileX /= fileXNum;
        fileZ /= fileZNum;

        for (int i = 0; i < preLoadOnMemGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnMemGeometryIndex[i, 0];
            int accZ = preLoadOnMemGeometryIndex[i, 1];
            int targetX = accX + fileX;
            int targetZ = accZ + fileZ;
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

    public void LoadPrefabWithUserPos(Vector3 userPosition)
    {
        int fileX = ((int)userPosition.x);
        int fileZ = ((int)userPosition.z);
        if (fileX < 0)
        {
            fileX -= (fileXNum - 1);
        }
        if (fileZ < 0)
        {
            fileZ -= (fileZNum - 1);
        }
        fileX /= fileXNum;
        fileZ /= fileZNum;

        for (int i = 0; i < preLoadOnPrefabGeometryIndex.Length / 2; i++)
        {
            int accX = preLoadOnPrefabGeometryIndex[i, 0];
            int accZ = preLoadOnPrefabGeometryIndex[i, 1];
            int targetX = accX + fileX;
            int targetZ = accZ + fileZ;
            if (curLoadedMemPairList.Contains(new KeyValuePair<int, int>(targetX, targetZ)))    // has temp file
            {
                if (!curLoadedPrefabPairList.Contains(new KeyValuePair<int, int>(targetX, targetZ))) //not has prefab
                {
                    LoadPrefabGeometry(targetX, targetZ);
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
    public void LoadPrefabGeometry(int targetX, int targetZ)
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
                for (int y=0; y<fileYNum; y++)
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


    //load origin geometry- > temp geometry
    //register to map.
    public void LoadOriginGeometry(int targetX, int targetZ)
    {
        byte[] tempGeometry = null;
        tempGeometry = new byte[fileBlockSize * fileXNum * fileZNum * fileYNum];

        BinaryReader br;
        try
        {
            //read origin file
            br = new BinaryReader(new FileStream(Application.persistentDataPath + "/map_" + targetX + "_" + targetZ + ".dat", FileMode.Open));
            br.Read(tempGeometry, 0, fileBlockSize * fileXNum * fileZNum * fileYNum);

            //write to temp file
            BinaryWriter bw;
            try
            {
                bw = new BinaryWriter(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.CreateNew));
                bw.Write(tempGeometry, 0, fileBlockSize * fileXNum * fileZNum * fileYNum);
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

    public void SaveOriginGeometry(int targetX, int targetZ)
    {
        byte[] bytes = new byte[fileBlockSize * fileXNum * fileZNum * fileYNum];

        BinaryReader br;
        BinaryWriter bw;
        try
        {
            br = new BinaryReader(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.Open));
            br.Read(bytes, 0, fileBlockSize * fileXNum * fileZNum * fileYNum);
            br.Close();
        }
        catch (IOException e)
        {
            Debug.Log(e.Message + "\n Cannot open temp file. saveOriginGeometry");
        }

        try
        {
            bw = new BinaryWriter(new FileStream(Application.persistentDataPath + "/map_" + targetX + "_" + targetZ + ".dat", FileMode.Create));
            bw.Write(bytes, 0, fileBlockSize * fileXNum * fileZNum * fileYNum);
            bw.Close();
        }
        catch (IOException e)
        {
            Debug.Log(e.Message + "\n Cannot write origin file. saveOriginGeometry");
        }
    }

    public void SaveTempGeometry(int targetX, int targetZ)
    {
        byte[] saveByte = null;
        KeyValuePair<int, int> mapKey = new KeyValuePair<int, int>(targetX, targetZ);

        saveByte = curLoadedMem[mapKey];

        if (saveByte != null)
        {
            BinaryWriter bw;
            try
            {
                bw = new BinaryWriter(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.Create));
                bw.Write(saveByte, 0, fileBlockSize * fileXNum * fileZNum * fileYNum);
                bw.Close();
            }
            catch (IOException e)
            {
                Debug.Log(e.Message + "\n Cannot create temp file. saveTempGeometry");
            }
        }
        else
        {
            Debug.Log("SaveByte is null. saveTempGeometry");
        }

    }

    public void LoadTempGeometry(int targetX, int targetZ)
    {
        byte[] tempGeometry = null;
        tempGeometry = new byte[fileBlockSize * fileXNum * fileZNum * fileYNum];
        BinaryReader br;
        try
        {
            br = new BinaryReader(new FileStream(Application.persistentDataPath + "/temp_map_" + targetX + "_" + targetZ + ".dat", FileMode.Open));
            br.Read(tempGeometry, 0, fileBlockSize * fileXNum * fileZNum * fileYNum);
            curLoadedMem.Add(new KeyValuePair<int, int>(targetX, targetZ), tempGeometry);
            br.Close();
            curLoadedMemPairList.Add(new KeyValuePair<int, int>(targetX, targetZ));
        }
        catch(IOException e)
            {
            Debug.Log(e.Message + "\n Cannot read temp file. LoadTempGeometry");
        }
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
            bw.Write(outByte, 0, fileBlockSize * fileXNum * fileZNum * fileYNum);
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

}

public class BlockPrefabClass
{
    public bool isAir;
    public bool isInstantize;
    public bool isRender;
    public int blockType;
    public GameObject gameObject;
}


[Serializable]
public class PlayerData
{
    public Vector3 pos;
    public Quaternion quat;
}