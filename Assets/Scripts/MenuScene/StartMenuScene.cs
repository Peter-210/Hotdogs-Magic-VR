using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuScene : MonoBehaviour {
    [SerializeField] private string musicPath = "menuSong";
    [SerializeField] private float volume = 0.2f;
    [SerializeField] private float fadeInDuration = 1.0f;
    [SerializeField] private string bookPath = "bookAudio";
    [SerializeField] private string wandPath = "wandEquipAudio";
    
    private void Awake() {
        resetGlobal();
        initController();
        initWand();
        initTutorial();
        startScene();
    }

    private void resetGlobal() {
        // Game Globals
        Game.startGame = GameDefault.startGame;
        Game.crates = GameDefault.crates;

        // Player1 Globals
        Player1.DominantSide = GameDefault.DominantSide;
        Player1.health = GameDefault.healthPlayer1;
        
        // Player2 Globals
        Player2.health = GameDefault.healthPlayer2;
    }

    private void initController() {
        GameObject leftController = GameObject.Find("Controller (left)");
        leftController.AddComponent<MenuSelection>();

        GameObject rightController = GameObject.Find("Controller (right)");
        rightController.AddComponent<MenuSelection>();
    }

    private void initWand() {
        GameObject wandObject = GameObject.Find("Wand");
        
        Material wandShine = gameObject.GetComponent<Renderer>().sharedMaterial;
        wandShine.SetFloat("_Fresnel", 0);
        wandShine.SetFloat("_Dist", 0);
        

        AudioSource wandAudio = wandObject.AddComponent<AudioSource>();
        wandAudio.playOnAwake = false;
        wandAudio.clip = Resources.Load<AudioClip>(wandPath);

        wandObject.AddComponent<StartDuel>();
    }

    private void initTutorial() {
        GameObject tutorialObject = GameObject.Find("BookTutorial");
       // tutorialObject.AddComponent<Tutorial>();  don't put this on there else it will break

        GameObject dynamicBook = GameObject.Find("BookInteractable");
        initBook(tutorialObject, dynamicBook);
        
        

        GameObject tutorialVideo = GameObject.Find("Video");
        tutorialVideo.AddComponent<Video>();
        tutorialVideo.SetActive(false);
    }
    
    private void initBook(GameObject staticBook, GameObject dynamicBook)
    {

        staticBook.AddComponent<ArticulationBody>();
        ArticulationBody body = staticBook.GetComponent<ArticulationBody>();
        body.useGravity = false;
        body.immovable = true;
        
        
        dynamicBook.AddComponent<HingeJoint>();
        HingeJoint joint = dynamicBook.GetComponent<HingeJoint>();
        joint.anchor = new Vector3(0.013f, 0f, 0.0045f);
        joint.axis = new Vector3(0, 1, 0);
        joint.connectedAnchor = new Vector3(-1.4307f, 0.0576f, -0.463f);
        joint.connectedArticulationBody = body;
        
        joint.useSpring = true;
        JointSpring spring = new JointSpring();
        spring.spring = 500;  ///newtons
        spring.damper = 100;
        spring.targetPosition = -180;

        joint.spring = spring;

        BoxCollider collider = dynamicBook.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        
        AudioSource bookAudio = dynamicBook.AddComponent<AudioSource>();
        bookAudio.playOnAwake = false;
        bookAudio.clip = Resources.Load<AudioClip>(bookPath);
        
        dynamicBook.AddComponent<BookInteractionHandler>();

    }

    private void startScene() {
        // Background Dueling Music
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        if (audioSource.isPlaying) audioSource.Stop();
        audioSource.loop = true;
        audioSource.clip = Resources.Load<AudioClip>(musicPath);
        audioSource.volume = volume;
        audioSource.Play();

        // Fade In Player View
        gameObject.AddComponent<TransistionScene>();
        TransistionScene transition = FindObjectOfType<TransistionScene>();
        transition.fadeInView(fadeInDuration);
    }
}
