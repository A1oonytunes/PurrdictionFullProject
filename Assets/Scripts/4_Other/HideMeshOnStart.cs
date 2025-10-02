using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// this script hides the mesh of an object while leaving the shadows
public class HideMeshOnStart : MonoBehaviour
{
    private void Start()
    {
        GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    }
}
