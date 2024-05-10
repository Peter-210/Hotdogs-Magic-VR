using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DuelingScene.Entity;

public abstract class ProjectileAbstract : MonoBehaviour, IDamage {
    public static event Action <GameObject> OnCollision;

    protected float projectileSpeed;
    protected float deleteProjectile;

    [SerializeField] protected Vector3 gameObjectSize = new Vector3(0.25f, 0.25f, 0.25f);
    protected Vector3 initialPos;
    protected BoxCollider boundingBox;
    
    protected UnityEngine.Object effect;
    protected string particlePath;

    private void Awake() {
        Init();
    }

    protected abstract void Init();

    protected virtual void Start() {
        effect = Resources.Load<UnityEngine.Object>(particlePath);
        boundingBox = gameObject.AddComponent<BoxCollider>();
    }

    private void Update() {
        fireProjectile();
    }

    protected virtual void fireProjectile() {
        // Let the projectile travel forward
        StartCoroutine(Travel());
    }

    private IEnumerator Travel() {
        float currentTime = 0;

        while (currentTime < deleteProjectile) {
            currentTime += Time.deltaTime;

            // Keep track of the previous position of projectile
            initialPos = transform.position;

            // Move projectile forward
            transform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);

            updateBoundingBox();

            yield return null;
        }

        // Projectile expires
        Destroy(gameObject);
    }


    private void updateBoundingBox() {
        // Final position will always be at local (0, 0, 0)

        // Initial Position Relative to Projectile Final Position
        initialPos = transform.InverseTransformPoint(initialPos);

        // Box Collider Center //

        // Since final = 0, center = (initial + final) / 2

        boundingBox.center = initialPos * 0.5f;

        // Box Collider Size //

        // Distance between initial and final position

        // Since final = 0, distance = Sqrt(Pow(initialPos.z, 2) + Pow(initialPos.z, 2))
        // distance = Sqrt(Pow(initialPos.z, 2))

        float distanceZ = Mathf.Sqrt(Mathf.Pow(initialPos.z, 2));

        // This could be simplified to (distanceZ = initialPos.z) 
        // but it seems to make the size of the bounding box smaller than normal

        // boxSize = initialDefaultSize/2 + finalDefaultSize/2 + distanceZ
        // Since initialDefaultSize == finalDefaultSize == (gameObjectSize)
        // boxSize = gameObjectSize + distanceZ

        Vector3 boxSize = gameObjectSize;
        boxSize.z += distanceZ;

        boundingBox.size = boxSize;
    }

    public void Hit(GameObject Player) {
        if (OnCollision == null) return;
        OnCollision.Invoke(Player);
    }

    private void OnEnable() {
        OnCollision += ProjectileHit;
    }

    private void OnDisable() {
        OnCollision -= ProjectileHit;
    }

    private void OnTriggerEnter(Collider other) {
        // Debug.Log("projectile hit:"+other.gameObject.name+" from: "+gameObject.GetType().FullName);
        ProjectileHit(other.gameObject);
    }

    //called when projectile hit
    public virtual void ProjectileHit(GameObject hitObject) {
        Entity e = hitObject.GetComponent<Entity>();
        bool damage = false;
        if (e != null) {
            damage = e.damageEntity(gameObject); //the current game object
        }

        //do the trigger stuff here for the wand + flashy bits
        if (damage) DestroyProjectile();
        if (hitObject.tag.Equals("Crate")) DestroyProjectile();
        if (hitObject.tag.Equals("Environment"))
        {
            Rigidbody body = hitObject.GetComponent<Rigidbody>();
            if (body == null)
            {
                DestroyProjectile();
                return;
            }

            Vector3 force =  gameObject.transform.position - hitObject.transform.position;
            force = force.normalized;
            force *= 100;  //newtons
            body.AddForce(force, ForceMode.Impulse);
            DestroyProjectile();
        }
    }

    public abstract void DestroyProjectile();
}
