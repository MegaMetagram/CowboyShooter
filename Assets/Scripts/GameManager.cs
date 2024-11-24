/*
@Authors - Patrick
@Description - Class that manages game data that needs to be shared between other classes. It could just go in player because
that's a singleton as well, but we want to keep the scripts concise.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager gameManager;
    
    public int currentLevel;
    public int currentCheckpoint;
    [SerializeField] private bool debugMode = false;
    private bool disableCursor;
    private bool enableCursor;

    public void Awake()
    {
        //Deletes itself if there's another instance. Basically forces the class to be a singleton
        if (gameManager != null && gameManager != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            gameManager = this; 
        } 
    }

    //!You MUST change game manager variables here!
    public void Start()
    {               
        disableCursor = false;
        enableCursor = false;

        currentCheckpoint = 0;
    }

    public void Update()
    {
        if (debugMode)
        {
            Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in enemies)
            {
                Destroy(enemy.gameObject);
            }
        }

        //This cannot be if-else, they are not mutually exclusive
        if (enableCursor){
            EnableCursor();
        }
        if (disableCursor) {
            DisableCursor();
        }
    }

    public void EnableCursor() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void DisableCursor() 
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
