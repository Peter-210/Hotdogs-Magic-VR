using UnityEngine;

public static class Game {
    public static bool startGame = false;
    public static int crates = 3;
}

public static class Player1 {
    public static string DominantSide = "right";
    public static int health = 5;
}

public static class Player2 {
    public static Vector3 spawnPoint;
    public static int health = 5;
}