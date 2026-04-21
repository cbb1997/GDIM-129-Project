using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
	// Player state variable
	private bool m_isGrounded = true;
    private bool m_isSprinting = false;
    private bool m_isCrouching = false;

    // Player movement variables
    [SerializeField] private float m_walkAcceleration = 35f;
    [SerializeField] private float m_maxWalkVelocity = 6f;
    [SerializeField] private float m_sprintAcceleration = 60f;
    [SerializeField] private float m_maxSprintVelocity = 9f;
    [SerializeField] private float m_crouchAcceleration = 15f;
    private float m_standCameraHeight = 0.85f;  // Controls CameraHolder Height
    private float m_crouchCameraHeight = 0.4f;
    private float m_crouchShrinkRatio = 0.45f;   // Scale factor for player entity when crouching
    private float m_currMoveAcceleration;
    private float m_currMaxVelocity;
    
    [SerializeField] private float m_jumpForce = 5f;
    [SerializeField] private float m_groundDrag = 5f;
    [SerializeField] private float m_airDrag = 0f;
    private float m_gcRadius = 0.35f;    // Grounndeed check shpere radius

	[SerializeField] private float m_cameraSensitivity = 100f;
    private float m_xOritation = 0; // Record of player look direction
    private float m_yOritation = 0;
    
	// Reference to other components or gameObjects
    [SerializeField] private Transform m_playerEntity;
    [SerializeField] private Transform m_camera;
    [SerializeField] private Transform m_footPos;
	private Rigidbody m_rb;
    private PhysicsMaterial m_playerMaterial;
    
    // Getter
    public bool IsGrounded { get {return m_isGrounded; } }
    public float Speed { get { return m_rb.linearVelocity.magnitude; } }
    
    
    
    // Start
    void Start()
    {
        // Varialbe initialization
        m_rb = this.GetComponent<Rigidbody>();
        m_playerMaterial = m_playerEntity.gameObject.GetComponent<Collider>().material;
        m_currMoveAcceleration = m_walkAcceleration;
        m_currMaxVelocity = m_maxWalkVelocity;
        
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Link Jump functionality to input system
        InputController.Instance.Input.Player.Jump.performed += Jump;
        InputController.Instance.Input.Player.Sprint.performed += ToggleSprint;
        InputController.Instance.Input.Player.Crouch.performed += ToggleCrouch;
    }

    // Update per frame
    void Update()
    {
        // Player Look
        PlayerLook();
    }

    // Fixed Update for physics simulation
    void FixedUpdate()
    {
        // Player state check
        Vector3 groundedPointNormal = GroundedCheck();
        SprintCheck();
        
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
        
        m_camera.rotation = Quaternion.Euler(-m_yOritation, m_xOritation, 0f);
        m_playerEntity.rotation = Quaternion.Euler(0f, m_xOritation, 0f);
    }
    
    // Handles player's movement
    private void PlayerMove(Vector3 groundedPointNormal)
    {
        // Calculate new input move direction
        Vector2 input = InputController.Instance.Input.Player.Move.ReadValue<Vector2>();
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);
        moveDirection = (Quaternion.Euler(0f, m_xOritation, 0f) * moveDirection).normalized;

        // Modify input move direction to be parallel to the ground if player is grounded
        float cos;
        if (m_isGrounded)
        {
            cos = Vector3.Dot(moveDirection, groundedPointNormal);    // Cosine of angle between new move direction and normal of surface
            float degTheta = Mathf.Acos(cos) * Mathf.Rad2Deg;          // Angle between new move direction and normal of surface in degrees
            degTheta -= 90;
            
            moveDirection = Quaternion.AngleAxis(degTheta, Vector3.Cross(moveDirection, Vector3.up)) * moveDirection;
            moveDirection = moveDirection.normalized;
        }
        
        m_rb.AddForce(moveDirection * m_currMoveAcceleration, ForceMode.Acceleration);
    }
    
    // Perfomr player jump action
    private void Jump(InputAction.CallbackContext ctx)
    {
        // Perform jump is player is grounded
        if (m_isGrounded)
        {
            StopCrouching();
            m_rb.AddForce(new Vector3(0f, m_jumpForce, 0f), ForceMode.VelocityChange);
            m_isGrounded = false;
        }
    }
    
    // Toggle player sprint
    private void ToggleSprint(InputAction.CallbackContext ctx)
    {
        if (m_isGrounded)   // Only allow toggling when player's grounded
        {
            if (m_isSprinting)      // Stop sprinting
            {
                StopSprinting();
            }
            else                    // Start sprinting
            {
                StopCrouching();
                StartSprinting();
            }
        }
    }

    // Start sprinting
    private void StartSprinting()
    {
        if (!m_isSprinting)
        {
            m_isSprinting = true;
            m_currMoveAcceleration = m_sprintAcceleration;
            m_currMaxVelocity = m_maxSprintVelocity;
        }
    }

    // Stop sprinting
    private void StopSprinting()
    {
        if (m_isSprinting)
        {
            m_isSprinting = false;
            m_currMoveAcceleration = m_walkAcceleration;
            m_currMaxVelocity = m_maxWalkVelocity;   
        }
    }
    
    // Toggle player crouch
    private void ToggleCrouch(InputAction.CallbackContext ctx)
    {
        if (m_isGrounded) // Only allow toggling when player's grounded
        {
            if (m_isCrouching)      // Stop crouching
            {
                StopCrouching();
            }
            else                    // Start crouching
            {
                StopSprinting();
                StartCrouching();
            }
        }
    }

    // Start crouching
    private void StartCrouching()
    {
        if (!m_isCrouching)
        {
            m_isCrouching = true;
            m_currMoveAcceleration = m_crouchAcceleration;
            // Update player entity and camera
            StartCoroutine(CrounchCameraChange(new Vector3(0f, m_crouchCameraHeight, 0f)));
        }
    }

    // Stop crouching
    private void StopCrouching()
    {
        if (m_isCrouching)
        {
            m_isCrouching = false;
            m_currMoveAcceleration = m_walkAcceleration;
            // Update player entity and camera
            StartCoroutine(CrounchCameraChange(new Vector3(0f, m_standCameraHeight, 0f)));
        }
    }

    private IEnumerator CrounchCameraChange(Vector3 targetPos)
    {
        while (Vector3.Distance(m_camera.localPosition, targetPos) >= 0.001f)
        {
            m_camera.localPosition = Vector3.Lerp(m_camera.localPosition, targetPos, 0.15f);
            yield return null;
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

    // Exit sprint if player stops moving
    private void SprintCheck()
    {
        if (m_rb.linearVelocity.magnitude < 0.0001f)
        {
            m_isSprinting = false;
            m_currMoveAcceleration = m_walkAcceleration;
            m_currMaxVelocity = m_maxWalkVelocity;
        }
    }
    
    
    // Contorl player's horizontal velocity by a maximum speed
    private void VelocityControl()
    {
        Vector3 currHorVelocity = m_rb.linearVelocity;
        currHorVelocity.y = 0f;
        if (currHorVelocity.magnitude > m_currMaxVelocity)
        {
            currHorVelocity = currHorVelocity.normalized * m_currMaxVelocity;
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
