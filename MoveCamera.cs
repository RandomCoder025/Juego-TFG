using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform CameraPosition;
    
    // Mueve este componente a la posición de los ojos del personaje 
    void Update()
    {
        this.transform.position = CameraPosition.position;
    }
}
