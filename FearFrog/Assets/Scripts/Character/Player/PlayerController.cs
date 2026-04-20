using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Member Variables
    [SerializeField] private float m_moveAcceleration = 35f;
    [SerializeField] private float m_maxVelocity = 6f;
    [SerializeField] private float m_cameraSensitivity = 100f;
    [SerializeField] private float m_jumpForce = 5f;
    [SerializeField] private float m_groundDrag = 5f;
    [SerializeField] private float m_airDrag = 0f;
    private float m_gcRadius = 0.35f;    // Grounndeed Check Shpere radius
    
    private uint m_health = 0; // To be set later with scriptable objects
    private bool m_isGrounded = true;
    private float m_xOritation = 0; // Record of player look direction
    private float m_yOritation = 0;
    private Rigidbody m_rb;
    private PhysicsMaterial m_playerMaterial;
    
    [SerializeField] private GameObject m_playerEntity;
    [SerializeField] private GameObject m_camera;
    [SerializeField] private Transform m_footPos;
    
    
    // Awake
    void Awake()
    {
        // Varialbe initialization
        m_rb = this.GetComponent<Rigidbody>();
        m_playerMaterial = m_playerEntity.GetComponent<Collider>().material;
        
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

    // Update per frame
    void Update()
    {
        // Player Look
        PlayerLook();
        Debugger.Log(m_playerMaterial.dynamicFriction.ToString());
    }

    // Fixed Update for physics simulation
    void FixedUpdate()
    {
        // Player Grounded Check
        Vector3 groundedPointNormal = GroundedCheck();
        
        // Player Move
        PlayerMove(groundedPointNormal);
        
        // Player movement adjustment
        AntiSlide(groundedPointNormal);
        VelocityControl();
    }
    

    // Handles player's looking around
    private void PlayerLook()
    {
        Vector2 lookDirection = InputController.Instance.Input.Player.Look.ReadValue<Vector2>();
        m_xOritation += lookDirection.x * m_cameraSensitivity * Time.deltaTime;
        m_yOritation += lookDirection.y * m_cameraSensitivity * Time.deltaTime;
        m_yOritation = Math.Clamp(m_yOritation, -90f, 90f);
        
        m_camera.transform.rotation = Quaternion.Euler(-m_yOritation, m_xOritation, 0f);
        m_playerEntity.transform.rotation = Quaternion.Euler(0f, m_xOritation, 0f);
    }
    
    // Handles player's movement
    private void PlayerMove(Vector3 groundedPointNormal)
    {
        // Calculate new input move direction
        Vector2 input = InputController.Instance.Input.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);
        moveDirection = (Quaternion.Euler(0f, m_xOritation, 0f) * moveDirection).normalized;

        // Modify input move direction to be parallel to the ground
        // if player is grounded
        float cos;
        if (m_isGrounded)
        {
            cos = Vector3.Dot(moveDirection, groundedPointNormal);    // Cosine of angle between new move direction and normal of surface
            float degTheta = Mathf.Acos(cos) * Mathf.Rad2Deg;          // Angle between new move direction and normal of surface in degrees
            degTheta -= 90;
            
            moveDirection = Quaternion.AngleAxis(degTheta, Vector3.Cross(moveDirection, Vector3.up)) * moveDirection;
            moveDirection = moveDirection.normalized;
        }
        
        m_rb.AddForce(moveDirection * m_moveAcceleration, ForceMode.Acceleration);
    }
    
    // Perfomr player jump action
    private void Jump(InputAction.CallbackContext ctx)
    {
        // Perform jump is player is grounded
        if (m_isGrounded)
        {
            m_rb.AddForce(new Vector3(0f, m_jumpForce, 0f), ForceMode.VelocityChange);
            m_isGrounded = false;
        }
    }

    // Check whether the player is grounded
    // Return grounded point's normal vector if grounded; (0, 0, 0) otherwise
    private Vector3 GroundedCheck()
    {
        // Grounded Check
        RaycastHit hitInfo;
        m_isGrounded = Physics.SphereCast(transform.position, m_gcRadius, Vector3.down, out hitInfo, -m_footPos.localPosition.y);
        m_rb.linearDamping = m_isGrounded ? m_groundDrag : m_airDrag;
        m_playerMaterial.dynamicFriction = m_isGrounded ? 0.6f : 0f;
        
        // Return
        return hitInfo.normal;
    }
    
    // Contorl player's horizontal velocity by a maximum speed
    private void VelocityControl()
    {
        Vector3 currHorVelocity = m_rb.linearVelocity;
        currHorVelocity.y = 0f;
        if (currHorVelocity.magnitude > m_maxVelocity)
        {
            currHorVelocity = currHorVelocity.normalized * m_maxVelocity;
            // Player's fall down speed should not be affected by velocity control
            currHorVelocity.y = m_rb.linearVelocity.y;
            m_rb.linearVelocity = currHorVelocity;
        }
    }

    // Make player anti-sliding when standing on slopes
    private void AntiSlide(Vector3 groundedPointNormal)
    {
        float cos;
        if (m_isGrounded)
        {
            // Calculate downward force by gravity
            // and add it back to alleviate sliding
            cos = Vector3.Dot(-groundedPointNormal, Vector3.down);    // Cosine of angle between gravity force and normal force
            float gravityForceMag = m_rb.mass * Physics.gravity.y;
            Vector3 gravityForce = new Vector3(0f, -gravityForceMag, 0f);
            Vector3 gravityNormal = (cos * gravityForceMag) * (-groundedPointNormal);
            
            m_rb.AddForce((gravityForce - gravityNormal), ForceMode.Acceleration);
        }
    }
}
