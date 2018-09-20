using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBlockIgnoreColide : MonoBehaviour
{
    public MeshCollider meshColiderComponent;
    public int ignoreCollidLayer1 = 11;
    public int ignoreCollidLayer2 = 13;
    // Use this for initialization

    private void Awake()
    {
        meshColiderComponent = GetComponent<MeshCollider>();

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == ignoreCollidLayer1 || collision.gameObject.layer == ignoreCollidLayer2)
        {
            Physics.IgnoreCollision(meshColiderComponent, collision.collider);
        }

    }
}
