using UnityEngine;

public class EnemyStateIdle : EnemyStateAbstract {
    [SerializeField] private float shootPercent = 0.7f;
    [SerializeField] private float movementRange = 3.5f;
    [SerializeField] private float threshold = 0.2f;
    [SerializeField] private float speed = 1.0f;
    private float currentZPos;
    private float finalZPos;

    private GameObject enemyObject;

    public override void EnterState
    (EnemyStateManager state) {
        // Store current position of the enemy
        enemyObject = GameObject.Find("Enemy");
        currentZPos = enemyObject.transform.position.z;

        // Pick a random location for the enemy to go to
        float rightMax = movementRange/2 + Player2.spawnPoint.z;
        float leftMax = -rightMax;
        finalZPos = Random.Range(leftMax, rightMax);
    }

    public override void UpdateState
    (EnemyStateManager state) {
        // Update position of the enemy
        currentZPos = enemyObject.transform.position.z;
        float offset = Mathf.Abs(currentZPos - finalZPos);

        if (offset <= threshold) {
            // Change enemy to shoot or potion state when it has moved to the target position
            // Meaning that the enemy is within threshold
        
            if (Random.value <= shootPercent) {
              state.ChangeState(state.Shoot);
            } else {
              state.ChangeState(state.Potion);
            }
        } else {
            // Move enemy to the random position
            if (currentZPos - finalZPos > 0)
                enemyObject.transform.Translate(-Vector3.right * speed * Time.deltaTime);
            else
                enemyObject.transform.Translate(Vector3.right * speed * Time.deltaTime);
        }
    }
}
