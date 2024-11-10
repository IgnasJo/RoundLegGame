using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class UziController : MonoBehaviour
{
    private FireLine fireLine;
    public AudioSource audioSource;
    private Vector3 initalForward;
	[SerializeField] public float muzzleFlashDuration = 0.1f;
    [SerializeField] private float shotCooldownSeconds = 0.03f;
    [SerializeField] private int penetration = 2;
    [SerializeField] private float knockbackForce = 50f;
    [SerializeField] private float baseDamage = 1f;

    private float lastShotTime = 0.0f;
    public VisualEffect muzzleFlash;
	public Light muzzlePointFlashLight;
    public Light muzzleDirectionalFlashLight;

    void Start()
    {
        fireLine = GetComponentInChildren<FireLine>();

        muzzlePointFlashLight.enabled = false;
        muzzleDirectionalFlashLight.enabled = false;
    	initalForward = transform.forward;
    }

    public void Fire()
    {
        float lastShotDifference = Time.time - lastShotTime;
		bool gunCooledDown = lastShotDifference >= shotCooldownSeconds;
		if (!gunCooledDown)
		{
			return;
		}
		
		muzzlePointFlashLight.enabled = true;
        muzzleDirectionalFlashLight.enabled = true;
		Invoke(nameof(DisableMuzzleFlashLight), muzzleFlashDuration); 
		muzzleFlash.Play();
        audioSource.Play();
		lastShotTime = Time.time;

        fireLine.Fire(penetration, knockbackForce, baseDamage);
    }

    void DisableMuzzleFlashLight()
    {
        muzzlePointFlashLight.enabled = false;
        muzzleDirectionalFlashLight.enabled = false;
    }
}
