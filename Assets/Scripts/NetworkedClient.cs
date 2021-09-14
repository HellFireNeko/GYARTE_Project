using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;

[Serializable]
public enum PlayerRole
{
    Hunter,
    Runner
}

[RequireComponent(typeof(CharacterController))]
public class NetworkedClient : NetworkBehaviour
{
    private CharacterController controller;

    [SerializeField] private float speed = 25.0F;
    [SerializeField] private float jumpSpeed = 8.0F;
    [SerializeField] private float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    private float turner;
    private float looker;
    [SerializeField] private float sensitivity = 5;
    [SerializeField] private float LookContraint;
    private float y = 0;
    private float updown = 0;
    [SerializeField] private bool Locked = false;

    [SerializeField] private bool GravityEnable = true;

    private bool IsActivePlayer = false;

    private NetworkVariableInt Health = new NetworkVariableInt(new NetworkVariableSettings() { WritePermission = NetworkVariablePermission.Everyone }, 3);
    private bool Alive { get { return Health.Value > 0; } }

    [SerializeField] private NetworkVariable<PlayerRole> Role = new NetworkVariable<PlayerRole>(new NetworkVariableSettings() { WritePermission = NetworkVariablePermission.Everyone });

    [SerializeField] private bool RandomizeColor;

    [SerializeField] private Camera PlayerCamera;
    private Camera MainCamera;

    private NetworkVariableColor playerColor = new NetworkVariableColor(new NetworkVariableSettings() { WritePermission = NetworkVariablePermission.Everyone });

    [ClientRpc]
    public void SetRoleClientRpc(string role)
    {
        if (Enum.TryParse(role, out PlayerRole r))
            Role.Value = r;
    }

    // Use this for initialization
    void Start()
    {
        MainCamera = Camera.main;
        playerColor.OnValueChanged += (a, b) => { GetComponent<MeshRenderer>().material.color = b; };
        Role.OnValueChanged += (old, val) => { print("Updated"); };
        controller = GetComponent<CharacterController>();
        Health.OnValueChanged += ListenChange;
        GenerateMyColorServerRpc();
        GetComponent<MeshRenderer>().material.color = playerColor.Value;
    }

    [ServerRpc]
    private void GenerateMyColorServerRpc()
    {
        if (!IsServer || !RandomizeColor) return;

        playerColor.Value = UnityEngine.Random.ColorHSV();
    }

    [ServerRpc]
    private void SetHealthServerRpc(int health)
    {
        if (!IsServer) return;

        Health.Value = health;
    }

    [ClientRpc]
    public void InitClientRpc()
    {
        SetHealthServerRpc(3);
        MainCamera.enabled = false;
        if (IsLocalPlayer)
        {
            IsActivePlayer = true;

            Cursor.lockState = CursorLockMode.Locked;

            PlayerCamera.enabled = true;

            ToggleLock();
        }
    }

    [ClientRpc]
    public void ReturnToLobbyClientRpc()
    {
        PlayerCamera.enabled = false;
        MainCamera.enabled = true;
    }

    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void ToggleLock()
    {
        Locked = !Locked;
    }

    [ServerRpc(RequireOwnership = false)]
    public void HitEventServerRpc()
    {
        if (!IsServer) return;

        Health.Value -= 1;
    }

    void ListenChange(int old, int newval)
    {
        Debug.Log($"Client {NetworkObjectId}, lost hp now at {newval}", this);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) { return; }

        if (!Locked && IsActivePlayer && (Alive || Role.Value == PlayerRole.Hunter))
        {
            // is the controller on the ground?
            if (true)
            {
                //Feed moveDirection with input.
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                moveDirection = transform.TransformDirection(moveDirection);
                //Multiply it by speed.
                moveDirection *= speed;
                //Jumping
                if (Input.GetButton("Jump") && controller.isGrounded && Role.Value == PlayerRole.Runner)
                    moveDirection.y = y = jumpSpeed;
            }

            turner = Input.GetAxis("Mouse X") * sensitivity;
            looker = -Input.GetAxis("Mouse Y") * sensitivity;
            if (turner != 0)
            {
                //Code for action on mouse moving right
                transform.eulerAngles += new Vector3(0, turner, 0);
            }
            if (looker != 0)
            {
                //Code for action on mouse moving right
                updown += looker;
                updown = Mathf.Clamp(updown, -LookContraint, LookContraint);
                Camera.main.transform.rotation = Quaternion.Euler(updown, transform.eulerAngles.y, 0);
            }

            moveDirection.y = y;
            //Applying gravity to the controller
            moveDirection.y -= gravity * Time.deltaTime;

            y = moveDirection.y;
            //Making the character move
            controller.Move(moveDirection * Time.deltaTime);

            if (Role.Value == PlayerRole.Hunter && Input.GetMouseButtonDown(0))
            {
                var s = Physics.Raycast(Camera.main.transform.position, Camera.main.transform.rotation.eulerAngles, out RaycastHit hit, 5f, 3);
                if (s)
                {
                    hit.collider.gameObject.GetComponent<NetworkedClient>().HitEventServerRpc();
                }
            }
        }
        if (GravityEnable && Locked)
        {
            moveDirection.y = y;
            //Applying gravity to the controller
            moveDirection.y -= gravity * Time.deltaTime;

            y = moveDirection.y;
        }
    }
}
