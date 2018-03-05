using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHash : MonoBehaviour {
    public static int SpeedID;
    public static int IdleLongStartID;
    public static int IdleLongEndID;
    public static int JumpStartID;
    public static int JumpAirID;
    public static int JumpEndID;
    public static int FindStartID;
    public static int FindEndID;
    public static int AttackID;
    public static int SlideID;
    public static int WinID;
    public static int RefreshID;
    public static int StunID;
    public static int KnockoutID;
    public static int LoseID;


    void Awake()
    {
        SpeedID = Animator.StringToHash("Speed");
        IdleLongStartID = Animator.StringToHash("IdleLongStart");
        IdleLongEndID = Animator.StringToHash("IdleLongEnd");
        JumpStartID = Animator.StringToHash("JumpStart");
        JumpAirID = Animator.StringToHash("JumpAir");
        JumpEndID = Animator.StringToHash("JumpEnd");
        FindStartID = Animator.StringToHash("FindStart");
        FindEndID = Animator.StringToHash("FindEnd");
        AttackID = Animator.StringToHash("Attack");
        SlideID = Animator.StringToHash("Slide");
        WinID = Animator.StringToHash("Win");
        RefreshID = Animator.StringToHash("Refresh");
        StunID = Animator.StringToHash("Stun");
        KnockoutID = Animator.StringToHash("Knockout");
        LoseID = Animator.StringToHash("Lose");
    }
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
