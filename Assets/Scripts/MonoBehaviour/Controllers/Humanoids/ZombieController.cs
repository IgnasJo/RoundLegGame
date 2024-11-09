using System;
using System.Collections;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine;

public class ZombieController : MonoBehaviour, IDamagable, IKnockable
{
    private HealthProvider healthProvider;
    Collider myCollider;

    [SerializeField] private float maxHealth = 3;
    //[SerializeField] private float knockbackForce = 5f; // Add a knockback force variable

    [SerializeField] private int scoreGivenOnDeath = 10;
    [SerializeField] private int xpGivenOnDeath = 10;
    private readonly float chaseTargetCooldownSeconds = 0.5f;
    private readonly float speedDeltaToPlayer = 1f;
    private readonly float OUT_OF_BOUNDS_CHECK_SECONDS = 0.5f;
    private readonly float SUICIDE_Y = -10f;

    private GameObject nearestPlayer;
    private Rigidbody rb;
    private bool isDying = false;
    private MainHudController mainHudController;
    
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        mainHudController = GameObject.Find("MainHud").GetComponent<MainHudController>();
        healthProvider = new (maxHealth);
        StartCoroutine(SuicideOnOutOfBounds());
        StartCoroutine(ChaseNearestTargetCoroutine());
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (nearestPlayer != null && isDying != true) 
        {
            transform.LookAt(new Vector3(nearestPlayer.transform.position.x, transform.position.y, nearestPlayer.transform.position.z));
            // move towards player
            transform.position = Vector3.MoveTowards(transform.position, nearestPlayer.transform.position, speedDeltaToPlayer * Time.deltaTime);
        }
    }

    void ChaseNearestTarget()
    {
        nearestPlayer = GameObject.FindGameObjectsWithTag("Player")
            .OrderBy(player => (player.transform.position - transform.position).sqrMagnitude)
            .FirstOrDefault();
    }

    private IEnumerator ChaseNearestTargetCoroutine()
    {
        while (true)
        {
            ChaseNearestTarget();
            yield return new WaitForSeconds(chaseTargetCooldownSeconds);
        }
    }

    // WARN: Stay preserves the contact but it could be heavy on the system
    void OnCollisionStay(Collision collision)
    {
        Attack(collision);
    }

    private void Attack(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            animator.SetTrigger("Hitting");
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(1);
        }

        if (collision.gameObject.TryGetComponent<IDamagable>(out IDamagable damagable))
        {
            animator.SetTrigger("Hitting");
            damagable.TakeDamage(1);
        }
    }

    public void TakeDamage(float damage)
    {
        animator.SetTrigger("Shot");
        healthProvider.TakeDamage(damage);
	    //animator.ResetTrigger("Shot");
        StartCoroutine(temporaryInvulnerability());
        if (healthProvider.IsDead())
        {
            OnDeath();
        }
    }

    public void TakeKnockback(float knockbackForce)
    {
        // Apply knockback
        Vector3 knockbackDirection = (transform.position - nearestPlayer.transform.position).normalized; 
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
    }

    public IEnumerator temporaryInvulnerability(){
        myCollider.enabled = false;
        yield return new WaitForSeconds(0.01f);
        myCollider.enabled = true;
    }

    private void OnDeath()
    {
        //animator.SetTrigger("Shot");
        animator.SetTrigger("Died");
        mainHudController.AddScore(scoreGivenOnDeath);
        GameObject.FindWithTag("Player").GetComponent<PlayerController>().GainXp(xpGivenOnDeath);
        StartCoroutine(DelayedSuicide());
    }

    private IEnumerator SuicideOnOutOfBounds()
    {
        for ( ; ; )
        {
            if (transform.position.y < SUICIDE_Y)
            {
                StartCoroutine(DelayedSuicide()); // Start the coroutine
            }
            yield return new WaitForSeconds(OUT_OF_BOUNDS_CHECK_SECONDS);
        }
    }

    private IEnumerator DelayedSuicide() 
    {
        isDying = true;
        Destroy(GetComponent<Collider>());
        GetComponent<Rigidbody>().useGravity = false; 
        
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}
