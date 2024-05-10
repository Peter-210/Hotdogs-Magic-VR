using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;

public interface IMenuSelection {
    public void Select(GameObject controller);
}



//menu selection exit is for when the object exits the trigger hitbox regardless of if the trigger is up or down
//this IOnMenuSelectionRelease is for specifically when the trigger is released
public interface IOnSelectionRelease
{
    public void OnTriggerRelease(GameObject controller);
    
}


public class MenuSelection : MonoBehaviour {
    private SteamVR_Input_Sources InputSource;
    private SteamVR_Action_Boolean ActionBoolean;

    private Rigidbody controllerRigid;
    private BoxCollider collisionBox;

    private GameObject controller;
    private IMenuSelection MenuOption;
    private IMenuSelectionExit exit;
    private IOnSelectionRelease release;

    private bool isRight;
    private bool isKeyPressed;
    

    private void Start() {
        //this class is used for the stuff in the menuscene but I think we can use it generally 
        isKeyPressed = false;
        release = null;
        
        initSteamVR();
        addComponents();
    }

    private void Update() {
        
        
        if (ActionBoolean.GetStateDown(InputSource))
        {
            isKeyPressed = true;
            
         if (isRight)
             SteamVR_Behaviour_Skeleton.isRightClenching = true;
         else
             SteamVR_Behaviour_Skeleton.isLeftClenching = true;
         
         if (MenuOption == null)
                return;
         
         MenuOption.Select(gameObject);
        }
        else if (ActionBoolean.GetStateUp(InputSource))
        {
            
            isKeyPressed = false;
            
            //this is for the glove mechanics
            if (isRight)
                SteamVR_Behaviour_Skeleton.isRightClenching = false;
            else
                SteamVR_Behaviour_Skeleton.isLeftClenching = false;
            
            
            //okay the issue is that the thing is colliding with the camera which is also a trigger....
            //variable release is being set to Camera...
            //so we gotta either prevent that or find a workaround...
            if (release != null)
            {
                //   Debug.Log("UpState - release:"+release.ToString());
                release.OnTriggerRelease(gameObject);
                release = null;

            }
            
        }

        
        //more redundancy this way
        if (exit != null)
        {
            exit.SelectExit(gameObject);
            exit = null;
        }


    }

    private void OnTriggerEnter(Collider collision) {
        
    //    Debug.Log("ontrigger:"+collision.gameObject.name);
        MenuOption = collision.GetComponent<IMenuSelection>();
        
        if (release == null)
            release = collision.GetComponent<IOnSelectionRelease>();

    }

    private void OnTriggerExit(Collider other)
    {
        exit = other.GetComponent<IMenuSelectionExit>();
        
        if (release != null && !isKeyPressed)
        {
            release.OnTriggerRelease(gameObject);
            release = null;
        }

        
        MenuOption = null;
    }
    
    
    private void OnTriggerStay(Collider collision)
    {
        if (release == null)
            release = collision.GetComponent<IOnSelectionRelease>();
        
    }
    

    private void initSteamVR() {
        // Initialize InputSource
        if (gameObject.name.Contains("left"))
        {
            isRight = false;
            InputSource = SteamVR_Input_Sources.LeftHand;
        }
        else if (gameObject.name.Contains("right"))
        {
            isRight = true;
            InputSource = SteamVR_Input_Sources.RightHand;
        }
        else {
            Debug.LogError("Failed to initialize SteamVR input source.");
        }

        // Initialize ActionBoolean
        ActionBoolean = SteamVR_Input
            .GetActionFromPath<SteamVR_Action_Boolean>("/actions/default/in/GrabGrip");
    }

    private void addComponents() {
        // Create hands as rigidbody
        if (gameObject.GetComponent<Rigidbody>() == null) {
            controllerRigid = gameObject.AddComponent<Rigidbody>();
        } else {
            controllerRigid = gameObject.GetComponent<Rigidbody>();
        }
        controllerRigid.useGravity = false;
        controllerRigid.isKinematic = true;

        // Add collision box to hands
        if (gameObject.GetComponent<BoxCollider>() == null) {
            collisionBox = gameObject.AddComponent<BoxCollider>();
        } else {
            collisionBox = gameObject.GetComponent<BoxCollider>();
        }
        collisionBox.isTrigger = true;
        collisionBox.center = new Vector3(0f, 0f, -0.08f);
        collisionBox.size = new Vector3(0.12f, 0.12f, 0.12f);
    }
}
