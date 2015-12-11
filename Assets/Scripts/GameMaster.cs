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

	void Start() 
    {
	
	}

    IEnumerator _RespawnPlayer()
    {
        yield return new WaitForSeconds(spawnDelay);

        Instantiate(m_PlayerPrefab, spawnPoint.position, spawnPoint.rotation);
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
