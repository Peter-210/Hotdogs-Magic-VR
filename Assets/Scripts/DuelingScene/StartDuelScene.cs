using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

// using UnityEngine.transform;

public class StartDuelScene : MonoBehaviour {
    [SerializeField] private string musicPath = "duelingSong";
    [SerializeField] private float volume = 0.2f;
    [SerializeField] private float fadeInDuration = 1.0f;
    
    private void Awake() {
        spawnCrates();
        initSpawnWand();
        initPlayer();
        initEnemy();
        startScene();
        Game.startGame = true;
    }

    private void spawnCrates()
    {
        int crates = Game.crates;
        Object o = Resources.Load<Object>("crate");
        
        
        for (int spawned = 0; spawned < crates; spawned++)
        {
            Vector3 pos = new Vector3(5f, Random.Range(0f, 3f), Random.Range(-3f, 3f));
            GameObject crate = Instantiate(o, pos, Quaternion.identity) as GameObject;

            crate.AddComponent<Rigidbody>();
            crate.AddComponent<BoxCollider>();
            BoxCollider collider = crate.GetComponent<BoxCollider>();
            collider.isTrigger = true;

            crate.AddComponent<BoxCollider>();
            crate.tag = "Crate";
            crate.AddComponent<CrateObject>();

        }
    }

    private void initSpawnWand() {
        GameObject leftController;
        GameObject rightController;

        if (Player1.DominantSide.Equals("left")) {
            leftController = GameObject.Find("Controller (left)");
            leftController.AddComponent<SpawnWand>();
        } else if (Player1.DominantSide.Equals("right")) {
            rightController = GameObject.Find("Controller (right)");
            rightController.AddComponent<SpawnWand>();
        } else {
            Debug.LogError("Failed to initialize DominantSide variable from PlayerData.");
        }
    }

    private void initPlayer() {
        GameObject playerObject = GameObject.Find("Camera");
        playerObject.AddComponent<PlayerObject>();
    }

    private void initEnemy() {
        GameObject enemyObject = GameObject.Find("Enemy");

        // Make sure the spawn point of enemy aligns with player
        Vector3 enemyPosition = enemyObject.transform.position;
        enemyPosition.z = GameObject.Find("[CameraRig]").transform.position.z;
        enemyObject.transform.position = enemyPosition;

        // Save position of the enemy spawnpoint
        Player2.spawnPoint = enemyPosition;

        // Add AI Logic Component
        enemyObject.AddComponent<EnemyStateManager>();
    }

    
    private void startScene() {
        // Background Dueling Music
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = Resources.Load<AudioClip>(musicPath);
        audioSource.volume = volume;
        audioSource.Play();

        // Fade In Player View
        gameObject.AddComponent<TransistionScene>();
        TransistionScene transition = FindObjectOfType<TransistionScene>();
        transition.fadeInView(fadeInDuration);
    }
}
