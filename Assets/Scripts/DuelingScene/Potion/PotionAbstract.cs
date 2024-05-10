using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DuelingScene.Entity;
using Object = UnityEngine.Object;

public abstract class PotionAbstract : MonoBehaviour, IDamage {
    [SerializeField] private float potionSpeed = 0.02f;
    [SerializeField] private float deletePotion = 30f;
    [SerializeField] protected float delayTime = 0.5f;

    protected bool delayPotionBool;
    [SerializeField] protected Vector3 rotateReset;

    [SerializeField] private Vector3 gameObjectSize = new Vector3(0.003f, 0.003f, 0.003f);
    
    public static event Action <GameObject> OnCollision;
    private BoxCollider potionCollider;

    private static Object particleExplosionEffect = null;
    
    private void Awake()
    {
        if (particleExplosionEffect == null) 
            particleExplosionEffect = Resources.Load<Object>("FX_Explosion_Blue");
        
        Init();
    }

    protected abstract void Init();

    protected virtual void Start() {

        potionCollider = gameObject.GetComponent<BoxCollider>();
        if (potionCollider == null)
            potionCollider = gameObject.AddComponent<BoxCollider>();

        potionCollider.size = gameObjectSize;
        potionCollider.isTrigger = true;

        delayPotionBool = true;
        StartCoroutine(DelayPotion());
    }

    private IEnumerator DelayPotion() {
        yield return new WaitForSeconds(delayTime);

        Rigidbody potionRigid = 
            gameObject.GetComponent<Rigidbody>();
        potionRigid.useGravity = false;
        potionRigid.isKinematic = true;

        // Reset orientation of the potion
        transform.rotation = Quaternion.identity;
        transform.Rotate(rotateReset.x, rotateReset.y, rotateReset.z);

        AimPotion();

        delayPotionBool = false;
    }

    private void Update() {
        if (delayPotionBool) return;
        StartCoroutine(FirePotion());
    }

    public void Hit(GameObject Player) {
        if (OnCollision == null) return;
        OnCollision.Invoke(Player);
    }

    private void OnEnable() {
        OnCollision += PotionHit;
    }

    private void OnDisable() {
        OnCollision -= PotionHit;
    }

    private IEnumerator FirePotion() {
        float currentTime = 0;

        while (currentTime < deletePotion) {
            currentTime += Time.deltaTime;

            // Move potion forward
            transform.Translate(Vector3.forward * potionSpeed * Time.deltaTime);

            yield return null;
        }

        // Potion expires
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other) {
        PotionHit(other.gameObject);
    }

    public void PotionHit(GameObject hitObject) {
        bool damage = false;

        Entity e = hitObject.GetComponent<Entity>();
        if (e != null) damage = e.damageEntity(gameObject);
       
        if (damage) DestroyPotion();
        if (
            hitObject.name.Equals("ProjectilePlayer") || 
            hitObject.name.Equals("ProjectileEnemy")
        ) DestroyPotion();
        
        if (hitObject.tag.Equals("Crate")) DestroyPotion();
        if (hitObject.tag.Equals("Environment")) DestroyPotion();


        if (damage)
            return;
        
        Collider[] boxColliders = Physics.OverlapSphere(gameObject.transform.position, GameDefault.explosionRadius);
        foreach (Collider collider in boxColliders)
        {
            GameObject splashed = collider.gameObject;
            Entity splashedEntity = splashed.GetComponent<Entity>();
            if (splashedEntity == null)
                continue;
            
            splashedEntity.damageEntity(gameObject);
            return;
        }

        
    }
    
    public void DestroyPotion()
    {
       GameObject particles = Instantiate(particleExplosionEffect, 
           gameObject.transform.position,
           Quaternion.identity) as GameObject;
       
       particles.AddComponent<ProjectileParticle>();

       
       SpatialAudio explode = particles.AddComponent<SpatialAudio>();
       explode.setSound("med_explosion", false);

       SpatialAudio glass = particles.AddComponent<SpatialAudio>();
       glass.setSound("glass_break",false);
       
       
        Destroy(gameObject);
    }

    
    public abstract void AimPotion();
}
