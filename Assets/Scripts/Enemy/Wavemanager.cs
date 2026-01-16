using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wavemanager : MonoBehaviour
{
    [Header("Prefabs & Spawning")]
    [SerializeField] private List<GameObject> enemyPrefabs = new List<GameObject>();
    [SerializeField] private Transform[] spawnPoints = Array.Empty<Transform>();
    [SerializeField] private float defaultSpawnInterval = 0.5f;

    [Header("Wave Settings")]
    [SerializeField] private Wavegenerator waveGenerator = new Wavegenerator();
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool loopWaves = false;

    public List<GameObject> aliveEnemies = new List<GameObject>();
    private int currentWaveIndex = -1;
    private bool spawning = false;

    public event Action<int> OnWaveStarted;
    public event Action<int> OnWaveCompleted;

    private void Start()
    {
        if (autoStart)
            StartCoroutine(WaveController());
    }

    private IEnumerator WaveController()
    {
        yield return new WaitForSeconds(1f);

        do
        {
            while (HasAliveEnemies())
                yield return null;

            currentWaveIndex++;
            if (waves.Count > 0)
            {
                if (currentWaveIndex >= waves.Count)
                {
                    if (loopWaves) currentWaveIndex = 0;
                    else break;
                }

                var wave = waves[currentWaveIndex];
                OnWaveStarted?.Invoke(currentWaveIndex + 1);
                yield return StartCoroutine(SpawnWaveDefinition(wave));
                while (HasAliveEnemies())
                    yield return null;
                OnWaveCompleted?.Invoke(currentWaveIndex + 1);
                yield return new WaitForSeconds(wave.delayAfter);
            }
            else
            {
                int waveNumber = currentWaveIndex + 1;
                var data = waveGenerator.GetWave(waveNumber);
                OnWaveStarted?.Invoke(waveNumber);
                yield return StartCoroutine(SpawnProceduralWave(data));
                while (HasAliveEnemies())
                    yield return null;
                OnWaveCompleted?.Invoke(waveNumber);
                yield return new WaitForSeconds(1f);
            }
        } while (loopWaves || currentWaveIndex < (waves.Count - 1));

        spawning = false;
    }

    private bool HasAliveEnemies()
    {
        aliveEnemies.RemoveAll(e => e == null);
        return aliveEnemies.Count > 0 || spawning;
    }

   
    private IEnumerator SpawnWaveDefinition(Wave wave)
    {
        spawning = true;

        if (wave.delayBefore > 0f)
            yield return new WaitForSeconds(wave.delayBefore);

        for (int g = 0; g < wave.groups.Count; g++)
        {
            var group = wave.groups[g];
            if (group.prefab == null)
                continue;

            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.prefab, wave.healthMultiplier, wave.speedMultiplier);
                yield return new WaitForSeconds(Mathf.Max(0.01f, group.spawnRate));
            }

            if (wave.groupSpacing > 0f)
                yield return new WaitForSeconds(wave.groupSpacing);
        }

        spawning = false;
    }
    
    private IEnumerator SpawnProceduralWave(Wavegenerator.WaveData data)
    {
        spawning = true;

        int toSpawn = data.enemyCount;
        for (int i = 0; i < toSpawn; i++)
        {
            if (enemyPrefabs.Count == 0 || spawnPoints.Length == 0)
                break;

            var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            SpawnEnemy(prefab, data.healthMultiplier, data.speedMultiplier);
            yield return new WaitForSeconds(defaultSpawnInterval);
        }

        spawning = false;
    }

    private void SpawnEnemy(GameObject prefab, float healthMult, float speedMult)
    {
        if (prefab == null || spawnPoints.Length == 0)
            return;

        var spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        var go = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, null);
        aliveEnemies.Add(go);

        var multipliers = new Vector2(healthMult, speedMult);
        go.SendMessage("ApplyMultipliers", multipliers, SendMessageOptions.DontRequireReceiver);
    }

    public void StartWaves()
    {
        StopAllCoroutines();
        currentWaveIndex = -1;
        StartCoroutine(WaveController());
    }

    public void StopWaves()
    {
        StopAllCoroutines();
        spawning = false;
    }

    [Serializable]
    public class Wave
    {
        public string waveName;
        [Tooltip("Groups of enemies in this wave")]
        public List<EnemyGroup> groups = new List<EnemyGroup>();
        [Tooltip("Delay before this wave starts")]
        public float delayBefore = 0.5f;
        [Tooltip("Delay after the wave completes before next wave begins")]
        public float delayAfter = 1f;
        [Tooltip("Spacing between groups inside the wave")]
        public float groupSpacing = 0.5f;
        [Tooltip("Multipliers applied to all spawned enemies in this wave")]
        public float healthMultiplier = 1f;
        public float speedMultiplier = 1f;
        public bool isBoss = false;
    }

    [Serializable]
    public class EnemyGroup
    {
        public GameObject prefab;
        [Min(1)]
        public int count = 1;
        [Tooltip("Seconds between spawns inside this group")]
        public float spawnRate = 0.3f;
    }
}