using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PotionEnemy : PotionAbstract {
    private GameObject targetObject;

    protected override void Init() {
        rotateReset = new Vector3(0, 90, 0);
    }

    protected override void Start() {
        targetObject = GameObject.Find("Camera");
        base.Start();
    }

    public override void AimPotion() {
        GameObject rigObject = GameObject.Find("[CameraRig]");

        //// Calculating Horizontal Angle ////

        // Pythagorean Theorem
        
        // Opposite
        float enemyToPlayerZ = 
            Mathf.Abs(gameObject.transform.position.z - targetObject.transform.position.z);

        // Adjacent
        float enemyToPlayerX = 
            Mathf.Abs(targetObject.transform.position.x - gameObject.transform.position.x);

        // Hypotenuse
        float enemyToPlayerHorizontal = 
            Mathf.Sqrt(Mathf.Pow(enemyToPlayerZ, 2f) + Mathf.Pow(enemyToPlayerX, 2f));

        float horizontalAngle = Mathf.Rad2Deg * Mathf.Asin(enemyToPlayerZ/enemyToPlayerHorizontal);

        // Negative depending on if enemy is past player Z position
        if (targetObject.transform.position.z - gameObject.transform.position.z > 0) 
            horizontalAngle = -horizontalAngle;


        //// Calculating Vertical Angle ////

        // Pythagorean Theorem

        // Adjust to aim a bit above the player's center of mass
        float playerCenter = 
            Mathf.Abs(targetObject.transform.position.y - rigObject.transform.position.y) * 0.66f;

        // Add back the camera rig offset
        playerCenter = playerCenter + rigObject.transform.position.y;

        // Opposite
        float PlayerToPotion = 
            Mathf.Abs(playerCenter - gameObject.transform.position.y);

        // Adjacent is already calculated from the horizontal angle calculation

        // Hypotenuse
        float enemyToPlayerVertical = 
            Mathf.Sqrt(Mathf.Pow(enemyToPlayerX, 2f) + Mathf.Pow(PlayerToPotion, 2f));
        
        float verticalAngle = Mathf.Rad2Deg * Mathf.Asin(PlayerToPotion/enemyToPlayerVertical);

        // Negative value when player center is above line of enemy fire
        if (playerCenter - gameObject.transform.position.y > 0) 
            verticalAngle = -verticalAngle;

        // Adjust the trajectory of potion
        gameObject.transform.Rotate(verticalAngle, horizontalAngle, 0);
    }
}
