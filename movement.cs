using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class movement : MonoBehaviour
{
    public float playerSpeed = 5.0f;
    public float jumpPower = 7.0f;
    public float mouseSensitivity = 100.0f;

    private float distToGround;
    public float cameraRotation;
    private Vector3 moveDirection;
    public float gravity;
    public Transform transform;
    private Rigidbody _playerRigidbody;

    public float playerHorizontalRotation;
    public float playerVerticalRotation;

    public float mouseHorizontalSpeed;


    private void Start()
    {
        distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;

        _playerRigidbody = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // Inicializar la variable 'transform' para referenciar el componente Transform del objeto actual
        transform = GetComponent<Transform>();
        // Inicializar la variable 'cameraRotation' con la rotación inicial de la cámara
        cameraRotation = transform.localRotation.eulerAngles.x;
    }

    private void Update()
    {
        MovePlayer();
        RotatePlayer();

        /*bool suelo = Physics.Raycast(transform.position, -Vector3.up,
            GetComponent<CapsuleCollider>().height / 2 + 0.1f);*/
        moveDirection.y -= gravity * Time.deltaTime;

        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            //Jump();
        }

        /*if (Input.GetKeyDown(KeyCode.Space) && suelo)
        {
        }*/
    }

    private bool IsGrounded()
    {
        // Castear un rayo desde el centro del personaje hacia abajo para comprobar si está tocando el suelo
        float raycastDistance = distToGround + 0.1f; // Añadir un pequeño margen para evitar errores de redondeo
        RaycastHit hit;
        return Physics.Raycast(transform.position, -Vector3.up, out hit, raycastDistance);
    }


    private void MovePlayer()
    {
        var horizontalInput = Input.GetAxisRaw("Horizontal");
        var verticalInput = Input.GetAxisRaw("Vertical");

        // Obtener la dirección de movimiento en base a la rotación actual del personaje
        Vector3 moveDirection2 = Quaternion.Euler(0, cameraRotation, 0) *
                                 new Vector3(horizontalInput, 0, verticalInput).normalized;

        // Aplicar la dirección de movimiento a la velocidad del jugador
        _playerRigidbody.velocity = moveDirection2 * playerSpeed;
    }


    private void RotatePlayer()
    {
        // Obtener el movimiento del ratón y aplicarlo a la rotación de la cámara
        float mouseX = -Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY =
            Input.GetAxis("Mouse Y") * mouseSensitivity *
            Time.deltaTime; // Agregar esta línea para obtener la rotación vertical del ratón

        // Aplicar la rotación horizontal y vertical a la cámara
        cameraRotation -= mouseX;
        cameraRotation = Mathf.Clamp(cameraRotation, -90f, 90f); // Limitar la rotación vertical entre -90 y 90 grados
        transform.localRotation = Quaternion.Euler(cameraRotation, 0, 0); // Aplicar la rotación vertical a la cámara

        transform.parent.Rotate(Vector3.up * mouseY);
        // Rotar el cuerpo del personaje en base a la rotación vertical del ratón

        // Actualizar la rotación horizontal del personaje
        playerHorizontalRotation =
            transform.parent.localRotation.eulerAngles
                .y; // Actualizar la rotación horizontal del personaje en base a la rotación del padre
    }
}