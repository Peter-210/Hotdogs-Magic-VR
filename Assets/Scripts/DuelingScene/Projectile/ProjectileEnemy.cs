using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ProjectileEnemy : ProjectileAbstract {
    [SerializeField] private float horizontalNoise = 4f;
    [SerializeField] private float verticalNoise = 2f;
    [SerializeField] private float projectileSpeed = 0.3f;
    [SerializeField] private float deleteProjectile = 3f;

    private Object effect;
    void Start() {
        AimProjectile();
        effect = Resources.Load<Object>("ParticleExplosionRed");
        
    }

    public override void fireProjectile() {
        // Let the projectile travel forward
        StartCoroutine(Travel());
    }

    public override void ProjectileHit(GameObject hitObject) {
        base.ProjectileHit(hitObject);
        if (hitObject.tag.Equals("Environment")) {
           DestroyProjectile();
       }
    }

    IEnumerator Travel() {
        float currentTime = 0;

        while (currentTime < deleteProjectile) {
            // Update elapsed time
            currentTime += Time.deltaTime;

            
            //travelling "up" since relative rotation
            // Move projectile forward
            transform.Translate(Vector3.up * projectileSpeed * Time.deltaTime);

            yield return null;
        }

        // Projectile expires
        Destroy(gameObject);
    }


    public override void playExplosion()
    {
        StartCoroutine(particleTimer());
    }

    private IEnumerator particleTimer()
    {
        GameObject particles = Instantiate(effect, gameObject.transform.position, Quaternion.identity) as GameObject;
        yield return new WaitForSeconds(1f);
        Destroy(particles);
        
    }
    
    
    
    

    private void AimProjectile() {
        GameObject enemyObject = GameObject.Find("Enemy");
        GameObject playerObject = GameObject.Find("Camera");
        GameObject rigObject = GameObject.Find("[CameraRig]");

        //// Calculating Horizontal Angle ////

        float currentZPos = enemyObject.transform.position.z;

        // Pythagorean Theorem
        float enemyToPlayerZ = 
            Mathf.Abs(currentZPos - playerObject.transform.position.z);

        float enemyToPlayerX = 
            Mathf.Abs(playerObject.transform.position.x - enemyObject.transform.position.x);

        float enemyToPlayerHorizontal = 
            Mathf.Sqrt(Mathf.Pow(enemyToPlayerZ, 2f) + Mathf.Pow(enemyToPlayerX, 2f));

        float horizontalAngle = Mathf.Rad2Deg * Mathf.Asin(enemyToPlayerZ/enemyToPlayerHorizontal);

        // Negative depending on if enemy is past player Z position
        if (playerObject.transform.position.z - currentZPos > 0) horizontalAngle = -horizontalAngle;


        //// Calculating Vertical Angle ////

        // Pythagorean Theorem

        // Adjust to aim a bit above the player's center of mass
        float playerCenter = 
            Mathf.Abs(playerObject.transform.position.y - rigObject.transform.position.y) * 0.75f;

        // Add back the camera rig offset
        playerCenter = playerCenter + rigObject.transform.position.y;

        float PlayerToProjectile = 
            Mathf.Abs(playerCenter - gameObject.transform.position.y);

        float enemyToPlayerVertical = 
            Mathf.Sqrt(Mathf.Pow(enemyToPlayerX, 2f) + Mathf.Pow(PlayerToProjectile, 2f));
        
        float verticalAngle = -1 * Mathf.Rad2Deg * Mathf.Asin(PlayerToProjectile/enemyToPlayerVertical);

        // Negative depending on if player center is below leveled line of enemy fire
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
        gameObject.transform.Rotate(verticalAngle, 0, horizontalAngle, Space.Self);
    }
}
