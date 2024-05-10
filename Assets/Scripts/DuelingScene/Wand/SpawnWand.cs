using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnWand : MonoBehaviour {
    [SerializeField] private string wandPath = "Wand";
    private Object wandPrefab;
    
    private void Start() {
        // Load Wand Prefab

        GameObject controller;
        controller = Player1.DominantSide.Equals("right")
            ? GameObject.Find("Controller (right)")
            : GameObject.Find("Controller (left)");
        
        
        
        
        wandPrefab = Resources.Load<Object>(wandPath);

        Vector3 position = transform.position + transform.forward;
        Quaternion rotation = transform.rotation;

        // Spawn the wand into player's hand
        GameObject wandObject = Instantiate(wandPrefab, position, rotation) as GameObject;
        wandObject.transform.SetParent(transform); 
        wandObject.name = "Wand";
    
        // Move and scale wand to fit in player's hand
        wandObject.transform.localScale = new Vector3(60f, 60f, 35f);
        wandObject.transform.Translate(0, 0, -1.15f);
        
        
        Vector3 globalPos = controller.transform.TransformPoint(0f,0f,-0.1f);
      //  Quaternion rot = controller.transform.rotation;
      //  rot *= Quaternion.Euler(Vector3.right * -35);
        
     //   wandObject.transform.rotation = rot;
        wandObject.transform.position = globalPos;
        

        // Add components for wand logic
        Rigidbody wandRigid = wandObject.AddComponent<Rigidbody>();
        wandRigid.useGravity = false;
        wandRigid.isKinematic = true;

        wandObject.AddComponent<WandLogic>();
    }
    
    
    
}


/*
*/