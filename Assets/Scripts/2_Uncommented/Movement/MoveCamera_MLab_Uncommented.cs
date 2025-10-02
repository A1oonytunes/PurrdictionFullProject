using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera_MLab_Uncommented : MonoBehaviour
{
    public Transform cameraPos;

    private void Update()
    {
        transform.position = cameraPos.position;
    }
}
