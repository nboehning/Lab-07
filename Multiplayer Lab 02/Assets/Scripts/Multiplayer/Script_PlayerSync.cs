﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Script_PlayerSync : NetworkBehaviour
{

    [SyncVar]
    Vector3 syncedPosition;
    [SyncVar]
    Quaternion syncedRotation;

    #region variables
    [Header("\tReference Values")]
    public Transform myTransform;

    [Header("\tValues for Client Management")]
    [Header("Player")]
    public Rigidbody myRigidbody;
    public CapsuleCollider myCollider;
    public UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController myController;
    [Header("Camera")]
    public GameObject myCameraObject;
    public Camera myCamera;
    public AudioListener myListener;

    [Header("\tSync Values")]
    [Header("Rotation")]
    public float rotationLerpRate = 15f;
    public float rotationThreshold = 5f;
    [Header("Position")]
    public float positionLerpRate = 15f;
    public float positionThreshold = 0.3f;

    Quaternion lastPlayerRotation;
    Vector3 lastPlayerPosition;
    #endregion

    void Start()
    {
        if (!isLocalPlayer)
        {
            Destroy(myController);
            Destroy(myRigidbody);
            Destroy(myCollider);
            Destroy(myCameraObject);
        }
    }

    void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            TransmitRotation();
            TransmitPosition();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            LerpRotation();
            LerpPosition();
        }
    }

    #region rotation
    [Client]
    void TransmitRotation()
    {
        if (Quaternion.Angle(myTransform.rotation, lastPlayerRotation) > rotationThreshold)
        {
            CmdSendRotationToServer(myTransform.rotation);
            lastPlayerRotation = myTransform.rotation;
        }

    }

    [Command]
    void CmdSendRotationToServer(Quaternion rotationToSend)
    {
        //Debug.Log("Sync rotation!");

        syncedRotation = rotationToSend;
    }


    void LerpRotation()
    {
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, syncedRotation, Time.deltaTime * rotationLerpRate);
    }
    #endregion

    #region position
    [Client]
    void TransmitPosition()
    {

    }

    [Command]
    void CmdSendPositionToServer(Vector3 positionToSend)
    {

    }

    void LerpPosition()
    {

    }
    #endregion
}