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

    
}
