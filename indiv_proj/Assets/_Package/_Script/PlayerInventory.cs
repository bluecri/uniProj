using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour {
    public static PlayerInventory instance;
    
    [SerializeField]
    public List<GameObject> mainInventorySeriallized;
    [SerializeField]
    public List<GameObject> fastInventorySeriallized;

    public Dictionary<int, InventoryEntry> mainInventory;
    public Dictionary<int, GameObject> mainInventoryGameObject;
    public Dictionary<int, InventoryEntry> fastInventory;
    public Dictionary<int, GameObject> fastInventoryGameObject;


    public enum ENUM_INVEN_TYPE
    {
        FAST, MAIN, CHEST, TOOLBOX, MAKEDONEBOX
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
        mainInventoryGameObject = new Dictionary<int, GameObject>();
        fastInventoryGameObject = new Dictionary<int, GameObject>();

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
    }

    public void RightClick_Hand_FastInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.SwapInventoryEntry(UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot, fastInventory[index], fastInventoryGameObject[index]);
    }

    public void LeftClick_Hand_MainInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.SwapInventoryEntry(mainInventory[index], mainInventoryGameObject[index], UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot);
    }

    public void RightClick_Hand_MainInventory(int index, InventoryEntry handEntry)
    {
        InventoryEntry.SwapInventoryEntry(UIManager.instance.cursorSlot.GetComponent<InventoryEntry>(), UIManager.instance.cursorSlot, mainInventory[index], mainInventoryGameObject[index]);
    }

    /*
     * Minus inventory with index, item num. Use item function.
    */
    public bool UseItemInFastInventory(int index, int itemMinusNum)
    {
        if (fastInventory[index].m_itemNum >= itemMinusNum)
        {
            fastInventory[index].m_itemNum -= itemMinusNum;
            if (fastInventory[index].m_itemNum == 0)
            {
                fastInventory[index].m_geoCode = 0; //make empty entry
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    
}
