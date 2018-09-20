using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfRotateYAxis : MonoBehaviour {
    private Vector3 rotateVector = new Vector3(0.0f, 0.5f, 0.0f);
    
    // Update is called once per frame
    void Update () {
        transform.Rotate(rotateVector);
        
	}
}
