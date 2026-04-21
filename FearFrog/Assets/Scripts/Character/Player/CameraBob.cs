using System;
using UnityEngine;

public class CameraBob : MonoBehaviour
{
    // Member variables
    [SerializeField] private bool m_bobEnabled = true;
    [SerializeField, Range(0f, 30f)] private float m_walkFrequency = 8f;        // Bob effect frequency
    [SerializeField, Range(0f, 30f)] private float m_sprintFrequency = 8f;
    [SerializeField, Range(0f, 30f)] private float m_crouchFrequency = 8f;
    [SerializeField, Range(0.1f, 1f)] private float m_walkMagModifier = 0.2f;   // Modifier for maginitude of bob effect
    [SerializeField, Range(0.1f, 1f)] private float m_sprintMagModifier = 0.2f;
    [SerializeField, Range(0.1f, 1f)] private float m_crouchMagModifier = 0.2f;
    private float m_amplitude = 0.001f;
    private float m_toggleSpeed = 0.3f;     // Speed threshold for whether apply bob effect
    private Vector3 m_startPos;
    private float timer = 0f;
    
    // Reference to other components or gameObjects
    [SerializeField] private Transform m_cameraHolder;
    [SerializeField] private Transform m_camera;
    private PlayerController m_playerController;
    
    
    // Start
    void Start()
    {
        // Varialbe initialization
        m_playerController = this.GetComponent<PlayerController>();
        m_startPos = m_camera.localPosition;
    }

    // Update
    void Update()
    {
        if (!m_bobEnabled) return;

        // Check to play camera bob effect
        if (!m_playerController.IsGrounded || m_playerController.Speed < m_toggleSpeed)
        {
            StopBob();
        }
        else
        {
            PerformBob();
        }
    }

    // Play bob motion on the camera
    private void PerformBob()
    {
        // Check which frequency and magnitude modifier to use
        float frequency, magModifier;
        if (m_playerController.IsSprinting)
        {
            frequency = m_sprintFrequency;
            magModifier = m_sprintMagModifier;
        }
        else if (m_playerController.IsCrouching)
        {
            frequency = m_crouchFrequency;
            magModifier = m_crouchMagModifier;
        }
        else
        {
            frequency = m_walkFrequency;
            magModifier = m_walkMagModifier;
        }
        
        // Calculate and perform bob offset
        timer += Time.deltaTime;
        Vector3 offset = new Vector3();
        offset.x = (1.4f * m_amplitude * m_walkMagModifier) * Mathf.Cos((m_walkFrequency / 2f) * timer);
        offset.y = (1f * m_amplitude * m_walkMagModifier) * Mathf.Sin(m_walkFrequency * timer);
        
        m_camera.localPosition += offset;
    }

    // Stop bob motion and move camera back to start position
    private void StopBob()
    {
        timer = 0f;
        if (m_camera.localPosition == m_startPos) return;
        m_camera.localPosition = Vector3.Lerp(m_camera.localPosition, m_startPos, 7f * Time.deltaTime);
    }
}


    
