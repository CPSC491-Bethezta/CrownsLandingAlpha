using UnityEngine;
using System.Collections;

public class SkeletonBehavior : MonoBehaviour
{
    [Header("Detection / AI")]
    public float detectionRadus = 10.0f;
    public float detectionAngle = 90.0f;
    public float timeToStopPursuit = 5.0f;
    public float timeToWaitOnPursuit = 2.0f;

    [Header("Chase / Spacing")]
    [SerializeField] private float desiredAttackDistance = 3.0f;
    [SerializeField] private float reApproachBuffer = 0.25f;    

    [Header("Health")]
    [SerializeField] private float maxHp = 100f;
    [SerializeField] private float despawnDelay = 15f;
    [SerializeField] private DamagePopup damagePopupPrefab;

    [SerializeField] private float hitReactCooldown = 0.15f;
    private float lastHitReactTime = -999f;

    private float currentHp;
    private bool isDead;

    private PlayerController m_Target;
    private Animator m_Animator;
    private UnityEngine.AI.NavMeshAgent m_NavMeshAgent;
    private float m_TimeSinceLostTarget = 0;
    private Vector3 m_OriginPosition;

    private readonly int m_HashInPursuit = Animator.StringToHash("InPursuit");
    private readonly int m_HashNearBase = Animator.StringToHash("NearBase");
    private readonly int m_HashDie = Animator.StringToHash("Die");
    private readonly int m_HashHit = Animator.StringToHash("Hit");

    private void Awake(){
    m_NavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    m_Animator = GetComponent<Animator>();
    m_OriginPosition = transform.position;

    currentHp = maxHp;

    if (m_NavMeshAgent != null)
        m_NavMeshAgent.stoppingDistance = desiredAttackDistance;
}

    private void Update(){
        if(isDead){
             return;
        }
        var target = LookForPlayer();

        if(m_Target == null){
            if(target != null){
                m_Target = target;
            }
        }else
{
    float distToPlayer = Vector3.Distance(transform.position, m_Target.transform.position);

    if (distToPlayer <= desiredAttackDistance + reApproachBuffer)
    {
        m_NavMeshAgent.isStopped = true;
        m_Animator.SetBool(m_HashInPursuit, false);
    }
    else
    {
        m_NavMeshAgent.isStopped = false;
        m_NavMeshAgent.SetDestination(m_Target.transform.position);
        m_Animator.SetBool(m_HashInPursuit, true);
    }

    var seenTarget = LookForPlayer();
    if (seenTarget == null)
    {
        m_TimeSinceLostTarget += Time.deltaTime;
        if (m_TimeSinceLostTarget >= timeToStopPursuit)
        {
            m_Target = null;
            m_NavMeshAgent.isStopped = true;
            m_Animator.SetBool(m_HashInPursuit, false);
            StartCoroutine(WaitOnPursuit());
        }
    }
    else
    {
        m_TimeSinceLostTarget = 0;
    }
}

        Vector3 toBase = m_OriginPosition - transform.position;
        toBase.y = 0;

        m_Animator.SetBool(m_HashNearBase, toBase.magnitude < 0.2f);
    }
    public void TakeDamage(float damage)
{
    if (isDead) return;

    currentHp -= damage;

    if (damagePopupPrefab != null)
    {
        DamagePopup popup = Instantiate(
            damagePopupPrefab,
            transform.position + Vector3.up,
            Quaternion.identity
        );
        popup.Setup(damage);
    }

    if (currentHp > 0f && m_Animator != null)
    {
        if (Time.time >= lastHitReactTime + hitReactCooldown)
        {
            lastHitReactTime = Time.time;
            m_Animator.SetTrigger(m_HashHit);
        }
    }

    if (currentHp <= 0f)
    {
        Die();
    }
}
    public void PauseMovement()
{
    if (m_NavMeshAgent != null)
    {
        m_NavMeshAgent.isStopped = true;
        m_NavMeshAgent.ResetPath();
    }
    if (m_Animator != null)
    {
        m_Animator.SetBool(m_HashInPursuit, false);
    }
}

public void ResumeChase()
{
    if (isDead) return;
    if (m_NavMeshAgent != null) m_NavMeshAgent.isStopped = false;
    if (m_Animator != null) m_Animator.SetBool(m_HashInPursuit, true);
}

    private void Die(){
        isDead = true;
        var attack = GetComponent<SkeletonMinionAttack>();
        if (attack != null) attack.enabled = false;
        if(m_NavMeshAgent != null){
            m_NavMeshAgent.isStopped = true;
            m_NavMeshAgent.ResetPath();
        }
         if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashInPursuit, false);
            m_Animator.SetTrigger(m_HashDie);
        }

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        StartCoroutine(DespawnAfterDelay());
    }

    private IEnumerator DespawnAfterDelay(){
        yield return new WaitForSeconds(despawnDelay);
        Destroy(gameObject);
    }

    private IEnumerator WaitOnPursuit(){
        yield return new WaitForSeconds(timeToWaitOnPursuit);
        if(isDead) yield break;
        m_NavMeshAgent.isStopped = false;
        m_NavMeshAgent.SetDestination(m_OriginPosition);
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
