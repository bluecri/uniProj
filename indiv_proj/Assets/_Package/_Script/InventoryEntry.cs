using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryEntry : MonoBehaviour {
    public int m_geoCode;
    public int m_itemNum;
    public int m_maxItemNum = 4;
    public GameObject textObject;

    [System.NonSerialized]
    public int m_index;
    [System.NonSerialized]
    public int m_entryType;
    [System.NonSerialized]
    public GameObject m_instantedUIImage = null;    //save image gameObject to this.
    [System.NonSerialized]
    public Text textObjectText;

    private void Awake()
    {
        textObjectText = textObject.GetComponent<Text>();
    }

    //Add item
    public bool ItemAddNum(int itemNum)
    {
        if (m_geoCode == 0 && m_itemNum == 0)
        {
            Debug.Log("call ItemAddNum(int itemNum, GameObject gob) instead ItemAddNum(int itemNum)");
            return false;
        }
        if (m_itemNum + itemNum <= m_maxItemNum)
        {
            m_itemNum += itemNum;
        }
        else
        {
            Debug.Log("Add error ItemAddNum(int itemNum)");
            return false;
        }

        SyncTextWithItemNum();
        return true;
    }

    //Shoud get parent set complete image gameObject from PlayerInventory
    public bool ItemAddNum(int itemNum, GameObject gob)
    {
        if(m_geoCode != 0 || m_itemNum != 0)
        {
            Debug.Log("call ItemAddNum(int itemNum) instead ItemAddNum(int itemNum, GameObject gob)");
            return false;
        }
        if (m_itemNum + itemNum <= m_maxItemNum)
        {
            m_itemNum += itemNum;
            m_geoCode = gob.GetComponent<BlockPrefab>().m_geoCode;
        }
        else
        {
            Debug.Log("Add error ItemAddNum(int itemNum, GameObject gob)");
        }
        m_instantedUIImage = gob;
        //m_instantedUIImage.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        SyncTextWithItemNum();

        return true;
    }

    //minus item
    public bool ItemMinusNum(int itemNum)
    {
        if(m_itemNum >= itemNum)
        {
            m_itemNum -= itemNum;
        }
        else
        {
            Debug.Log("Add error ItemMinusNum(int itemNum)");
            return false;
        }

        if(m_itemNum == 0)
        {
            m_geoCode = 0;
            ItemPool.instance.ReturnGameObjectToPool(m_instantedUIImage);
            m_instantedUIImage = null;
        }
        SyncTextWithItemNum();

        return true;
    }

    public static void SwapInventoryEntry(InventoryEntry entry, GameObject parent, InventoryEntry otherEntry, GameObject otherEntryParent)
    {
        if (entry.IsEmpty() && otherEntry.IsEmpty())
        {
            //do nothing
        }
        //empty <= not empty
        else if (entry.IsEmpty() && !otherEntry.IsEmpty())
        {
            entry.ItemAddNum(otherEntry.m_itemNum, otherEntry.m_instantedUIImage);
            otherEntry.m_instantedUIImage.transform.SetParent(parent.transform, false);
            //otherEntry.m_instantedUIImage.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            otherEntry.m_instantedUIImage = null;
            otherEntry.m_itemNum = 0;
            otherEntry.m_geoCode = 0;
        }
        //not empty => empty
        else if (!entry.IsEmpty() && otherEntry.IsEmpty())
        {
            otherEntry.ItemAddNum(entry.m_itemNum, entry.m_instantedUIImage);
            entry.m_instantedUIImage.transform.SetParent(otherEntryParent.transform, false);
            //entry.m_instantedUIImage.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            entry.m_instantedUIImage = null;
            entry.m_itemNum = 0;
            entry.m_geoCode = 0;
        }
        else if (entry.IsGeocode(otherEntry.m_geoCode))     //same geocode. first to second
        {
            int canPushNum = otherEntry.GetCanPushItemNum();
            otherEntry.ItemAddNum(canPushNum);
            entry.ItemMinusNum(canPushNum);
        }
        else
        {
            //different geocode. Swap block
            int tempGeoCode = entry.m_geoCode;
            int tempNum = entry.m_itemNum;
            GameObject tempGameObject = entry.m_instantedUIImage;

            entry.m_geoCode = otherEntry.m_geoCode;
            entry.m_itemNum = otherEntry.m_itemNum;
            entry.m_instantedUIImage = otherEntry.m_instantedUIImage;
            otherEntry.m_instantedUIImage.transform.SetParent(parent.transform, false);

            otherEntry.m_geoCode = tempGeoCode;
            otherEntry.m_itemNum = tempNum;
            otherEntry.m_instantedUIImage = tempGameObject;
            tempGameObject.transform.SetParent(otherEntryParent.transform, false);

            //entry.m_instantedUIImage.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);
            //otherEntry.m_instantedUIImage.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, 0.0f, 0.0f);

        }

        entry.SyncTextWithItemNum();
        otherEntry.SyncTextWithItemNum();
    }

    public void SyncTextWithItemNum()
    {
        textObjectText.text = m_itemNum.ToString();
    }

    //how many block can push to inventory entry
    public int GetCanPushItemNum()
    {
        return m_maxItemNum - m_itemNum;
    }

    public bool IsGeocode(int geoCode)
    {
        if(m_geoCode == geoCode)
        {
            return true;
        }
        return false;
    }

    public bool IsFull()
    {
        if(m_itemNum == m_maxItemNum)
        {
            return true;
        }
        return false;
    }
    public bool IsEmpty()
    {
        if (m_itemNum == 0)
        {
            return true;
        }
        return false;
    }
}
