using System;
using UnityEngine;

public class LimitFPS : MonoBehaviour
{
    private void Awake()
    {
        #if !UNITY_EDITOR
                Application.targetFrameRate = 240; 
        #endif
    }
    
}