using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PotionPlayer : PotionAbstract {
    private GameObject targetObject;

    protected override void Init() {
        rotateReset = new Vector3(0, -90, 0);
    }

    protected override void Start() {
        targetObject = GameObject.Find("Enemy");
        base.Start();
    }

    public override void AimPotion() {
        GameObject rigObject = GameObject.Find("[CameraRig]");

        //// Calculating Vertical Angle ////

        // Pythagorean Theorem

        // Adjust to aim at enemy's center of mass
        float enemyCenter = 
            Mathf.Abs(targetObject.transform.position.y - rigObject.transform.position.y);

        // Add back the camera rig offset
        enemyCenter = enemyCenter + rigObject.transform.position.y;

        // Opposite
        float PlayerToPotion = 
            Mathf.Abs(enemyCenter - gameObject.transform.position.y);

        // Adjacent
        float potionToEnemyX = 
            Mathf.Abs(targetObject.transform.position.x - gameObject.transform.position.x);

        // Hypotenuse
        float enemyToPlayerVertical = 
            Mathf.Sqrt(Mathf.Pow(potionToEnemyX, 2f) + Mathf.Pow(PlayerToPotion, 2f));
        
        float verticalAngle = Mathf.Rad2Deg * Mathf.Asin(PlayerToPotion/enemyToPlayerVertical);

        // Negative value when enemy center is above line of enemy fire
        if (enemyCenter - gameObject.transform.position.y > 0) 
            verticalAngle = -verticalAngle;



        //// Calculating Horizontal Angle ////

        // Better to just have vertical tracking so that players can time 
        // when the potion hits the enemy. (Ignore horizontal angle logic)
        float horizontalAngle = 0;



        // Adjust the trajectory of potion
        gameObject.transform.Rotate(verticalAngle, horizontalAngle, 0);
    }
}
