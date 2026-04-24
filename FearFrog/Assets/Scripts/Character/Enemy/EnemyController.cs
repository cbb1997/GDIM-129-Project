using UnityEngine;

public enum EnemyState 
{ 
    Inactive,
    Idle,
    Alert,
    Aggressive,
    Attacking
}

public class EnemyController : MonoBehaviour
{
    private EnemyState m_CurrentState;

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    private void SetCurrentState(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Inactive:
                InactiveBehavior();
                break;
            case EnemyState.Idle:
                IdleBehavior();
                break;
            case EnemyState.Alert:
                AlertBehavior();
                break;
            case EnemyState.Aggressive:
                AlertBehavior();
                break;
            case EnemyState.Attacking:
                AttackBehavior();
                break;
            default:
                break;
        }

        m_CurrentState = state;
    }

    private void InactiveBehavior() { }
    private void IdleBehavior() { }
    private void AlertBehavior() { }
    private void AggressiveBehavior() { }
    private void AttackBehavior() { }
}
