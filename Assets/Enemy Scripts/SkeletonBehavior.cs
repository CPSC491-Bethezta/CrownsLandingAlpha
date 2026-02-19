using UnityEngine;

public class SkeletonBehavior : MonoBehaviour
{
    public float detectionRadus = 10.0f;
    public float detectionAngle = 90;

    private PlayerController m_Target;
    private UnityEngine.AI.NavMeshAgent m_NavMeshAgent;

    private void Awake(){
        m_NavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    private void Start(){
        Debug.Log(PlayerController.Instance);
    }

    private void Update(){
        m_Target = LookForPlayer();

        if(!m_Target){
            return;
        }
        Vector3 targetPosition = m_Target.transform.position;
        
        m_NavMeshAgent.SetDestination(targetPosition);
    }

    private PlayerController LookForPlayer(){
        if(PlayerController.Instance == null){
            return null;
        }

        Vector3 enemyPosition = transform.position;
        Vector3 toPlayer = PlayerController.Instance.transform.position - enemyPosition;
        toPlayer.y = 0;

        if(toPlayer.magnitude <= detectionRadus){
            if(Vector3.Dot(toPlayer.normalized, transform.forward) >
                Mathf.Cos(detectionAngle * 0.5f * Mathf.Deg2Rad))
                {
                    return PlayerController.Instance;
            }
        }
        return null;
    }
#if UNITY_EDITOR
    private void OnDrawGizmosSelected(){
        Color c = new Color(0.8f,0,0, 0.5f);
        UnityEditor.Handles.color = c;

        Vector3 rotatedForward = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward;
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, rotatedForward, detectionAngle, detectionRadus);
    }
#endif
}
