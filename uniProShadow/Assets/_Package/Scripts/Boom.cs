using UnityEngine;
using System.Collections;

public class Boom : MonoBehaviour {

	public GameObject boomObject;
	public Transform boomDummy;

	public void ShowBoom() {
		if( boomDummy != null && boomObject != null) {
			GameObject go = Instantiate( boomObject, boomDummy.position, Quaternion.identity) as GameObject;
		}
	}
}
