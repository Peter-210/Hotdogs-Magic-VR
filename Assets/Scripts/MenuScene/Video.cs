using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class Video : MonoBehaviour {
    private VideoPlayer videoPlayer;

    void Start() {
        gameObject.AddComponent<VideoPlayer>();
        videoPlayer = FindObjectOfType<VideoPlayer>();

        videoPlayer.clip = Resources.Load<VideoClip>("demoVideo");
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.Prepare();
    }
}
