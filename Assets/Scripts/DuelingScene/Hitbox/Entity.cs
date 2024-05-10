using UnityEngine;
using Valve.VR;

namespace DuelingScene.Entity
{
    public interface Entity
    {
        public abstract bool damageEntity(GameObject damageSource);
        public abstract void die();
        
       static void FadePlayer() {
            
            //reset hand positions so that they're not clenching when they fade back
            SteamVR_Behaviour_Skeleton.lockLeftClench = false;
            SteamVR_Behaviour_Skeleton.lockRightClench = false;
            
            // Fade out back to main menu
            TransistionScene transition = GameObject.Find("[CameraRig]").GetComponent<TransistionScene>();
            transition.fadeOutToScene(3f, "MenuScene");
        }
    }
}