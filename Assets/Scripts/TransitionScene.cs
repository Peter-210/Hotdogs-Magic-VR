using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class TransistionScene : MonoBehaviour {
    public void fadeInView(float fadeOutDuration) {
        StartCoroutine(fadeTransistion(fadeOutDuration, null));
    }

    public void fadeOutToScene(float fadeOutDuration, string sceneName) {
        StartCoroutine(fadeTransistion(fadeOutDuration, sceneName));
    }

    private IEnumerator fadeTransistion(float fadeOutDuration, string sceneName) {
        bool stayInFade = false;

        // Set initial time with fade color and alpha
        float currentTime = 0;
        Color currentColor = Color.black;
        float alphaStart, alphaEnd;
        
        if (sceneName != null) {
            alphaStart = 0; alphaEnd = 1f;
        } else {
            alphaStart = 1f; alphaEnd = 0;
        }

        // Set color to match alpha setting for fade in/out
        currentColor.a = alphaStart;

        // Gradually fade in/out player view
        float fractionfadeOutDuration = fadeOutDuration*2/3;
        while (currentTime < fadeOutDuration) {
            // Fade to black then linger in black screen (fractionfadeOutDuration)
            if (currentTime < fractionfadeOutDuration) {
                // Change alpha of color for fade view
                currentColor.a = Mathf.Lerp(
                    alphaStart, alphaEnd, currentTime/fractionfadeOutDuration
                );
                SteamVR_Fade.View(currentColor, 0); 
            } else if (!stayInFade) {
                // Set fade alpha value to be 100% of alphaEnd
                stayInFade = true;
                currentColor.a = alphaEnd;
                SteamVR_Fade.View(currentColor, 0);
            }

            // Update elapsed time
            currentTime += Time.deltaTime;
            yield return null;
        }

        // Load to the new scene
        if (sceneName != null) {
            SceneManager.LoadScene(sceneName);
        }
    }
}
