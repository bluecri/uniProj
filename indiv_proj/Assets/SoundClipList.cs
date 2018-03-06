using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundClipList : MonoBehaviour {
    public static SoundClipList instance;

    public AudioClip blockHitSound;
    public AudioClip blockSetupSound;
    public AudioClip blockDestroySound;
    public AudioClip jumpSound;
    public AudioClip attackSound;
    public AudioClip hitSound;

    public void Awake()
    {
        //singleton

        if (instance == null)
        {
            instance = this;

        }
        else if (instance != this)
        {
            Destroy(gameObject);
            DontDestroyOnLoad(instance);
        }
    }
    
}
