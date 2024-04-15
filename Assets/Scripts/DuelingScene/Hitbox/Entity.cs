using UnityEngine;

namespace DuelingScene.Entity
{
    public interface Entity
    {
        public abstract bool damageEntity(GameObject damageSource);
        public abstract void die();
        
       static void FadePlayer() {
            // Fade out back to main menu
            TransistionScene transition = GameObject.Find("[CameraRig]").GetComponent<TransistionScene>();
            transition.fadeOutToScene(3f, "MenuScene");
        }
    }
}