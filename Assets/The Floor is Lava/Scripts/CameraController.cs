using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerTarget;
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - playerTarget.position;
    }

    private void LateUpdate()
    {
        transform.position = offset + playerTarget.position;
    }
}
