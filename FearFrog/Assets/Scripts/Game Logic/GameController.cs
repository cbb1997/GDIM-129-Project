using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private VisualDictionary<string, int> m_Dict;

    private void Start()
    {
        Debugger.Log("Engine initialized...");

        m_Dict = new VisualDictionary<string, int>(5);
    }
}
