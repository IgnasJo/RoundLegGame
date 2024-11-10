using System.Collections;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    [SerializeField] private GameObject zombiePrefab;
    [SerializeField] private GameObject monster1Prefab;
    [SerializeField] private GameObject player;
    [SerializeField] private float spawnTimeSeconds = 1f;
    private Vector3 worldViewFromPlayerBounds = new (20f, 0, 20f);
    private Transform floorTransform;
    private Vector3 randomSpawnPositionBounds = new (10f, 2f, 10f);
    private int maxZombiesAtOneTime = 20;

    private SceneFadeController sceneFadeController;

    private MainHudController mainHudController;

    private int waveTime = 0;
    private readonly int waveEndTime = 60 * 10;

    void Awake()
    {
        mainHudController = GameObject.Find("MainHud").GetComponent<MainHudController>();
        sceneFadeController = GameObject.Find("SceneFade").GetComponent<SceneFadeController>();
        StartCoroutine(CountdownTimeCoroutine());
    }

    void Start()
    {
        sceneFadeController.FadeIn(() =>
        {
            StartCoroutine(SpawnZombiesCoroutine());
        });  
    }

    private IEnumerator SpawnZombiesCoroutine()
    {
        for ( ; ; )
        {
            yield return new WaitForSeconds(spawnTimeSeconds);
            if (player == null) break;
            if (transform.childCount >= maxZombiesAtOneTime) continue;
            float randomX = Random.Range(-randomSpawnPositionBounds.x, randomSpawnPositionBounds.x);
            float randomZ = Random.Range(-randomSpawnPositionBounds.z, randomSpawnPositionBounds.z);
            Vector3 randomPosition = new (
                randomX < 0
                    ? player.transform.position.x + randomX - worldViewFromPlayerBounds.x
                    : player.transform.position.x + randomX + worldViewFromPlayerBounds.x,
                randomSpawnPositionBounds.y,
                randomZ < 0
                    ? player.transform.position.z + randomZ - worldViewFromPlayerBounds.z
                    : player.transform.position.z + randomZ + worldViewFromPlayerBounds.z
            );
            // attach to GameSystem so it's organised
            Instantiate(zombiePrefab, randomPosition, Quaternion.identity, transform);
            if (transform.childCount % 7 == 0){
                Instantiate(monster1Prefab, randomPosition, Quaternion.identity, transform);
            }
        }
    }

    private IEnumerator CountdownTimeCoroutine()
    {
        while (waveTime < waveEndTime)
        {
            yield return new WaitForSeconds(1f);
            waveTime += 1;
            mainHudController.SetWaveTime(waveTime);
        }
    }
}
