using UnityEngine;
using System.Collections;

[RequireComponent(typeof(StatsProfile))]
public class SkeletonBehavior : MonoBehaviour
{
    [Header("Detection / AI")]
    public float detectionRadus = 10.0f;
    public float detectionAngle = 90.0f;
    public float timeToStopPursuit = 5.0f;
    public float timeToWaitOnPursuit = 2.0f;

    [Header("Chase / Spacing")]
    [SerializeField] private float desiredAttackDistance = 1.35f;
    [SerializeField] private float reApproachBuffer = 0.1f;

    [Header("Patrol")]
    [SerializeField] private bool enablePatrol = true;
    [SerializeField] private float patrolRadius = 4f;
    [SerializeField] private float patrolWaitMin = 2f;
    [SerializeField] private float patrolWaitMax = 4f;
    [SerializeField] private float patrolPointTolerance = 0.35f;

    [Header("Death")]
    [SerializeField] private float despawnDelay = 15f;

    [SerializeField] private float hitReactCooldown = 0.15f;
    private float lastHitReactTime = -999f;

    private bool isDead;
    private bool isReturningHome;
    private bool isWaitingToPatrol;
    private bool _registeredCombat;
    private Vector3 m_CurrentPatrolPoint;
    private Coroutine patrolCoroutine;
    private Coroutine returnCoroutine;

    private StatsProfile statsProfile;
    private PlayerControllerHub m_Target;
    private Animator m_Animator;
    private UnityEngine.AI.NavMeshAgent m_NavMeshAgent;
    private float m_TimeSinceLostTarget = 0;
    private Vector3 m_OriginPosition;

    private readonly int m_HashInPursuit = Animator.StringToHash("InPursuit");
    private readonly int m_HashNearBase = Animator.StringToHash("NearBase");
    private readonly int m_HashDie = Animator.StringToHash("Die");
    private readonly int m_HashHit = Animator.StringToHash("Hit");

    private void Awake()
    {
        statsProfile = GetComponent<StatsProfile>();
        m_NavMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        m_Animator = GetComponent<Animator>();
        m_OriginPosition = transform.position;
        m_CurrentPatrolPoint = m_OriginPosition;

        if (m_NavMeshAgent != null)
        {
            m_NavMeshAgent.stoppingDistance = 0f;
        }
    }

    private void OnEnable()
    {
        if (statsProfile == null)
        {
            statsProfile = GetComponent<StatsProfile>();
        }

        if (statsProfile == null)
        {
            Debug.LogWarning($"SkeletonBehavior on {name} requires a StatsProfile.");
            return;
        }

        statsProfile.OnDamaged += HandleDamaged;
        statsProfile.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        if (statsProfile == null)
        {
            return;
        }

        statsProfile.OnDamaged -= HandleDamaged;
        statsProfile.OnDied -= HandleDeath;
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        var target = LookForPlayer();

        if (m_Target == null)
        {
            if (target != null)
            {
                CancelPatrolState();
                m_Target = target;
                m_TimeSinceLostTarget = 0f;
                isReturningHome = false;

                if (!_registeredCombat)
                {
                    _registeredCombat = true;
                    AudioManager.RegisterCombat();
                }
            }
            else
            {
                HandlePatrol();
            }
        }
        else
        {
            if (m_NavMeshAgent != null)
            {
                m_NavMeshAgent.stoppingDistance = desiredAttackDistance;
            }

            float distToPlayer = Vector3.Distance(transform.position, m_Target.transform.position);

            if (distToPlayer <= desiredAttackDistance + reApproachBuffer)
            {
                if (m_NavMeshAgent != null)
                {
                    m_NavMeshAgent.isStopped = true;
                }

                if (m_Animator != null)
                {
                    m_Animator.SetBool(m_HashInPursuit, false);
                }
            }
            else
            {
                if (m_NavMeshAgent != null)
                {
                    m_NavMeshAgent.isStopped = false;
                    m_NavMeshAgent.SetDestination(m_Target.transform.position);
                }

                if (m_Animator != null)
                {
                    m_Animator.SetBool(m_HashInPursuit, true);
                }
            }

            var seenTarget = LookForPlayer();
            if (seenTarget == null)
            {
                m_TimeSinceLostTarget += Time.deltaTime;
                if (m_TimeSinceLostTarget >= timeToStopPursuit)
                {
                    m_Target = null;
                    m_TimeSinceLostTarget = 0f;

                    if (_registeredCombat)
                    {
                        _registeredCombat = false;
                        AudioManager.UnregisterCombat();
                    }

                    if (m_NavMeshAgent != null)
                    {
                        m_NavMeshAgent.isStopped = true;
                    }

                    if (m_Animator != null)
                    {
                        m_Animator.SetBool(m_HashInPursuit, false);
                    }

                    if (returnCoroutine != null)
                    {
                        StopCoroutine(returnCoroutine);
                    }

                    returnCoroutine = StartCoroutine(WaitOnPursuit());
                }
            }
            else
            {
                m_TimeSinceLostTarget = 0;
            }
        }

        Vector3 toBase = m_OriginPosition - transform.position;
        toBase.y = 0;

        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashNearBase, toBase.magnitude < 0.2f);
        }
    }

    private void HandlePatrol()
    {
        if (!enablePatrol || m_NavMeshAgent == null || m_Animator == null)
        {
            return;
        }

        if (isReturningHome)
        {
            float distToHome = Vector3.Distance(transform.position, m_OriginPosition);
            if (distToHome <= patrolPointTolerance)
            {
                isReturningHome = false;
                m_NavMeshAgent.isStopped = true;
                m_Animator.SetBool(m_HashInPursuit, false);
                StartPatrolWait();
            }

            return;
        }

        if (isWaitingToPatrol)
        {
            return;
        }

        if (!m_NavMeshAgent.pathPending && m_NavMeshAgent.hasPath)
        {
            float distToPatrolPoint = Vector3.Distance(transform.position, m_CurrentPatrolPoint);
            if (distToPatrolPoint <= patrolPointTolerance)
            {
                m_NavMeshAgent.isStopped = true;
                m_Animator.SetBool(m_HashInPursuit, false);
                StartPatrolWait();
            }
        }
        else if (!m_NavMeshAgent.hasPath)
        {
            StartPatrolWait();
        }
    }

    private void StartPatrolWait()
    {
        if (!enablePatrol || isWaitingToPatrol || isDead || m_Target != null)
        {
            return;
        }

        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
        }

        patrolCoroutine = StartCoroutine(PatrolWaitAndMove());
    }

    private IEnumerator PatrolWaitAndMove()
    {
        isWaitingToPatrol = true;

        float waitTime = Random.Range(patrolWaitMin, patrolWaitMax);
        yield return new WaitForSeconds(waitTime);

        isWaitingToPatrol = false;

        if (isDead || m_Target != null || m_NavMeshAgent == null)
        {
            patrolCoroutine = null;
            yield break;
        }

        Vector3 patrolPoint = GetRandomPatrolPoint();
        m_CurrentPatrolPoint = patrolPoint;

        m_NavMeshAgent.stoppingDistance = 0f;
        m_NavMeshAgent.isStopped = false;

        bool gotPath = m_NavMeshAgent.SetDestination(patrolPoint);

        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashInPursuit, gotPath);
        }

        patrolCoroutine = null;
    }

    private Vector3 GetRandomPatrolPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 random2D = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = m_OriginPosition + new Vector3(random2D.x, 0f, random2D.y);

            if (UnityEngine.AI.NavMesh.SamplePosition(candidate, out UnityEngine.AI.NavMeshHit hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return m_OriginPosition;
    }

    private void CancelPatrolState()
    {
        isReturningHome = false;
        isWaitingToPatrol = false;

        if (patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            patrolCoroutine = null;
        }

        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }
    }

    private void HandleDamaged(float _)
    {
        if (isDead || m_Animator == null)
        {
            return;
        }

        if (Time.time >= lastHitReactTime + hitReactCooldown)
        {
            lastHitReactTime = Time.time;
            m_Animator.SetTrigger(m_HashHit);
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

        CancelPatrolState();

        if (m_NavMeshAgent != null)
        {
            m_NavMeshAgent.isStopped = false;
            m_NavMeshAgent.stoppingDistance = desiredAttackDistance;
        }

        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashInPursuit, true);
        }
    }

    private void HandleDeath()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        CancelPatrolState();

        if (_registeredCombat)
        {
            _registeredCombat = false;
            AudioManager.UnregisterCombat();
        }

        GetComponent<EnemyLootDropper>()?.DropLoot();

        var attack = GetComponent<SkeletonMinionAttack>();
        if (attack != null) attack.enabled = false;

        if (m_NavMeshAgent != null)
        {
            m_NavMeshAgent.isStopped = true;
            m_NavMeshAgent.ResetPath();
        }

        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashInPursuit, false);
            m_Animator.SetTrigger(m_HashDie);
        }

        foreach (var col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        StartCoroutine(DespawnAfterDelay());
    }

    private IEnumerator DespawnAfterDelay()
    {
        yield return new WaitForSeconds(despawnDelay);
        Destroy(gameObject);
    }

    private IEnumerator WaitOnPursuit()
    {
        yield return new WaitForSeconds(timeToWaitOnPursuit);

        if (isDead || m_NavMeshAgent == null)
        {
            yield break;
        }

        isReturningHome = true;
        m_NavMeshAgent.stoppingDistance = 0f;
        m_NavMeshAgent.isStopped = false;
        m_NavMeshAgent.SetDestination(m_OriginPosition);

        if (m_Animator != null)
        {
            m_Animator.SetBool(m_HashInPursuit, true);
        }

        returnCoroutine = null;
    }

    private PlayerControllerHub LookForPlayer()
    {
        if (PlayerControllerHub.Instance == null)
        {
            return null;
        }

        Vector3 enemyPosition = transform.position;
        Vector3 toPlayer = PlayerControllerHub.Instance.transform.position - enemyPosition;
        toPlayer.y = 0;

        if (toPlayer.magnitude <= detectionRadus)
        {
            if (Vector3.Dot(toPlayer.normalized, transform.forward) >
                Mathf.Cos(detectionAngle * 0.5f * Mathf.Deg2Rad))
            {
                return PlayerControllerHub.Instance;
            }
        }

        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Color detectionColor = new Color(0.8f, 0, 0, 0.5f);
        UnityEditor.Handles.color = detectionColor;

        Vector3 rotatedForward = Quaternion.Euler(0, -detectionAngle * 0.5f, 0) * transform.forward;
        UnityEditor.Handles.DrawSolidArc(transform.position, Vector3.up, rotatedForward, detectionAngle, detectionRadus);

        if (enablePatrol)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, patrolRadius);
        }
    }
#endif
}
