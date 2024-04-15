using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamage : MonoBehaviour {
    private BoxCollider boxCollider;
    private IDamage Damage;

    private void Start() {
        addRigid();
    }

    private void Update() {
        PlayerHitbox();
    }

    private void OnTriggerEnter(Collider collision) {
        Damage = collision.GetComponent<IDamage>();
        Damage.Hit(gameObject);
    }

    private void addRigid() {
        // Create hands as rigidbody
        gameObject.AddComponent<Rigidbody>();
        Rigidbody enemyRigid = FindObjectOfType<Rigidbody>();
        enemyRigid.useGravity = false;
        enemyRigid.isKinematic = true;
    }

    private void PlayerHitbox() {
        float boxCollierY = gameObject.transform.position.y;
        float CameraRigY = Mathf.Abs(GameObject.Find("[CameraRig]").transform.position.y);

        boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider == null) {
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        float midpoint = -(boxCollierY + CameraRigY)/2f ;
        boxCollider.center = new Vector3(0, midpoint, 0);
        boxCollider.size = new Vector3(0.69f, boxCollierY + CameraRigY, 0.55f);
        boxCollider.isTrigger = true;
    }
}
