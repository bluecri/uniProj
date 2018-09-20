using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * BlockPrefab is in BlockInfo List & BlockPrefab.
 * Will be component of Blocks.
*/

[System.Serializable]
public class BlockPrefab : MonoBehaviour {
    [SerializeField]
    public GameObject m_geoPrefab;
    [SerializeField]
    public int m_geoCode = 0;
    [SerializeField]
    public int m_hp = 0;
    [SerializeField]
    public bool m_hpReset = true;
    public AudioClip hitSoundClip;

    [SerializeField]
    public bool m_canSetupBlock = true; //is block can setup
    [SerializeField]
    public bool m_canDig = true;    //can dig
    [SerializeField]
    public int m_blockDamage = 1;   //dmg to block
    [SerializeField]
    public float m_blockDigDistance = 1.5f;   //dmg to block
    [SerializeField]
    public bool m_canAttack = true; //can dmg to unit
    
    [SerializeField]
    public int m_unitDamage = 1;    //dmg to unit
    [SerializeField]
    public bool m_isChest = false; //can dmg to unit

    
}
