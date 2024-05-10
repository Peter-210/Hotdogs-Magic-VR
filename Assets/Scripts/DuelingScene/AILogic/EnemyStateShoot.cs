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

        // Rotate the projectile so that it flies towards the enemy
        projectileObject.transform.Rotate(-90.0f, 180.0f, 0);

        GameObject projectileChild = projectileObject.transform.GetChild(0).gameObject;

        // Add spatial audio
        SpatialAudio audio = projectileObject.AddComponent<SpatialAudio>();
        audio.setSound("projectileAudio", true);
    }

    private void fireProjectile() {
        if (projectileObject == null) return;

        // Detach projectile from the enemy
        projectileObject.transform.SetParent(null);

        // Add component for projectile logic - Let the projectile fly
        projectileObject.AddComponent<ProjectileEnemy>();
    }
}
