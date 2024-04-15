using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Valve.VR;

public interface IMenuSelection {
    public void Select(GameObject controller);
}

public class MenuSelection : MonoBehaviour {
    private SteamVR_Input_Sources InputSource;
    private SteamVR_Action_Boolean ActionBoolean;

    private Rigidbody controllerRigid;
    private BoxCollider collisionBox;

    private GameObject controller;
    private IMenuSelection MenuOption;
    private IMenuSelectionExit exit;
    private bool isRight;
    

    private void Start() {
        initSteamVR();
        addComponents();
    }

    private void Update() {
        
        
        if (ActionBoolean.GetStateDown(InputSource)) {

         if (isRight)
             SteamVR_Behaviour_Skeleton.isRightClenching = true;
         else
             SteamVR_Behaviour_Skeleton.isLeftClenching = true;
         
         if (MenuOption == null)
            {
                return;
            }

            ////for some reason I had to update this code here in order for the event to register the book - Talon
            /// also increasing the book hitbox helps
         //   Debug.Log("MenuOption:"+MenuOption.GetType().Name); // < this should return BookInteractionHandler if it is the book
          //  Debug.Log("========end======");
            MenuOption.Select(gameObject);
        }
        else if (ActionBoolean.GetStateUp(InputSource))
        {
            
            //this is for the glove mechanics
            if (isRight)
                SteamVR_Behaviour_Skeleton.isRightClenching = false;
            else
                SteamVR_Behaviour_Skeleton.isLeftClenching = false;
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
        
   //     if (MenuOption!=null)
    //        Debug.Log("option:"+MenuOption.GetType().Name);
    //    else Debug.Log("option is null");
    }

    private void OnTriggerExit(Collider other)
    {
        exit = other.GetComponent<IMenuSelectionExit>();
        MenuOption = null;
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
