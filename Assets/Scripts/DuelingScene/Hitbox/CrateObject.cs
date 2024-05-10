using System.Collections;
using System.Collections.Generic;
using DuelingScene.Entity;
using UnityEngine;

public class CrateObject : MonoBehaviour, Entity {
    private int crateHealth = GameDefault.healthCrate;

    public bool damageEntity(GameObject damageSource) {
        ProjectilePlayer projectilePlayer = damageSource.GetComponent<ProjectilePlayer>();
        ProjectileEnemy projectileEnemy = damageSource.GetComponent<ProjectileEnemy>();

        if (projectilePlayer != null || projectileEnemy != null) {
            crateHealth--;

            if (crateHealth <= 0 && Game.startGame) {
                die();
                return true;
            }
        }
        return false;
    }

    public void die() {
        Destroy(gameObject);
    }
}
