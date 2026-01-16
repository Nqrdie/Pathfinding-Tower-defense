using UnityEngine;

[System.Serializable]
public class Wavegenerator
{
    [Header("Wave Generation Settings")]
    public int baseEnemies = 3;
    public int enemiesPerWave = 2;
    public float healthPerWave = 0.10f;
    public float speedPerWave = 0.05f;
    public int enemyCountVariance = 1;

    public struct WaveData
    {
        public int enemyCount;
        public float healthMultiplier;
        public float speedMultiplier;
        public bool isValid => enemyCount > 0;
    }

    public WaveData GetWave(int waveNumber)
    {
        if (waveNumber < 1) waveNumber = 1;

        int count = baseEnemies + (waveNumber - 1) * enemiesPerWave;
        if (enemyCountVariance > 0)
            count += Random.Range(-enemyCountVariance, enemyCountVariance + 1);
        count = Mathf.Max(1, count);

        float healthMulti = 1f + (waveNumber - 1) * healthPerWave;
        float speedMulti = 1f + (waveNumber - 1) * speedPerWave;

        return new WaveData
        {
            enemyCount = count,
            healthMultiplier = healthMulti,
            speedMultiplier = speedMulti
        };
    }
}