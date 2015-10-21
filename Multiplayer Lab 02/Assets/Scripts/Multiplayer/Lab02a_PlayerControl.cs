using UnityEngine;
using UnityEngine.Networking;

// @author: Tiffany Fischer
// Modified by: Nathan Boehning

// Purpose: Updates the characters state and position
public class Lab02a_PlayerControl : NetworkBehaviour
{
	private struct PlayerState
	{
		public float posX, posY, posZ;
		public float rotX, rotY, rotZ;
	}

	[SyncVar] private PlayerState state;

	// Initialize the state
	void InitState()
	{

		state = new PlayerState
		{
			posX = -119f,
			posY = 165.08f,
			posZ = -924f,
			rotX = 0f,
			rotY = 0f,
			rotZ = 0f

		};

	}

	// Updates the rotation and position of the player
	void SyncState()
	{

		transform.position = new Vector3(state.posX, state.posY, state.posZ);
		transform.rotation = Quaternion.Euler(state.rotX, state.rotY, state.rotZ);

	}

	// Moves the player based on the previous player state, and the key that was pressed
	PlayerState Move(PlayerState previous, KeyCode newKey)
	{
		float deltaX = 0, deltaY = 0, deltaZ = 0;
		float deltaRotationY = 0;

		switch (newKey)
		{
			case KeyCode.Q:
				deltaX = -0.5f;
				break;
			case KeyCode.S:
				deltaZ = -0.5f;
				break;
			case KeyCode.E:
				deltaX = 0.5f;
				break;
			case KeyCode.W:
				deltaZ = 0.5f;
				break;
			case KeyCode.A:
				deltaRotationY = -1f;
				break;
			case KeyCode.D:
				deltaRotationY = 1f;
				break;
		}

		return new PlayerState
		{
			posX = deltaX + previous.posX,
			posY = deltaY + previous.posY,
			posZ = deltaZ + previous.posZ,
			rotX = previous.rotX,
			rotY = deltaRotationY + previous.rotY,
			rotZ = previous.rotZ
		};
	}
	// Use this for initialization
	void Start ()
	{
		// Initializes the player state and syncs with the server
		InitState();
		SyncState();

	}
	
	// Update is called once per frame
	void Update ()
	{

		if (isLocalPlayer)
		{
			// Array of possible keys that can be pressed
			KeyCode[] possibleKeys = {KeyCode.A, KeyCode.S, KeyCode.D, KeyCode.W, KeyCode.Q, KeyCode.E, KeyCode.Space };
			
			// Loops through the keys
			foreach (KeyCode possibleKey in possibleKeys)
			{
				// If an valid key was pressed, then continue
				if (!Input.GetKey(possibleKey))
					continue;
				// Continues onto a function that updates the position on the server.
				CmdMoveOnServer(possibleKey);
			}
		}

		// Syncs the players state
		SyncState();

	}

	// Updates the position on the server.
	[Command]
	void CmdMoveOnServer(KeyCode pressedKey)
	{
		state = Move(state, pressedKey);
	}
}
