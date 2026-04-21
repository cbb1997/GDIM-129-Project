using UnityEngine;

public class InputController : MonoBehaviour
{
    // Member Variables
    private static InputController m_instance;
    public static InputController Instance { get { return m_instance; } }

    private PlayerInput m_input;
    public PlayerInput Input { get { return m_input; } }
    
    
    // Awake
    void Awake()
    {
        // Singleton
        if (m_instance != null && m_instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        // Initialization
        m_instance = this;
        m_input = new PlayerInput();
        m_input.Player.Enable();
    }
}
