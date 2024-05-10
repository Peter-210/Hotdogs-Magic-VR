using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePlayer : ProjectileAbstract {
    [SerializeField] private float delayFire = 1f;
    private bool charging = false;

    protected override void Init() {
        projectileSpeed = 3f;
        deleteProjectile = 3f;
        particlePath = "ParticleExplosionBlue";
    }
    
    protected override void Start() {
        base.Start();
        StartCoroutine(Charging());
    }

    protected override void fireProjectile() {
        // Charging - Keep the projectile stuck to the wand
        if (charging) return;

        // Detach projectile from wand
        this.transform.SetParent(null);

        base.fireProjectile();
    }

    public override void ProjectileHit(GameObject hitObject) {
        base.ProjectileHit(hitObject);

        PotionEnemy potion = hitObject.GetComponent<PotionEnemy>();
        if (potion != null) DestroyProjectile();

        // !charging prevent player from accidentally destroying projectile 
        // during charge state when hitting it against the floor
        if (!charging && hitObject.tag.Equals("Environment")) {
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
        audio.setSound("soft_explosion",false);
        
        Destroy(gameObject);
    }

    IEnumerator Charging() {
        charging = true;
        yield return new WaitForSeconds(delayFire);
        charging = false;
    }
}
