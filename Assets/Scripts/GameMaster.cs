using UnityEngine;
using System.Collections;

public class GameMaster : MonoBehaviour {

    [HideInInspector]
    public static GameMaster gm;

    void Awake()
    {
        gm = this;
    }

    [SerializeField] private GameObject m_PlayerPrefab;

    public Transform m_Player;
    public Transform spawnPoint;
    public float spawnDelay = 2f;
    public Transform spawnPrefab;

    public CameraShake camShake;

	void Start() 
    {
        if (camShake == null)
        {
            camShake = GetComponent<CameraShake>();
            if (camShake == null)
            {
                Debug.LogError("No CameraShake component on _GameMaster");
            }
        }
	}

    IEnumerator _RespawnPlayer()
    {
        yield return new WaitForSeconds(spawnDelay);
        Transform clonePlayer = Instantiate(m_PlayerPrefab, spawnPoint.position, spawnPoint.rotation) as Transform;

        m_Player.GetComponent<P2D_Motor>().Reset();

        GameObject cloneEffect = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
        Destroy(cloneEffect, 3f);
    }

    public static void KillPlayer(Player player)
    {
        Destroy(player.gameObject);
        gm.StartCoroutine(gm._RespawnPlayer());
    }

    public static void KillEnemy(Enemy enemy)
    {
        gm._KillEnemy(enemy);
    }

    void _KillEnemy(Enemy _enemy)
    {
        Destroy(_enemy.gameObject);
    }
}
