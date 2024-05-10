using UnityEngine;

public class EnemyStatePotion : EnemyStateAbstract {
    [SerializeField] private float delayMovement = 1f;
    private float currentTime;

    [SerializeField] private string PotionPath = "EnchantedPotion";
    [SerializeField] private string PotionName = "PotionEnemy";
    private GameObject potionObject;
    private Rigidbody potionRigid;

    [SerializeField] private float throwAngle = 45f;
    [SerializeField] private float throwPower = 4f;

    public override void EnterState
    (EnemyStateManager state) {
        currentTime = 0;
        setupPotion();
        throwPotion();
    }

    public override void UpdateState
    (EnemyStateManager state) {
        if (currentTime >= delayMovement) {
            state.ChangeState(state.Idle);
        }
        else currentTime += Time.deltaTime; // Stand still for a while during potion throw
    }

    private void setupPotion() {
        GameObject enemyObject = GameObject.Find("Enemy");
        Vector3 position = enemyObject.transform.position + enemyObject.transform.up;
        Quaternion rotation = enemyObject.transform.rotation;

        // Spawn a potion from the enemy
        Object potionPrefab = Resources.Load<Object>(PotionPath);
        potionObject = GameObject.Instantiate(potionPrefab, position, rotation) as GameObject;
        potionObject.name = PotionName;

        // Attach potion to the enemy
        potionObject.transform.SetParent(GameObject.Find("Enemy").transform);

        // Rotate the potion so that the up vector is pointing up
        potionObject.transform.Rotate(-90.0f, 180.0f, 0);

        // Make the potion rigid
        potionRigid = potionObject.AddComponent<Rigidbody>();
        potionRigid.useGravity = false;
        potionRigid.isKinematic = true;
        
        // Add spatial audio
        SpatialAudio audio = potionObject.AddComponent<SpatialAudio>();
        audio.setSound("projectileAudio", true);

        // Travel Component for Potion
        potionObject.AddComponent<PotionEnemy>();
    }

    private void throwPotion() {
        // Randomly rotate potion to point between left and right vectors
        float rotateAngle = Random.Range(-throwAngle, throwAngle);
        potionObject.transform.Rotate(0, 0, rotateAngle);

        // Throw the potion
        Vector3 throwVector = potionObject.transform.up * throwPower;
        potionRigid.useGravity = true;
        potionRigid.isKinematic = false;
        potionRigid.AddForce(throwVector, ForceMode.Impulse);
        potionObject.transform.SetParent(null);
    }
}
