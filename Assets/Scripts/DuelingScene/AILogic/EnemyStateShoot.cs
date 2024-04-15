using UnityEngine;

public class EnemyStateShoot : EnemyStateAbstract {
    [SerializeField] private float delayShot = 1f;
    private float currentTime;

    [SerializeField] private string ProjectilePath = "ProjectileEnemy";
    private GameObject projectileObject;

    public override void EnterState
    (EnemyStateManager state) {
        setupProjectile();
        currentTime = 0;
    }

    public override void UpdateState
    (EnemyStateManager state) {
        if (currentTime >= delayShot) {
            fireProjectile();
            state.ChangeState(state.Idle);
        }
        else currentTime += Time.deltaTime; // Stand still for a while to charge projectile
    }

    private void setupProjectile() {
        GameObject enemyObject = GameObject.Find("Enemy");
        Vector3 position = enemyObject.transform.position + enemyObject.transform.up;
        Quaternion rotation = enemyObject.transform.rotation;

        // Spawn a projectile from the enemy
        Object projectilePrefab = Resources.Load<Object>(ProjectilePath);
        projectileObject = GameObject.Instantiate(projectilePrefab, position, rotation) as GameObject;
        projectileObject.name = ProjectilePath;

        // Attach projectile to the enemy
        projectileObject.transform.SetParent(GameObject.Find("Enemy").transform);

        // Raise projectile to be a bit above center of mass
        projectileObject.transform.Translate(0, 0, 0.3f);

        GameObject projectileChild = projectileObject.transform.GetChild(0).gameObject;

        // Make the projectile rigid
        Rigidbody projectileRigid = projectileObject.AddComponent<Rigidbody>();
        projectileRigid.useGravity = false;
        projectileRigid.isKinematic = true;

        Rigidbody projectileChildRigid = projectileChild.AddComponent<Rigidbody>();
        projectileChildRigid.useGravity = false;
        projectileChildRigid.isKinematic = true;

        // Make the projectile have a box collider
        BoxCollider boxCollider = projectileObject.AddComponent<BoxCollider>();
        boxCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
        boxCollider.isTrigger = true;

        // projectileChild.AddComponent<BoxCollider>();

        // Add spatial audio
        projectileObject.AddComponent<SpatialAudio>();
    }

    private void fireProjectile() {
        if (projectileObject == null) return;

        // Detach projectile from the enemy
        projectileObject.transform.SetParent(null);

        // Add component for projectile logic - Let the projectile fly
        projectileObject.AddComponent<ProjectileEnemy>();
    }
}
