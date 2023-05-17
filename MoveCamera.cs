using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform CameraPosition;
    // Update is called once per frame
    void Update()
    {
        this.transform.position = CameraPosition.position;
    }
}
