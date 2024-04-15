using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialAudio : MonoBehaviour {
    private string audioSourceName = "projectileAudio";
    private AudioSource audioSource;

    private Transform player;
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private float volume = 1f;

    private void Start() {
        player = GameObject.Find("Camera").transform;
        audioSource = gameObject.AddComponent<AudioSource>();

        // General Settings for Audio
        audioSource.playOnAwake = false;
        audioSource.loop = true;

        // Get the audio needed to attach to gameObject
        AudioClip audioClip = Resources.Load<AudioClip>(audioSourceName);
        if (audioClip != null) {
            audioSource.clip = audioClip;
        } else {
            Debug.LogError("Missing audio clip for spatial sound.");
        }
    }

    private void Update() {
        float currentDistance = Vector3.Distance(gameObject.transform.position, player.position);
        
        // If the object is too far away from the player to hear
        if (currentDistance > maxDistance) {
            if (audioSource.isPlaying) audioSource.Stop();
            return;
        }

        // Adjust the volume of the audio
        audioSource.volume = Mathf.Lerp(0, volume, 1f - (currentDistance / maxDistance));

        // Adjust the pan of audio (Balance between left and right ears)
        float relativeXPos = player.position.x - gameObject.transform.position.x;

        if (relativeXPos > 0) {
            // Object is on left side of player
            audioSource.panStereo = Mathf.Lerp(0, -1f, Mathf.Abs(relativeXPos) / maxDistance);
        } else {
            // Object is on the right side of player
            audioSource.panStereo = Mathf.Lerp(0, 1f, Mathf.Abs(relativeXPos) / maxDistance);
        }

        if (!audioSource.isPlaying) audioSource.Play();
    }
}
