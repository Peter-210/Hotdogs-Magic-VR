using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Valve.VR;


//@EventHandler - called from MenuSelection.cs
//code for flipping open/close the book in menuscene

public interface IMenuSelectionExit
{
    public void SelectExit(GameObject controller);
}


public class BookInteractionHandler : MonoBehaviour, IMenuSelection, IMenuSelectionExit
{
    //put this script on the interactable book

    // public static event Action<GameObject> BookInteractEventEnter;
    //  public static event Action<GameObject> BookInteractEventExit;


    private GameObject interactableBook;
    private HingeJoint bookHinge;


    private GameObject cameraRig;
    private bool playVideo, activatedOnce;

    private GameObject currentController;

    private SteamVR_Action_Boolean action;
    private SteamVR_Input_Sources rightSource;
    private SteamVR_Input_Sources leftSource;

    private GameObject tutorial;
    private GameObject video;
    private AudioSource soundEffect;
    private AudioSource backgroundMusic;

    //override from IMenuSelection
    public void Select(GameObject controller)
    {

        //technically we don't need the Invoke() statements if they're in the same class...
        //isn't it basically more overhead? The event functions already call the interface subclass. 
        //I guess it is nice to use it though so I'll let you decide if you wanna use the events or not - Talon

        // if (BookInteractEventEnter == null)
        //     return;
        //
        // BookInteractEventEnter.Invoke(controller);
        onBookInteract(controller);
        backgroundMusic = GameObject.Find("[CameraRig]").GetComponent<AudioSource>();



    }


    // override from IMenuSelectionExit
    public void SelectExit(GameObject controller)
    {
        // if (BookInteractEventExit == null)
        //     return;
        // BookInteractEventExit.Invoke(controller);

        onBookInteractExit(controller);
    }


    private void OnEnable()
    {
        //if you decide to use events make sure to uncomment this and in onDisable()
        //   BookInteractEventEnter += onBookInteract;
        //   BookInteractEventExit += onBookInteractExit;


        playVideo = false;
        activatedOnce = false;

        tutorial = GameObject.Find("Tutorial");
        video = tutorial.transform.Find("Video").gameObject;
        interactableBook = GameObject.Find("BookInteractable");
        soundEffect = interactableBook.GetComponent<AudioSource>();


        if (interactableBook == null)
            Debug.LogError("Could not find the interactable book half in MenuScene. (BookInteractionHandler.cs)");
        else
        {
            bookHinge = interactableBook.GetComponent<HingeJoint>();


        }
    }

    private void OnDisable()
    {

        //if you decide to use events make sure to uncomment this
        //    BookInteractEventEnter -= onBookInteract;
        //   BookInteractEventExit -= onBookInteractExit;
    }

    private void PlaySoundEffect()
    {
        if (soundEffect.isPlaying) soundEffect.Stop();
        soundEffect.Play();
    }

    public void FixedUpdate()
    {

        JointSpring spring = new JointSpring();
        spring.damper = 10;
        spring.spring = 50;

        //if the player isn't currently interacting with the book move it to the closest state: either
        //open or closed.
        //if open, play the video
        if (currentController == null)
        {

            if (!activatedOnce)
            {
                spring.targetPosition = -180;
                bookHinge.spring = spring;
                Invoke("PlaySoundEffect", 0);
                return;
            }

            if (bookHinge.angle > -90)
            {
                playVideo = true;  ///thinking of keeping the boolean in the case that want to add other things
                if (backgroundMusic.isPlaying) backgroundMusic.Stop();
                video.SetActive(playVideo);
                Invoke("PlaySoundEffect", 0);
                spring.targetPosition = 0;
            }
            else
            {
                playVideo = false;
                if (!backgroundMusic.isPlaying) backgroundMusic.Play();
                video.SetActive(playVideo);
                Invoke("PlaySoundEffect", 0);
                spring.targetPosition = -180;
            }

            bookHinge.spring = spring;
            return;
        }




        ///constant book positions.
        //will need to change if we move the book
        //#magic numbers 
        Vector3 globalBookPosition = new Vector3(-1.436f, 0.076f, -0.464f);
        Vector3 globalBookLeftPos = new Vector3(-1.53f, 0.076f, -0.803f);

        Vector3 globalControllerPos = currentController.transform.position;

        Vector3 left = globalBookLeftPos - globalBookPosition;

        Vector3 deltas = globalControllerPos - globalBookPosition;

        double horDist = Math.Sqrt(deltas.x * deltas.x + deltas.z * deltas.z);
        double angle = Math.Atan(deltas.y / horDist);

        Vector3 leftNorm = left.normalized;
        Vector3 deltasNorm = deltas.normalized;

        float angleDeg = -(float)(angle * 180 / Math.PI);
        float dot = Vector3.Dot(leftNorm, deltasNorm);
        //using dot product cause I don't wanna do more trigonometry



        float finalAng = angleDeg;

        if (dot < 0)
            finalAng = -69 + (-69 - angleDeg);
        finalAng *= 1.5f;

        finalAng = Math.Max(finalAng, -175);
        spring.targetPosition = finalAng;
        bookHinge.spring = spring;

        //basically the book starts as open and on game load it goes to closed immediately -- we want to ensure the player has interacted with it first
        //before we start playing vids
        activatedOnce = true;


    }


    //@Unused controller
    private void onBookInteractExit(GameObject controller)
    {
        // Debug.Log("Book Interaction exit");
        currentController = null;
    }





    private void onBookInteract(GameObject controller)
    {
        //   Debug.Log("Book interaction enter");
        currentController = controller;

    }
}
