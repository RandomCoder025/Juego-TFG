using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementMKII : MonoBehaviour
{
    #region variables
    [Header("Movement")] 
    private float moveSpeed;

    public float walkSpeed;
    public float sprintSpeed; 
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")] 
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")] 
    public CapsuleCollider playerCollider;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    public AudioSource salto;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.CheckCapsule(playerCollider.bounds.center, new Vector3(playerCollider.bounds.center.x, playerCollider.bounds.min.y, playerCollider.bounds.center.z), playerCollider.radius * 0.9f, whatIsGround);

        MyInput();
        MovePlayer();
        SpeedControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
    }

    // Captura las entradas de teclas del jugador
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Verifica si se puede saltar
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            salto.Play();
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    // Mueve al jugador
    private void MovePlayer()
    {
        // Calcula la dirección del movimiento en base a la orientación del GameObject
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Si está en el suelo, aplica fuerza para mover al jugador en base a la dirección
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        // Si está en el aire, aplica fuerza con un multiplicador para el movimiento en el aire
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    // Controla la velocidad de movimiento
    private void SpeedControl()
    {   
        // Permite correr al personaje
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveSpeed = sprintSpeed; 
        }
        else
        {
            moveSpeed = walkSpeed; 
        }

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        
        // Limita la velocidad si excede la velocidad máxima establecida
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    // Realiza el salto
    private void Jump()
    {
        // Resetea la velocidad en el eje Y
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Aplica una fuerza hacia arriba para ejecutar el salto
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    // Resetea la capacidad de saltar
    private void ResetJump()
    {
        readyToJump = true;
    }
}
