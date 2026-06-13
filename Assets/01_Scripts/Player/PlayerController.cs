using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 5f;

    [Header("Camara")]
    public Transform cameraPivot;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    private Rigidbody rb;
    private float pitch = 0f;

    [Header("Sonidos")]
    [SerializeField] private AudioSource walkAudioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Evita que el Rigidbody se caiga de lado
        rb.freezeRotation = true;

        // Oculta y bloquea el cursor al centro de la pantalla
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
    }

    private void FixedUpdate()
    {
        Move();
    }

    void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 direction =
            transform.forward * v +
            transform.right * h;

        direction.Normalize();

        Vector3 velocity =
            direction * moveSpeed;

        velocity.y = rb.linearVelocity.y;

        rb.linearVelocity = velocity;

        // Sonido de pasos: activo si hay movimiento horizontal
        if (SoundManager.Instance != null && walkAudioSource != null)
        {
            bool isMoving = direction.magnitude > 0.1f;
            SoundManager.Instance.SetLoopingSFX(walkAudioSource, SoundManager.Instance.playerWalk, isMoving);
        }
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        pitch -= mouseY;
        pitch = Mathf.Clamp(
            pitch,
            -maxLookAngle,
            maxLookAngle
        );

        cameraPivot.localRotation =
            Quaternion.Euler(pitch, 0f, 0f);
    }
}