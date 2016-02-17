using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameMaster : MonoBehaviour {

    [HideInInspector]
    public static GameMaster gm;

    void Awake()
    {
        gm = this;
    }

    public bool HasGameStarted = false;

    [SerializeField] private GameObject m_PlayerPrefab;

    public Transform m_Player;
    public Transform spawnPoint;
    public float spawnDelay = 2f;
    public Transform spawnPrefab;

    public float mobSpawnDelay = 20f;
    float lastSpawn;

    public GameObject Enemy;

    public Transform mobSpawn1;
    public Transform mobSpawn2;
    public Transform mobSpawn3;
    public Transform mobSpawn4;

    public CameraShake camShake;

    public GameObject Hotbar;
    public GameObject PlayerInfo;
    public GameObject FPSCounter;
    public bool ShowFpsCounter;
    public GameObject MainMenu;
    bool isMenuActive = false;

    public GameObject LoadingScreen;

    MapGenerator mapGen;
    public int blocksPerChunkSide = 20;
    public int Offset;
    public List<List<GameObject>> chunks;

	void Start() 
    {
        chunks = new List<List<GameObject>>();

        if (camShake == null)
        {
            camShake = GetComponent<CameraShake>();
            if (camShake == null)
            {
                Debug.LogError("No CameraShake component on _GameMaster");
            }
        }

        MainMenu.GetComponent<Animator>().SetBool("Active", true);
	}

    public void StartGame()
    {
        if (!LoadingScreen.active)
        {
            LoadingScreen.SetActive(true);
            MainMenu.SetActive(false);
            Invoke("StartGame", .1f);
            return;
        }

        mapGen = GetComponent<MapGenerator>();
        mapGen.GenerateMap();
        ShowThyMap(mapGen.width, mapGen.height);

        PlaceSpawn(mobSpawn3);
        PlaceSpawn(mobSpawn4);

        mapGen.GenerateMap();
        MakeSureDONTDefyTheGravity();

        for (int i = 0; i < 5; i++)
            mapGen.SmoothMap();

        ShowThyMap(mapGen.width, mapGen.height, 0.5f, true);

        PlaceSpawn(spawnPoint);
        PlaceSpawn(mobSpawn1);
        PlaceSpawn(mobSpawn2);
        StartCoroutine(_RespawnPlayer());

        HasGameStarted = true;

        Invoke("MakeMenuInactive", .1f);
        isMenuActive = false;

        LoadingScreen.SetActive(false);
    }

    void SpawnMobs()
    {
        Instantiate(Enemy, mobSpawn1.position, mobSpawn1.rotation);
        Instantiate(Enemy, mobSpawn2.position, mobSpawn1.rotation);
        Instantiate(Enemy, mobSpawn3.position, mobSpawn1.rotation);
        Instantiate(Enemy, mobSpawn4.position, mobSpawn1.rotation);

        lastSpawn = Time.time + mobSpawnDelay;
    }

    void Update()
    {
        if (!HasGameStarted)
            return;

        if (Time.time > lastSpawn)
            SpawnMobs();

        if (Input.GetKeyDown(KeyCode.Escape))
            if (isMenuActive)
            {
                Time.timeScale = 1f;
                MainMenu.GetComponent<Animator>().SetBool("Active", false);
                Invoke("MakeMenuInactive", .1f);
                isMenuActive = false;
            }
            else
            {
                MainMenu.SetActive(true);
                MainMenu.GetComponent<Animator>().SetBool("Active", true);
                isMenuActive = true;

                MainMenu.GetComponent<MainMenuButtons>().GoToMainPage();
                MainMenu.transform.FindChild("Background").GetComponent<Canvas>().sortingOrder = -2;

                PlayerInfo.SetActive(false);
                Hotbar.SetActive(false);

                if (ShowFpsCounter)
                    FPSCounter.SetActive(false);

                Time.timeScale = 0f;
            }
/*
        foreach (List<GameObject> chunkList in chunks)
            foreach (GameObject chunk in chunkList)
            {
                if (chunk.transform.position.x + blocksPerChunkSide * 0.25f + Offset > Camera.main.transform.position.x && chunk.transform.position.x - Offset < Camera.main.transform.position.x &&
                    chunk.transform.position.y + blocksPerChunkSide * 0.25f + Offset > Camera.main.transform.position.y && chunk.transform.position.y - Offset < Camera.main.transform.position.y)
                    chunk.SetActive(true);
                else
                    chunk.SetActive(false);
            }*/
    }

    void MakeMenuInactive()
    {
        MainMenu.SetActive(false);

        PlayerInfo.SetActive(true);
        Hotbar.SetActive(true);

        if (ShowFpsCounter)
            FPSCounter.SetActive(true);
    }

    void PlaceSpawn(Transform spawn, bool negativeY = false)
    {
        int x = Random.Range(10, mapGen.width - 10);
        for (int y = 0; y < mapGen.height; y++)
        {
            if (mapGen.map[x, y] == 0)
            {
                if (!negativeY)
                    spawn.position = new Vector2(x / 4, (y + 8) / 4);
                else
                    spawn.position = new Vector2(x / 4, (-y + 8) / 4);

            }
        }
    }

    void ShowThyMap(int width, int height, float AddToHeight = 0, bool negative = false)
    {
        int numberofchunks = 0;

        List<GameObject> Tempchunks = new List<GameObject>();

        for (int i = 0; i < (width / blocksPerChunkSide) * (height / blocksPerChunkSide); i++)
        {
            GameObject chunk = new GameObject(numberofchunks.ToString());
            Tempchunks.Add(chunk);
            numberofchunks++;
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject clone;

                if (!negative)
                {   
                    clone = Instantiate(Resources.Load("Prefabs/" + mapGen.map[i, j].ToString(), typeof(GameObject)), new Vector3((float)i / 2, (float)-j / 2 + AddToHeight), Quaternion.identity) as GameObject;

                    if (i % blocksPerChunkSide == 0 && j % blocksPerChunkSide == 0)
                        Tempchunks[i / blocksPerChunkSide * (height / blocksPerChunkSide) + j / blocksPerChunkSide].transform.position = clone.transform.position;

                    clone.transform.SetParent(Tempchunks[i / blocksPerChunkSide * (height / blocksPerChunkSide) + j / blocksPerChunkSide].transform);
                }
                else
                {
                    if (mapGen.map[i, j] != 0)
                        mapGen.map[i, j] = 3;

                    clone = Instantiate(Resources.Load("Prefabs/" + mapGen.map[i, j].ToString(), typeof(GameObject)), new Vector3((float)i / 2, (float)j / 2 + AddToHeight), Quaternion.identity) as GameObject;

                    if (i % blocksPerChunkSide == 0 && j % blocksPerChunkSide == 0)
                        Tempchunks[i / blocksPerChunkSide * (height / blocksPerChunkSide) + j / blocksPerChunkSide].transform.position = clone.transform.position;

                    clone.transform.SetParent(Tempchunks[i / blocksPerChunkSide * (height / blocksPerChunkSide) + j / blocksPerChunkSide].transform);
                }
            }
        }

        chunks.Add(Tempchunks);
    }

    void MakeSureDONTDefyTheGravity()
    {
        for (int x = 0; x < mapGen.width; x++)
        {
            bool fly = false;
            for (int y = 0; y < mapGen.height; y++)
            {
                if (mapGen.map[x, y] == 0)
                    fly = true;

                if (fly == true)
                    mapGen.map[x, y] = 0;
            }
        }
    }

    IEnumerator _RespawnPlayer()
    {
        yield return new WaitForSeconds(spawnDelay);
        m_Player.position = spawnPoint.position;
        m_Player.GetComponent<P2D_Motor>().Reset();

        m_Player.gameObject.SetActive(true);
        Player.Instance.Start();

        GameObject cloneEffect = Instantiate(spawnPrefab, spawnPoint.position, spawnPoint.rotation) as GameObject;
        Destroy(cloneEffect, 3f);
    }

    public static void KillPlayer(Player player)
    {
        gm.m_Player.gameObject.SetActive(false);
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
