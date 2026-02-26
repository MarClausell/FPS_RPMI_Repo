using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    #region General Variables
    [Header("Movement & Look")]
    [SerializeField] GameObject camHolder; //Ref al objeto que tiene como hijo la cámara (rota por la cámara)
    [SerializeField] float speed = 5f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float crouchSpeed = 3f;
    [SerializeField] float maxForce = 1f; //fuerza máxima de aceleración
    [SerializeField] float sensitivity = 0.1f; //sensibilidad para el input de look

    [Header("Jump & GroundCheck")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] bool isGrounded;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.3f;
    [SerializeField] LayerMask groundLayer;

    [Header("Player State Bools")]
    [SerializeField] bool isSprinting;
    [SerializeField] bool isCrouching;
    #endregion

    //Variables de referencia privadas
    Rigidbody rb; //Ref al rigidbody del player

    //Variables para el input
    Vector2 moveInput;
    Vector2 lookInput;
    float lookRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Lock del cursor del ratón
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; //Oculta el cursor de la vista
    }

    // Update is called once per frame
    void Update()
    {
        //Groundcheck
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        //Dibujar un rayo ficticeo en escena para determinar la orientacion de la camara
        Debug.DrawRay(camHolder.transform.position, camHolder.transform.forward * 100f, Color.red);
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void LateUpdate()
    {
        CameraLook();
    }

    void CameraLook()
    {
        //rotación horizontal del cuerpo del personaje
        transform.Rotate(Vector3.up * lookInput.x * sensitivity);
        //Rotación vertical (la lleva la camara)
        lookRotation += (-lookInput.y * sensitivity);
        lookRotation = Mathf.Clamp(lookRotation, -90, 90);
        camHolder.transform.localEulerAngles = new Vector3(lookRotation, 0f, 0f);
    }

    void Movement()
    {
        Vector3 currentVelocity = rb.linearVelocity; //Necesitamos calcular la velocidad actual del rb constantemente
        Vector3 targetVelocity = new Vector3(moveInput.x, 0, moveInput.y); //velocidad a alacanzar = la direccion que pulsamos
        targetVelocity *= isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : speed);
       
        //Convertir la dirección local en grlobal
        targetVelocity = transform.TransformDirection(targetVelocity);

        //Calcular el cambio de velocidad (aceleración)
        Vector3 velocityChange = (targetVelocity - currentVelocity);
        velocityChange = new Vector3(velocityChange.x, 0f, velocityChange.z);
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        //Aplicar la fuerza de movimiento/aceleracion
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

    }

    void Jump()
    {
        if (isGrounded) rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    #region Input Methods
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed) Jump();
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isCrouching = !isCrouching;
            //Añadir la animacion de agacharse
        }
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed && !isCrouching) isSprinting = true;
        if (context.canceled) isSprinting = false;
    }
    #endregion

}
