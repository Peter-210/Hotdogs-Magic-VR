using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyStateAbstract {
    public abstract void EnterState
        (EnemyStateManager state);

    public abstract void UpdateState
        (EnemyStateManager state);
}

public class EnemyStateManager : MonoBehaviour {
    EnemyStateAbstract currentState;

    // Possible Enemy States
    public EnemyStateIdle Idle = new EnemyStateIdle();
    public EnemyStateShoot Shoot = new EnemyStateShoot();

    void Awake() {
        // Add rigidbody and hitbox component to the enemy
        initEnemy();
    }

    void Start() {
        // Set the state to idle
        currentState = Idle;
        currentState.EnterState(this);
    }

    void FixedUpdate() {
        if (Game.startGame == true) {
            currentState.UpdateState(this);
        }
    }

    private void initEnemy() {
        GameObject.Find("Enemy").AddComponent<EnemyObject>();
    }

    public void ChangeState(EnemyStateAbstract state) {
        currentState = state;
        state.EnterState(this);
    }
}
