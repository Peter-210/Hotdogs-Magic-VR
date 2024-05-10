using UnityEngine;

// Globals are set on GameManager.cs
// Reset occurs on StartMenuScene.cs [resetGlobal()]

public static class GameDefault {
    // Game Defaults
    public static bool startGame = false;
    
    // Crate Defaults
    public static int crates = 3;
    public static int healthCrate = 3;

    // Damage Defaults
    public static int damageProjectile = 1;
    public static int damagePotion = 2;

    // Player1 Defaults
    public static int healthPlayer1 = 5;
    public static string DominantSide = "right";

    // Player2 Defaults
    public static int healthPlayer2 = 10;
    
    //distance in which splash damage should be applied for potion
    public static float explosionRadius = 0.5f;
    
}
