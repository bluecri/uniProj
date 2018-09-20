using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
    public static PlayerInventory instance;
    
    [SerializeField]
    public List<GameObject> mainInventorySeriallized;
    [SerializeField]
    public List<GameObject> fastInventorySeriallized;
    [SerializeField]
    public List<GameObject> toolInventorySeriallized;
    [SerializeField]
    public List<GameObject> madenInventorySeriallized;

    public Dictionary<int, InventoryEntry> mainInventory;
    public Dictionary<int, GameObject> mainInventoryGameObject;
    public Dictionary<int, InventoryEntry> fastInventory;
    public Dictionary<int, GameObject> fastInventoryGameObject;
    public Dictionary<int, InventoryEntry> toolInventory;
    public Dictionary<int, GameObject> toolInventoryGameObject;
    public Dictionary<int, InventoryEntry> madenInventory;
    public Dictionary<int, GameObject> madenInventoryGameObject;

    public enum ENUM_INVEN_TYPE
    {
        FAST, MAIN, CHEST, TOOLBOX, MADENBOX
    }

    //key input
    public bool leftKeyDown = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else
        {
            Destroy(this.gameObject);
        }

        mainInventory = new Dictionary<int, InventoryEntry>();
        fastInventory = new Dictionary<int, InventoryEntry>();
        toolInventory = new Dictionary<int, InventoryEntry>();
        madenInventory = new Dictionary<int, InventoryEntry>();
        mainInventoryGameObject = new Dictionary<int, GameObject>();
        fastInventoryGameObject = new Dictionary<int, GameObject>();
        toolInventoryGameObject = new Dictionary<int, GameObject>();
        madenInventoryGameObject = new Dictionary<int, GameObject>();

        int index = 0;
        foreach(GameObject entry in mainInventorySeriallized)
        {
            InventoryEntry invenEntry = entry.GetComponent<InventoryEntry>();
            invenEntry.m_index = index;
            invenEntry.m_entryType = (int)ENUM_INVEN_TYPE.MAIN;

            mainInventory.Add(index, invenEntry);
            mainInventoryGameObject.Add(index, entry);
            index++;
        }

        index = 0;
        foreach (GameObject entry in fastInventorySeriallized)
        {
            InventoryEntry invenEntry = entry.GetComponent<InventoryEntry>();
            invenEntry.m_index = index;
            invenEntry.m_entryType = (int)ENUM_INVEN_TYPE.FAST;

            fastInventory.Add(index, invenEntry);
            fastInventoryGameObject.Add(index, entry);
            index++;
        }

        index = 0;
        foreach (GameObject entry in toolInventorySeriallized)
        {
            InventoryEntry invenEntry = entry.GetComponent<InventoryEntry>();
            invenEntry.m_index = index;
            invenEntry.m_entryType = (int)ENUM_INVEN_TYPE.TOOLBOX;

            toolInventory.Add(index, invenEntry);
            toolInventoryGameObject.Add(index, entry);
            index++;
        }

        index = 0;
        foreach (GameObject entry in madenInventorySeriallized)
        {
            InventoryEntry invenEntry = entry.GetComponent<InventoryEntry>();
            invenEntry.m_index = index;
            invenEntry.m_entryType = (int)ENUM_INVEN_TYPE.MADENBOX;

            madenInventory.Add(index, invenEntry);
            madenInventoryGameObject.Add(index, entry);
            index++;
        }
    }

    /*
     * 1. push block to fastInventory with same geoCode
     * 2. push block to fastInventory empty entry 
     * 3. push block to mainInvenotry with same geoCode
     * 4. pubh block to mainInventory with empty entry
     * return remain block num
    */
    public int AddBlockWithPickUp(int geoCode, int addNum)
    {
        // 1.Process
        foreach(KeyValuePair<int, InventoryEntry> elem in fastInventory)
        {
            InventoryEntry entry = elem.Value;
            if (addNum > 0 && entry.IsGeocode(geoCode) && !entry.IsFull())
            {
                int pushBlockCount = Mathf.Min(addNum, entry.GetCanPushItemNum());  //push count block
                
                entry.ItemAddNum(pushBlockCount);
                addNum -= pushBlockCount;   //want to add block num - push block num
            }

            if (addNum == 0) { break; }
        }
        // 2. Process
        foreach (KeyValuePair<int, InventoryEntry> elem in fastInventory)
        {
            InventoryEntry entry = elem.Value;
            if (addNum > 0 && entry.IsGeocode(0) && !entry.IsFull())
            {
                int pushBlockCount = Mathf.Min(addNum, entry.GetCanPushItemNum());  //push count block

                addNum -= pushBlockCount;   //want to add block num - push block num

                if (entry.IsEmpty())
                {
                    GameObject imageObj = ItemPool.instance.GetItemBlockGameObjectFromPool(geoCode);
                    imageObj.transform.SetParent(fastInventoryGameObject[elem.Key].transform, false);
                    entry.ItemAddNum(pushBlockCount, imageObj);
                }
                else
                {
                    entry.ItemAddNum(pushBlockCount);
                }
                
                entry.m_geoCode = geoCode;
            }

            if (addNum == 0) { break; }
        }

        foreach (KeyValuePair<int, InventoryEntry> elem in mainInventory)
        {
            InventoryEntry entry = elem.Value;
            if (addNum > 0 && entry.IsGeocode(geoCode) && !entry.IsFull())
            {
                int pushBlockCount = Mathf.Min(addNum, entry.GetCanPushItemNum());  //push count block

                addNum -= pushBlockCount;   //want to add block num - push block num
                entry.ItemAddNum(pushBlockCount);
            }

            if (addNum == 0) { break; }
        }

        foreach (KeyValuePair<int, InventoryEntry> elem in mainInventory)
        {
            InventoryEntry entry = elem.Value;
            if (addNum > 0 && entry.IsGeocode(0) && !entry.IsFull())
            {
                int pushBlockCount = Mathf.Min(addNum, entry.GetCanPushItemNum());  //push count block

                addNum -= pushBlockCount;   //want to add block num - push block num

                if (entry.IsEmpty())
                {
                    GameObject imageObj = ItemPool.instance.GetItemBlockGameObjectFromPool(geoCode);
                    imageObj.transform.SetParent(mainInventoryGameObject[elem.Key].transform, false);
                    entry.ItemAddNum(pushBlockCount, imageObj);
                }
                else
                {
                    entry.ItemAddNum(pushBlockCount);
                }
                entry.m_geoCode = geoCode;
            }

            if (addNum == 0) { break; }
        }
        return addNum;
    }

    public void LeftClick_Hand_FastInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.SwapInventoryEntry(fastInventory[index], fastInventoryGameObject[index], UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot);
        SceneManager.playerInstance.PlayerHandItemChange(SceneManager.playerInstance.curSelectedFastInventoryIndex);
    }

    public void RightClick_Hand_FastInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.RightClickEntry(fastInventory[index], fastInventoryGameObject[index], UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot);
        SceneManager.playerInstance.PlayerHandItemChange(SceneManager.playerInstance.curSelectedFastInventoryIndex);
    }

    public void LeftClick_Hand_MainInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.SwapInventoryEntry(mainInventory[index], mainInventoryGameObject[index], UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot);
    }

    public void RightClick_Hand_MainInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.RightClickEntry(mainInventory[index], mainInventoryGameObject[index], UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot);
    }

    public void LeftClick_Hand_ToolInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.SwapInventoryEntry(toolInventory[index], toolInventoryGameObject[index], UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot);
        // check is can make item
        int canMakeGeocode = 0, canMakeItemNum = 0;
        if(true == ToolMakeManual.instance.GetCanMakeItemFromToolBox(toolInventory, out canMakeGeocode, out canMakeItemNum))
        {
            //make item in madenInventory
            InventoryEntry madenEntry = madenInventory[0];
            madenEntry.ItemMinusNum(madenEntry.m_itemNum);  //clear
            madenEntry.ItemAddNum(canMakeItemNum, canMakeGeocode, madenInventoryGameObject[0]); //add
        }
        else
        {
            InventoryEntry madenEntry = madenInventory[0];
            madenEntry.ItemMinusNum(madenEntry.m_itemNum);  //clear
        }

    }

    public void RightClick_Hand_ToolInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.RightClickEntry(toolInventory[index], toolInventoryGameObject[index], UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot);
        // check is can make item
        int canMakeGeocode, canMakeItemNum;
        if (true == ToolMakeManual.instance.GetCanMakeItemFromToolBox(toolInventory, out canMakeGeocode, out canMakeItemNum))
        {
            //make item in madenInventory
            InventoryEntry madenEntry = madenInventory[0];
            madenEntry.ItemMinusNum(madenEntry.m_itemNum);  //clear
            madenEntry.ItemAddNum(canMakeItemNum, canMakeGeocode, madenInventoryGameObject[0]); //add

        }
        else
        {
            InventoryEntry madenEntry = madenInventory[0];
            madenEntry.ItemMinusNum(madenEntry.m_itemNum);  //clear
        }
    }

    public void LeftClick_Hand_MadenInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry cursorEntry = UIManager.instance.cursorSlot.GetComponent<InventoryEntry>();

        if (cursorEntry.IsEmpty())
        {
            InventoryEntry.SwapInventoryEntry(madenInventory[index], madenInventoryGameObject[index], cursorEntry, UIManager.instance.cursorSlot);
            // Minus in tool item..
            if(cursorEntry.m_geoCode != 0)
            {
                MinusToolInvenWithTargetItemGeoCode(cursorEntry.m_geoCode);
            }
        }
        else
        {
            InventoryEntry madenEntry = madenInventory[0];
            madenEntry.ItemMinusNum(madenEntry.m_itemNum);  //clear
        }
    }

    public void RightClick_Hand_MadenInventory(int index, InventoryEntry handEntry)
    {
        //InventoryEntry.SwapInventoryEntry(UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot, madenInventory[index], madenInventoryGameObject[index]);
    }


    /*
     * Load Player Inventory
     */
    public void MainInventoryLoad(List<FileInventoryEntry> fileInventoryEntryLIst)
    {
        foreach(FileInventoryEntry elem in fileInventoryEntryLIst)
        {
            mainInventory[elem.index].ItemAddNum(elem.itemNum, elem.geoCode, mainInventoryGameObject[elem.index]);
        }
    }

    public void FastInventoryLoad(List<FileInventoryEntry> fileInventoryEntryLIst)
    {
        foreach (FileInventoryEntry elem in fileInventoryEntryLIst)
        {
            fastInventory[elem.index].ItemAddNum(elem.itemNum, elem.geoCode, fastInventoryGameObject[elem.index]);
        }
    }

    /*
     * Minus inventory with index, item num. Use item function.
    */
    public bool UseItemInFastInventory(int index, int itemMinusNum)
    {
        bool ret = fastInventory[index].ItemMinusNum(itemMinusNum);
        if (ret)
        {
            SceneManager.playerInstance.PlayerHandItemChange(SceneManager.playerInstance.curSelectedFastInventoryIndex);
        }
        return ret;
    }

    /*
     * Minus Tool inventory Item with geoCode & num
    */
    public void MinusToolInvenWithTargetItemGeoCode(int geoCode)
    {
        ToolMakeEntry[] targetItemNeedEntryArray = ToolMakeManual.instance.toolMakeEntryListWithCodeDictionary[geoCode].toolMakeEntryList;
        for(int row = 0; row < targetItemNeedEntryArray.Length; row++)
        {
            toolInventory[row].ItemMinusNum(targetItemNeedEntryArray[row].m_itemN);
        }
    }
}
