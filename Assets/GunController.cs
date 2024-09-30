using System.Collections;
using UnityEngine;

public class GunController : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20.0f;
    [SerializeField] private float shotCooldownSeconds = 0.5f;
    [SerializeField] private float bulletDisposeSeconds = 2.0f;
    private float lastShotTime = 0.0f;

    public void Fire()
    {
        float lastShotDifference = Time.time - lastShotTime;
        bool gunCooledDown = lastShotDifference >= shotCooldownSeconds;
        if (gunCooledDown)
        {
            GameObject bulletInstance = Instantiate(bulletPrefab, transform.position + transform.up, transform.rotation);
            bulletInstance.GetComponent<Rigidbody>().AddForce(transform.up * bulletSpeed, ForceMode.Impulse);
            lastShotTime = Time.time;
            StartCoroutine(DisposeBulletCoroutine(bulletInstance));
        }
    }

    private IEnumerator DisposeBulletCoroutine(GameObject bulletInstance)
    {
        yield return new WaitForSeconds(bulletDisposeSeconds);
        Destroy(bulletInstance);
    }
}
