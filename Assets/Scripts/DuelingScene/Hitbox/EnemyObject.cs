using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DuelingScene.Entity;

/// <summary>
/// this is for handling enemy damage, in which case it may as well handle the entire entity
/// </summary>
public class EnemyObject : MonoBehaviour, Entity {
    private Rigidbody enemyRigid;
    private BoxCollider collisionBox;
    [SerializeField] private float enemyFaceplantForce = 2f;

    private IDamage Damage;

    private void Awake() {
        addComponents();
    }

    // private void OnTriggerEnter(Collider collision) {
    //     //we first gotta check if it is a wall
    //     
    //     
    //     Damage = collision.GetComponent<IDamage>();
    //     Damage.Hit(gameObject);
    // }


    public bool damageEntity(GameObject damageSource)
    {
        ProjectileEnemy enemy = damageSource.GetComponent<ProjectileEnemy>();
        //ensure that the projectile is not from self
        if (enemy == null)
        {

            Player2.health--;
            if (Player2.health <= 0 && Game.startGame)
            {
                Game.startGame = false;
                this.die();

                Entity.FadePlayer(); //kinda stupid but we need it to be static cause I don't think this is C# 8.0 - Talon
                return true;
            }
        }

        return false;
    }
    
    
    

    private void addComponents() {
        // Create hands as rigidbody
        gameObject.AddComponent<Rigidbody>();
        Rigidbody enemyRigid = FindObjectOfType<Rigidbody>();
        enemyRigid.useGravity = false;
        enemyRigid.isKinematic = true;

        // Add collision box to hands
        gameObject.AddComponent<BoxCollider>();
        BoxCollider collisionBox = FindObjectOfType<BoxCollider>();
        collisionBox.isTrigger = true;
        collisionBox.center = new Vector3(0f, 0f, 0.0015f);
        collisionBox.size = new Vector3(0.005f, 0.005f, 0.02f);
    }
    
    
    
    //@Override from entity
    public void die() {
        // Disable projectile damage
        BoxCollider enemyCollider = GameObject.Find("Enemy").GetComponent<BoxCollider>();
        enemyCollider.isTrigger = false;

        BoxCollider playerCollider = GameObject.Find("Camera").GetComponent<BoxCollider>();
        playerCollider.isTrigger = false;

        // Make the enemy fall down
        Rigidbody enemyRigid = gameObject.GetComponent<Rigidbody>();
        enemyRigid.useGravity = true;
        enemyRigid.isKinematic = false;
        enemyRigid.AddForce(Vector3.right * enemyFaceplantForce, ForceMode.Impulse);

        // Fade back to main menu
        BoxCollider boxCollider = GameObject.Find("Camera").GetComponent<BoxCollider>();
        boxCollider.isTrigger = false;
    }
    
    
    
    
}
