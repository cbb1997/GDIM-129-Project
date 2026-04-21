using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Member Variables
    [SerializeField] private float m_moveSpeed = 5f;
    [SerializeField] private float m_maxVelocity = 10f;
    [SerializeField] private float m_cameraSensitivity = 100f;
    [SerializeField] private float m_jumpForce = 5f;
    [SerializeField] private float m_groundDrag = 5f;
    [SerializeField] private float m_airDrag = 0f;
    
    private uint m_health = 0; // To be set later with scriptable objects
    private bool m_isGrounded = true;
    private float m_xOritation = 0; // Record of player look direction
    private float m_yOritation = 0;
    private Rigidbody m_rb;
    
    [SerializeField] private GameObject m_playerEntity;
    [SerializeField] private GameObject m_camera;
    [SerializeField] private Transform m_footPos;
    
    
    // Awake
    void Awake()
    {
        // Set player rigidbody
        m_rb = this.GetComponent<Rigidbody>();
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    // Start
    void Start()
    {
        // Link Jump functionality to input system
        InputController.Instance.Input.Player.Jump.performed += Jump;
    }

    // Update
    void Update()
    {
        // Player Grounded Check
        m_isGrounded = Physics.Raycast(m_footPos.position, Vector3.down, 0.15f, LayerMask.GetMask("Ground"));
        m_rb.linearDamping = m_isGrounded ? m_groundDrag : m_airDrag;
        
        // Player Look
        Vector2 lookDirection = InputController.Instance.Input.Player.Look.ReadValue<Vector2>();
        m_xOritation += lookDirection.x * m_cameraSensitivity * Time.deltaTime;
        m_yOritation += lookDirection.y * m_cameraSensitivity * Time.deltaTime;
        m_yOritation = Math.Clamp(m_yOritation, -90f, 90f);
        
        m_camera.transform.rotation = Quaternion.Euler(-m_yOritation, m_xOritation, 0f);
        m_playerEntity.transform.rotation = Quaternion.Euler(0f, m_xOritation, 0f);
        
        // Player Move
        Vector2 input = InputController.Instance.Input.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);
        moveDirection = (Quaternion.Euler(0f, m_xOritation, 0f) * moveDirection).normalized;

        m_rb.AddForce(moveDirection * m_moveSpeed, ForceMode.Force);

        Vector3 currHorVelocity = m_rb.linearVelocity;
        currHorVelocity.y = 0f;
        Debug.Log(currHorVelocity.magnitude);
    }

    // LateUpdate
    void LateUpdate()
    {
        // Player Velocity Control
        Vector3 currHorVelocity = m_rb.linearVelocity;
        currHorVelocity.y = 0f;
        
        if (currHorVelocity.magnitude > m_maxVelocity)
        {
            currHorVelocity = currHorVelocity.normalized * m_maxVelocity;
            currHorVelocity.y = m_rb.linearVelocity.y;
            m_rb.linearVelocity = currHorVelocity;
        }
    }
    
    
    // Player Jump
    private void Jump(InputAction.CallbackContext ctx)
    {
        if (m_isGrounded)
        {
            // Update player rigidbody velocity
            m_rb.AddForce(new Vector3(0f, m_jumpForce, 0f), ForceMode.VelocityChange);
            
            // Update player grounded state
            m_isGrounded = false;
        }
    }
}
