using System.Collections;
using System.Collections.Generic;
using DuelingScene.Entity;
using UnityEngine;

public class PlayerObject : MonoBehaviour, Entity {
    private BoxCollider boxCollider;
    private IDamage Damage;

    //this is fine. Think of it as a constructor
    
    private void Start() {
        // Create hands as rigidbody
        gameObject.AddComponent<Rigidbody>();
        Rigidbody enemyRigid = FindObjectOfType<Rigidbody>();
        enemyRigid.useGravity = false;
        enemyRigid.isKinematic = true;
        
    }

    private void Update() {
        UpdatePlayerHitbox();
    }

    
    
    //@Override from Entity
    public bool damageEntity(GameObject damageSource)
    {
        ProjectilePlayer playerProj = damageSource.GetComponent<ProjectilePlayer>();
        if (playerProj == null)
        {
            Player1.health--;
            if (Player1.health <= 0 && Game.startGame)
            {
                Game.startGame = false;
                Entity.FadePlayer();
            }

            return true;
        }

        return false;
    }

    //the hitboxes of player/enemy are already triggers...

    
    //
    // //this is put on the player
    // //so this is for player damaging
    // //instead of using this how about we have the projectiles as triggers instead???
    // public void OnTriggerEnter(Collider collision) {
    //     //not sure why damage is an instance variable
    //     
    //     Damage = collision.GetComponent<IDamage>();
    //     
    //     if (Damage != null)
    //         Damage.Hit(gameObject);
    // }
    

    private void UpdatePlayerHitbox() {
        float boxCollierY = gameObject.transform.position.y;
        float CameraRigY = Mathf.Abs(GameObject.Find("[CameraRig]").transform.position.y);

        boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null) {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        float midpoint = -(boxCollierY + CameraRigY)/2f ;
        boxCollider.center = new Vector3(0, midpoint, 0);
        boxCollider.size = new Vector3(0.69f, boxCollierY + CameraRigY, 0.55f);
        boxCollider.isTrigger = true;
    }
    
    
    // public void FadePlayer() {
    //     // Fade out back to main menu
    //     TransistionScene transition = GameObject.Find("[CameraRig]").GetComponent<TransistionScene>();
    //     transition.fadeOutToScene(3f, "MenuScene");
    // }


    public void die()
    {
        // Disable projectile damage
        BoxCollider enemyCollider = GameObject.Find("Enemy").GetComponent<BoxCollider>();
        enemyCollider.isTrigger = false;

        BoxCollider playerCollider = GameObject.Find("Camera").GetComponent<BoxCollider>();
        playerCollider.isTrigger = false;

        // Fade out back to main menu
        TransistionScene transition = GameObject.Find("[CameraRig]").GetComponent<TransistionScene>();
        transition.fadeOutToScene(3f, "MenuScene");
    }
    
}
