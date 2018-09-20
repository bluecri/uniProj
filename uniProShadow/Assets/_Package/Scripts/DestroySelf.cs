using UnityEngine;
using System.Collections;

public class DestroySelf : MonoBehaviour {
	public float destroyTime;

	void Awake() {
		StartCoroutine("DeferredDestroy");
	}

	IEnumerator DeferredDestroy() {
		yield return new WaitForSeconds(destroyTime);
		Destroy(gameObject);
	}
}
