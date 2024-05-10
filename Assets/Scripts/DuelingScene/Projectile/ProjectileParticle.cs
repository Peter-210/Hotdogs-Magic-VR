using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileParticle : MonoBehaviour {
    [SerializeField] private float duration = 3f;

    private void Start() {
        StartCoroutine(particleDie());
    }

    private IEnumerator particleDie() {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}


