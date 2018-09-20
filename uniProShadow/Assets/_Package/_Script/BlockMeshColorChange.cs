using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockMeshColorChange : MonoBehaviour {
    Material originMaterial;
    public Material selectedMaterial;

    MeshRenderer meshRenderer;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originMaterial = meshRenderer.material;
        
    }

    public void DownColorOfMesh()
    {
        meshRenderer.material = selectedMaterial;
    }
    public void ResetColorOfMesh()
    {
        meshRenderer.material = originMaterial;
    }
}
