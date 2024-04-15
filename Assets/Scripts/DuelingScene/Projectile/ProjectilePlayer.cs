using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlayer : ProjectileAbstract {
    [SerializeField] private float projectileSpeed = 3f;
    [SerializeField] private float delayFire = 1f;
    [SerializeField] private float deleteProjectile = 3f;
    private bool charging = false;
    
    
    private Object effect;
    
    private void Start() {
        effect = Resources.Load<Object>("ParticleExplosionBlue");
        StartCoroutine(Charging());
    }

    public override void fireProjectile() {
        // Charging - Keep the projectile stuck to the wand
        if (charging) return;

        // Detach projectile from wand
        this.transform.SetParent(null);

        // Let the projectile travel forward
        StartCoroutine(Travel());
    }

    public override void ProjectileHit(GameObject hitObject) {
        base.ProjectileHit(hitObject);

        // !charging prevent player from accidentally destroying projectile 
        // during charge state when hitting it against the floor
        if (!charging && hitObject.tag.Equals("Environment")) {
           DestroyProjectile();
       }
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

    IEnumerator Charging() {
        charging = true;
        yield return new WaitForSeconds(delayFire);
        charging = false;
    }

    IEnumerator Travel() {
        float currentTime = 0;

        while (currentTime < deleteProjectile) {
            // Update elapsed time
            currentTime += Time.deltaTime;

            // Move projectile forward
            transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);

            yield return null;
        }

        // Projectile expires
        Destroy(gameObject);
    }
}
