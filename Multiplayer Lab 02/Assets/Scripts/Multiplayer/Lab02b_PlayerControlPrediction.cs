using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;


// Holds the state for animation changes
public enum CharacterState
{
    Idle = 0,
    WalkingForward = 1,
    WalkingBackwards = 3,
    Jumping = 4
}


// @author: Tiffany Fischer
// Modified by: Nathan Boehning

// Purpose: Updates the characters state and position with client side prediction
public class Lab02b_PlayerControlPrediction : NetworkBehaviour
{
    // Stores the location, rotation, position in the queue, and animation state
    private struct PlayerState
    {
        public int movementNumber;
        public float posX, posY, posZ;
        public float rotX, rotY, rotZ;
        public CharacterState animationState;
    }

    [SyncVar(hook = "OnServerStateChanged")]
    private PlayerState serverState;     // Represent the state of the player on the server

    private PlayerState predictedState; // Represent the state of the player as predicted on the client only

    private Queue<KeyCode> pendingMoves; // Represent the moves that the player is attempting that has not been
                                         // acknowledged by the server yet. FIFO

    // Controller that allows you to quickly switch between animation states
    public Animator animatorController;

    // Sets the initial position, rotation, and queue location of the player
    void InitState()
    {

        serverState = new PlayerState
        {
            movementNumber = 0,
            posX = -119f,
            posY = 165.08f,
            posZ = -924f,
            rotX = 0f,
            rotY = 0f,
            rotZ = 0f

        };

    }

    // Updates the position and rotation and animation
    void SyncState()
    {
        PlayerState stateToRender = isLocalPlayer ? predictedState : serverState;

        transform.localPosition = new Vector3(stateToRender.posX, stateToRender.posY, stateToRender.posZ);
        transform.localRotation = Quaternion.Euler(stateToRender.rotX, stateToRender.rotY, stateToRender.rotZ);
        animatorController.SetInteger("CharacterState", (int)stateToRender.animationState);

    }

    // Updates the animation based on the change in x, y, z, and rotation
    CharacterState CalcAnimation(float dx, float dy, float dz, float dRY)
    {
        if (dx == 0 && dy == 0 && dz == 0)
            return CharacterState.Idle;
        if (dx != 0 || dz != 0)
        {
            if (dx > 0 || dz > 0)
            {
                return CharacterState.WalkingForward;
            }
            else
                return CharacterState.WalkingBackwards;
        }
        return CharacterState.Idle;
    }

    // Moves the player based on the previous state and the key that was pressed
    PlayerState Move(PlayerState previous, KeyCode newKey)
    {
        float deltaX = 0, deltaY = 0, deltaZ = 0;
        float deltaRotationY = 0;

        switch (newKey)
        {
            case KeyCode.Q:
                deltaX = -0.1f;
                break;
            case KeyCode.S:
                deltaZ = -0.1f;
                break;
            case KeyCode.E:
                deltaX = 0.1f;
                break;
            case KeyCode.W:
                deltaZ = 0.1f;
                break;
            case KeyCode.A:
                deltaRotationY = -1f;
                break;
            case KeyCode.D:
                deltaRotationY = 1f;
                break;
            case KeyCode.Space:
                break;
        }

        // Returns the playerstate given the new x, y, z, and rotation values gotten from the different keys
        return new PlayerState
        {
            movementNumber = 1 + previous.movementNumber,
            posX = deltaX + previous.posX,
            posY = deltaY + previous.posY,
            posZ = deltaZ + previous.posZ,
            rotX = previous.rotX,
            rotY = deltaRotationY + previous.rotY,
            rotZ = previous.rotZ,
            animationState = CalcAnimation(deltaX, deltaY, deltaZ, deltaRotationY)
        };
    }

    // Gets the next move from the queue and updates the predicted state
    void OnServerStateChanged(PlayerState newState)
    {
        serverState = newState;
        if (pendingMoves != null)
        {
            while (pendingMoves.Count > (predictedState.movementNumber - serverState.movementNumber))
            {
                pendingMoves.Dequeue();
            }
            UpdatePredictedState();
        }
    }

    // Updates the key based on the next queue
    void UpdatePredictedState()
    {
        predictedState = serverState;
        foreach (KeyCode moveKey in pendingMoves)
        {
            predictedState = Move(predictedState, moveKey);
        }
    }

    // Use this for initialization
    void Start()
    {
        // Initializes the players position
        InitState();
        // Sets the predicted state based on the servers state
        predictedState = serverState;

        // If it's the local player
        if (isLocalPlayer)
        {
            // Create the move queue
            pendingMoves = new Queue<KeyCode>();
            UpdatePredictedState();
        }

        // Sync the state
        SyncState();

    }

    // Update is called once per frame
    void Update()
    {

        if (isLocalPlayer)
        {
            //Debug.Log("Pending moves: " + pendingMoves.Count);

            // Array of the eligible keys that the player can press
            KeyCode[] possibleKeys = { KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.Space };
            bool somethingPressed = false;

            // Loop through all of the keys
            foreach (KeyCode possibleKey in possibleKeys)
            {
                if (!Input.GetKey(possibleKey)) // If currently observed key code is not pressed
                    continue;                   // Do nothing

                // Sets the bool to true
                somethingPressed = true;
                pendingMoves.Enqueue(possibleKey);  // Queues the key into the pending moves queue          
                UpdatePredictedState();             // Updates the predicted state
                CmdMoveOnServer(possibleKey);       // Sends the update to the server
            }

            if (!somethingPressed)
            {
                pendingMoves.Enqueue(KeyCode.Alpha0);
                UpdatePredictedState();
                CmdMoveOnServer(KeyCode.Alpha0);
            }
        }

        SyncState();

    }

    [Command]
    void CmdMoveOnServer(KeyCode pressedKey)
    {
        // Set the serverstate
        serverState = Move(serverState, pressedKey);
    }
}
