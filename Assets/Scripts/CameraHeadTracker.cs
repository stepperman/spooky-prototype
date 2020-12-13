using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHeadTracker : MonoBehaviour
{
    [SerializeField] private Transform trackingTransform;
    [SerializeField] private Transform reference;

    private void Update()
    {
        if (!reference.gameObject.activeSelf) return;
        
        trackingTransform.position = reference.position;
        trackingTransform.rotation = reference.rotation;
    }
}
