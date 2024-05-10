using System;
using System.Collections;
using DuelingScene.Entity;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class PlayerObject : MonoBehaviour, Entity {
    private BoxCollider boxCollider;
    private IDamage Damage;

    
    private GameObject[] potionCircles;
    private Vector3[] circleOffsets;
    
    const int POTION_TIME = 3;  // time in seconds before the player gets a new potion
    const int NUM_CIRCLES = 1;   //the max number of potions a player can carry
    private int timeWaited;
    private Scene duelingScene;
    private GameObject parent;

    private void Start() {
        // Create hands as rigidbody
        gameObject.AddComponent<Rigidbody>();
        Rigidbody enemyRigid = FindObjectOfType<Rigidbody>();
        enemyRigid.useGravity = false;
        enemyRigid.isKinematic = true;
        
        
        duelingScene = SceneManager.GetActiveScene();  //
        
        timeWaited = 0;
        parent = gameObject.transform.parent.gameObject;
        createPotionCircles();
        StartCoroutine(checkFillCircles());
    }

    
    private float[] rotate(float x, float y, float angleRads)
    {
        double nx = x*Math.Cos(angleRads) - y*Math.Sin(angleRads);
        double ny = x*Math.Sin(angleRads) + y*Math.Cos(angleRads);
        return new float[] { (float)nx, (float)ny };
    }
    
    private void Update() {
        UpdatePlayerHitbox();
        
        //We do need this actually, cause the height when the game starts is 0, so using diff of height might not work
        //if you call the spawnCircles() in start()
        updateCirclePositions();
    }

    
    
    private void createPotionCircles()
    {
        string domHand = Player1.DominantSide;
        int dir = domHand.Equals("right") ? 1 : -1;
        
        
        
        potionCircles = new GameObject[NUM_CIRCLES];
        circleOffsets = new Vector3[NUM_CIRCLES];
        
        const float MARGIN = 0.77f; //the angle inbetween potions (45 deg)  << this is in radians!!!
        const string POTION_CIRCLE_PATH = "PotionBubble";   ///prefab to inst
        Object circlePrefab = Resources.Load<Object>(POTION_CIRCLE_PATH);
        
                

        //the idea is that we want to have x circles around the player facing the front
        /*
         *         x   x
         *       x   ^   x
         *
         *   we want to start from the middle of the player and work outwards
         */


        Transform form = gameObject.transform;

        Vector3 frontDirection = form.forward; 


        float totalOffset = MARGIN * NUM_CIRCLES;
        float angleHalf = totalOffset / 2; //the angles for one side of the potions
        
        float[] newAngle = rotate(frontDirection.x, frontDirection.z, dir * angleHalf);  //+angle since we're going leftwards
        
        
        for (int circlesLeft = NUM_CIRCLES; circlesLeft > 0; circlesLeft--)
        {
            Vector3 spawnPos = form.position;
            Vector3 offset = new Vector3(newAngle[0], 0, newAngle[1]) / 2;  //   /2 to make the circles closer to the player. Adjust as needed.
            spawnPos += offset;
            GameObject circleInstance = Instantiate(circlePrefab, spawnPos, Quaternion.identity) as GameObject;
            newAngle = rotate(newAngle[0], newAngle[1], -dir * MARGIN);  //-margin

          //  circleInstance.transform.parent = gameObject.transform;
            
            potionCircles[NUM_CIRCLES - circlesLeft] = circleInstance;
            circleOffsets[NUM_CIRCLES - circlesLeft] = offset;
        }
        
        //this spawns potions around the floor area.
        //2 things - check for trigger enters with the floor - if true, ignore
        //         - update positions of x,y,z each update to adjust the height to reflect player height & follow player pos
        //             ^ if you want rotations too then extract this method and use that to set rotations
        //
        //to make your life easier only add collision detection stuff to the objects when the player actually picks them up
        
    }
    
            
    private void updateCirclePositions()
    {
        /*
         *We want to update the positions each frame to be relative to the player
         */
        const float heightMultiplier = 0.6f; // this is about chest/waist height
        
        Transform parentTrans = parent.transform;
        Transform cameraTrans = gameObject.transform;
        Vector3 cameraPos = cameraTrans.position;
        
        //get the current height of the camera, *= by heightMult to set it at about waist height. Also
        //ensures that it doesn't go negative if the player crouches.
        float height = Math.Max((cameraPos.y - parentTrans.position.y) * heightMultiplier , 0 );
        for (int index = 0; index < potionCircles.Length; index++)
        {
            //use that y value, set it to the y of (offset+camera pos, excluding y).
            
            Vector3 pos = new Vector3(cameraPos.x + circleOffsets[index].x, height + parentTrans.position.y,
                cameraPos.z + circleOffsets[index].z);
            
            potionCircles[index].transform.position = pos;
        }
    }
    
        private IEnumerator checkFillCircles()
    {
        const string POTION_RESOURCE = "EnchantedPotion";
        Object potionPrefab = Resources.Load<Object>(POTION_RESOURCE);
        
        Scene current = SceneManager.GetActiveScene();
        int number = 0;
        
        while (current.Equals(duelingScene))
        {
            current = SceneManager.GetActiveScene();
            if (!PotionInteractionLogic.continueSpawning)
            {
                yield return new WaitForSeconds(1f);
            }
            else if (timeWaited < POTION_TIME)
            {
                timeWaited++;
                yield return new WaitForSeconds(1f);
            }
            else
            {
                bool hasEmpty = false;
                foreach (GameObject circle in potionCircles)
                {
                    if (circle.transform.childCount > 0)
                        continue;

                    GameObject potion = Instantiate(potionPrefab, Vector3.zero, Quaternion.identity) as GameObject;
                    if (potion != null)
                    {
                        Transform trans = potion.transform;
                        trans.parent = circle.transform; 
                        trans.localPosition = new Vector3(0,-0.38f,0);  //rotate/move so it's inside the circle
                        trans.rotation = Quaternion.Euler(-90,0,0);
                        //note that a side effect of this is that the rotation of the potion will change
                        //as the player's head rotates.
                        timeWaited = 0;
                        
                        //also allows us to use the onTriggerEnter() event fxn for events
                        PotionInteractionLogic logic = potion.AddComponent<PotionInteractionLogic>();
                        
                        
                        /////////////////////////////////////////////
                        logic.Initialize(gameObject);  //<<<<<<<------------------------------VERY IMPORTANT
                        /////////////////////////////////////////////
                        
                        
                        potion.name = potion.name + number; //< this is for debug
                        number++;   //< this is for debug


                        //do stuff here to do with preparing the object for interaction
                    }
                    
                    
                    hasEmpty = true;  //  << this is to prevent the thing from getting stuck on the while and crushing everything
                    break;

                }
                
                if (!hasEmpty)
                    yield return new WaitForSeconds(1f);
                
            }

        }
    }

        
        
        
    //@Override from Entity
    public bool damageEntity(GameObject damageSource)
    {
        PotionEnemy potion = damageSource.GetComponent<PotionEnemy>();
        ProjectileEnemy projectile = damageSource.GetComponent<ProjectileEnemy>();

        if (potion != null) return DecreaseHealth(GameDefault.damagePotion);
        if (projectile != null) return DecreaseHealth(GameDefault.damageProjectile);

        return false;
    }



    private bool DecreaseHealth(int decrement)
    {
        Player1.health -= decrement;
        if (Player1.health <= 0 && Game.startGame)
        {
            Game.startGame = false;
            Entity.FadePlayer();

        }

        return true;
    }


    private void UpdatePlayerHitbox() {
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
    
    
    // public void FadePlayer() {
    //     // Fade out back to main menu
    //     TransistionScene transition = GameObject.Find("[CameraRig]").GetComponent<TransistionScene>();
    //     transition.fadeOutToScene(3f, "MenuScene");
    // }


    public void die()
    {
        // Disable projectile damage
        BoxCollider enemyCollider = GameObject.Find("Enemy").GetComponent<BoxCollider>();
        enemyCollider.isTrigger = false;

        BoxCollider playerCollider = GameObject.Find("Camera").GetComponent<BoxCollider>();
        playerCollider.isTrigger = false;

        // Fade out back to main menu
        TransistionScene transition = GameObject.Find("[CameraRig]").GetComponent<TransistionScene>();
        transition.fadeOutToScene(3f, "MenuScene");
    }
    
}
