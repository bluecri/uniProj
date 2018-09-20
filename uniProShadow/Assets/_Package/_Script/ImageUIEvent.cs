using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImageUIEvent : MonoBehaviour, IPointerClickHandler
{
    private InventoryEntry entry = null;

    public void Awake()
    {
        entry = GetComponent<InventoryEntry>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Left)
        {
            switch (entry.m_entryType)
            {
                case (int)PlayerInventory.ENUM_INVEN_TYPE.FAST:
                    PlayerInventory.instance.LeftClick_Hand_FastInventory(entry.m_index, InputManager.instance.cursorObject.GetComponent<InventoryEntry>());
                    break;
                case (int)PlayerInventory.ENUM_INVEN_TYPE.MAIN:
                    PlayerInventory.instance.LeftClick_Hand_MainInventory(entry.m_index, InputManager.instance.cursorObject.GetComponent<InventoryEntry>());
                    break;
                case (int)PlayerInventory.ENUM_INVEN_TYPE.TOOLBOX:
                    PlayerInventory.instance.LeftClick_Hand_ToolInventory(entry.m_index, InputManager.instance.cursorObject.GetComponent<InventoryEntry>());
                    break;
                case (int)PlayerInventory.ENUM_INVEN_TYPE.MADENBOX:
                    InventoryEntry cursorInventoryEntry = InputManager.instance.cursorObject.GetComponent<InventoryEntry>();
                    if (cursorInventoryEntry.IsEmpty())
                    {
                        PlayerInventory.instance.LeftClick_Hand_MadenInventory(entry.m_index, InputManager.instance.cursorObject.GetComponent<InventoryEntry>());
                    }
                    break;
            }
        }
        else if(eventData.button == PointerEventData.InputButton.Right)
        {
            switch (entry.m_entryType)
            {
                case (int)PlayerInventory.ENUM_INVEN_TYPE.FAST:
                    PlayerInventory.instance.RightClick_Hand_FastInventory(entry.m_index, InputManager.instance.cursorObject.GetComponent<InventoryEntry>());
                    break;
                case (int)PlayerInventory.ENUM_INVEN_TYPE.MAIN:
                    PlayerInventory.instance.RightClick_Hand_MainInventory(entry.m_index, InputManager.instance.cursorObject.GetComponent<InventoryEntry>());
                    break;
                case (int)PlayerInventory.ENUM_INVEN_TYPE.TOOLBOX:
                    PlayerInventory.instance.RightClick_Hand_ToolInventory(entry.m_index, InputManager.instance.cursorObject.GetComponent<InventoryEntry>());
                    break;
                case (int)PlayerInventory.ENUM_INVEN_TYPE.MADENBOX:
                    
                    break;
            }
        }
    }
}
