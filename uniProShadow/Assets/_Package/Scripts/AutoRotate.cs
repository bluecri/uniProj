using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour {

	public Vector3 speed;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		gameObject.GetComponent<Transform>().Rotate( speed*Time.deltaTime);
	}
}
