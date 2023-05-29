using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX; // Sensibilidad del mouse en el eje X
    public float sensY; // Sensibilidad del mouse en el eje Y

    public Transform orientation; // Orientación del jugador
    public Transform camHolder; // GameObject que sujeta la cámara

    float xRotation; // Rotación en el eje X
    float yRotation; // Rotación en el eje Y

    private void Start()
    {
        // Bloquear el ratón al iniciar el juego
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Obtener la entrada delmovimiento del ratón
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // Actualizar la rotación en el eje Y
        yRotation += mouseX;

        // Actualizar la rotación en el eje X y limita su rotación
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 60f);

        // Rotar correctamente la cámara
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

