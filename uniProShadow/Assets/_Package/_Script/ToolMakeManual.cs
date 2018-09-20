using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolMakeManual : MonoBehaviour {
    public static ToolMakeManual instance = null;


    //List<ToolMakeEntry[]> toolMakeEntryArrayList;
    public Dictionary<int, ToolMakeEntryListWithItemCode> toolMakeEntryListWithCodeDictionary;

    ToolMakeEntry[] swordManualEntryArr;
    ToolMakeEntry[] axeManualEntryArr;
    ToolMakeEntry[] potionManualEntryArr;
    ToolMakeEntry[] sobManualEntryArr;


    private void Awake()
    {
        //singleton

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        // init Manual
        toolMakeEntryListWithCodeDictionary = new Dictionary<int, ToolMakeEntryListWithItemCode>();

        //sword
        ToolMakeEntryListWithItemCode toolMakeEntryListWithItemCode = new ToolMakeEntryListWithItemCode(101);
        toolMakeEntryListWithItemCode.SetEntryElem(1, 1, 1);
        toolMakeEntryListWithItemCode.SetEntryElem(4, 7, 1);
        toolMakeEntryListWithItemCode.SetEntryElem(7, 7, 1);    //dirt-wood-wood
        toolMakeEntryListWithCodeDictionary.Add(toolMakeEntryListWithItemCode.m_itemCode, toolMakeEntryListWithItemCode);

        toolMakeEntryListWithItemCode = new ToolMakeEntryListWithItemCode(102);
        toolMakeEntryListWithItemCode.SetEntryElem(0, 7, 1);
        toolMakeEntryListWithItemCode.SetEntryElem(1, 7, 1);
        toolMakeEntryListWithItemCode.SetEntryElem(4, 7, 1);
        toolMakeEntryListWithItemCode.SetEntryElem(7, 7, 1);    //wood wood wood wood
        toolMakeEntryListWithCodeDictionary.Add(toolMakeEntryListWithItemCode.m_itemCode, toolMakeEntryListWithItemCode);

        toolMakeEntryListWithItemCode = new ToolMakeEntryListWithItemCode(103);
        toolMakeEntryListWithItemCode.SetEntryElem(1, 3, 1);
        toolMakeEntryListWithItemCode.SetEntryElem(4, 7, 1);
        toolMakeEntryListWithItemCode.SetEntryElem(7, 7, 1);    //stone wood wood
        toolMakeEntryListWithCodeDictionary.Add(toolMakeEntryListWithItemCode.m_itemCode, toolMakeEntryListWithItemCode);

        toolMakeEntryListWithItemCode = new ToolMakeEntryListWithItemCode(110);
        toolMakeEntryListWithItemCode.SetEntryElem(4, 5, 1);
        toolMakeEntryListWithItemCode.SetEntryElem(7, 5, 1);    // grass grass
        toolMakeEntryListWithCodeDictionary.Add(toolMakeEntryListWithItemCode.m_itemCode, toolMakeEntryListWithItemCode);
        
    }

    public bool GetCanMakeItemFromToolBox(Dictionary<int, InventoryEntry> dictionary, out int geoCode, out int itemNum)
    {
        geoCode = 0;
        itemNum = 0;
        
        foreach(KeyValuePair<int, ToolMakeEntryListWithItemCode> elem in toolMakeEntryListWithCodeDictionary)
        {
            int retCount = 999; //will be calculated by min
            int indexOfList = 0;
            int retGeoCode = elem.Key;  //item Geocode
            bool bItemCodeInconsist = false;

            foreach (ToolMakeEntry entry in elem.Value.toolMakeEntryList)
            {
                if(entry.m_geoCode == dictionary[indexOfList].m_geoCode)
                {
                    if(entry.m_geoCode == 0)
                    {
                        //do nothing. Air block.
                    }
                    else
                    {
                        int canMakeItemN = dictionary[indexOfList].m_itemNum / entry.m_itemN;
                        if(canMakeItemN == 0)
                        {
                            break;
                        }
                        retCount = Mathf.Min(retCount, canMakeItemN);
                    }
                }
                else
                {
                    bItemCodeInconsist = true;   //not same geocode
                    break;
                }

                //loop increment
                indexOfList++;
            }

            if(retCount > 0 && !bItemCodeInconsist)
            {
                itemNum = retCount;
                geoCode = retGeoCode;
                return true;
            }
        }
        return false;
    }
}

public class ToolMakeEntry
{
    public int m_geoCode = 0;
    public int m_itemN = 0;

    public ToolMakeEntry(int geoCode, int itemN)
    {
        m_geoCode = geoCode;
        m_itemN = itemN;
    }
}

public class ToolMakeEntryListWithItemCode
{
    public ToolMakeEntry[] toolMakeEntryList;
    public int m_itemCode = 0;

    public ToolMakeEntryListWithItemCode(int itemCode)
    {
        toolMakeEntryList = new ToolMakeEntry[9];
        m_itemCode = itemCode;

        for(int i=0; i<toolMakeEntryList.Length; i++)
        {
            toolMakeEntryList[i] = new ToolMakeEntry(0, 0);
        }
    }
    public void SetEntryElem(int index, int geoCode, int itemNum)
    {
        toolMakeEntryList[index].m_geoCode = geoCode;
        toolMakeEntryList[index].m_itemN = itemNum;
    }
}
