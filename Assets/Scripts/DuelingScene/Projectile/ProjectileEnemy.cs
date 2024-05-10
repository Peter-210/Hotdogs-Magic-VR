using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ProjectileEnemy : ProjectileAbstract {
    [SerializeField] private float horizontalNoise = 4f;
    [SerializeField] private float verticalNoise = 2f;

    protected override void Init() {
        projectileSpeed = 0.15f;
        deleteProjectile = 20f;
        particlePath = "ParticleExplosionRed";
    }

    protected override void Start() {
        base.Start();
        boundingBox.isTrigger = true;
        AimProjectile();
    }

    public override void ProjectileHit(GameObject hitObject) {
        base.ProjectileHit(hitObject);

        PotionPlayer potion = hitObject.GetComponent<PotionPlayer>();
        if (potion != null) DestroyProjectile();

        if (hitObject.tag.Equals("Environment")) {
           DestroyProjectile();
       }
    }

    public override void DestroyProjectile() {
        GameObject particle = Instantiate(
            effect, 
            gameObject.transform.position, 
            Quaternion.identity
        ) as GameObject;
        
        particle.AddComponent<ProjectileParticle>();
        SpatialAudio audio = particle.AddComponent<SpatialAudio>();
        audio.setSound("soft_explosion", false);

        Destroy(gameObject);
    }
    
    private void AimProjectile() {
        GameObject playerObject = GameObject.Find("Camera");
        GameObject rigObject = GameObject.Find("[CameraRig]");

        //// Calculating Horizontal Angle ////

        // Pythagorean Theorem
        
        // Opposite
        float enemyToPlayerZ = 
            Mathf.Abs(gameObject.transform.position.z - playerObject.transform.position.z);

        // Adjacent
        float enemyToPlayerX = 
            Mathf.Abs(playerObject.transform.position.x - gameObject.transform.position.x);

        // Hypotenuse
        float enemyToPlayerHorizontal = 
            Mathf.Sqrt(Mathf.Pow(enemyToPlayerZ, 2f) + Mathf.Pow(enemyToPlayerX, 2f));

        float horizontalAngle = Mathf.Rad2Deg * Mathf.Asin(enemyToPlayerZ/enemyToPlayerHorizontal);

        // Negative depending on if enemy is past player Z position
        if (playerObject.transform.position.z - gameObject.transform.position.z > 0) 
            horizontalAngle = -horizontalAngle;


        //// Calculating Vertical Angle ////

        // Pythagorean Theorem

        // Adjust to aim a bit above the player's center of mass
        float playerCenter = 
            Mathf.Abs(playerObject.transform.position.y - rigObject.transform.position.y) * 0.66f;

        // Add back the camera rig offset
        playerCenter = playerCenter + rigObject.transform.position.y;

        // Opposite
        float PlayerToProjectile = 
            Mathf.Abs(playerCenter - gameObject.transform.position.y);

        // Adjacent is already calculated from the horizontal angle calculation

        // Hypotenuse
        float enemyToPlayerVertical = 
            Mathf.Sqrt(Mathf.Pow(enemyToPlayerX, 2f) + Mathf.Pow(PlayerToProjectile, 2f));
        
        float verticalAngle = Mathf.Rad2Deg * Mathf.Asin(PlayerToProjectile/enemyToPlayerVertical);

        // Negative value when player center is above line of enemy fire
        if (playerCenter - gameObject.transform.position.y > 0) 
            verticalAngle = -verticalAngle;


        // Add noise to the angle data so that enemy is not 100% accurate
        float noiseHorizontal = Random.Range(0, horizontalNoise);
        if (Random.value < 0.5) noiseHorizontal = -noiseHorizontal; // Random horizontal angle (+/-)

        float noiseVertical = Random.Range(0, verticalNoise);
        if (Random.value < 0.5) noiseVertical = -noiseVertical; // Random vertical angle (+/-)

        horizontalAngle = horizontalAngle + noiseHorizontal;
        verticalAngle = verticalAngle + noiseVertical;

        // Adjust the trajectory of projectile
        gameObject.transform.Rotate(verticalAngle, horizontalAngle, 0);
    }
}
