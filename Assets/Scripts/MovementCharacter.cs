using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementCharacter : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField]
    private float speed = 25.0F;
    [SerializeField]
    private float jumpSpeed = 8.0F;
    [SerializeField]
    private float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    private float turner;
    private float looker;
    [SerializeField]
    private float sensitivity = 5;
    [SerializeField]
    private float LookContraint;
    float y = 0;
    private float updown = 0;
    [SerializeField]
    private bool Locked = false;
    [SerializeField]
    private Raycaster raycaster;

    // Use this for initialization
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
    }

    public void ToggleLock()
    {
        Locked = !Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Locked)
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
                if (Input.GetButton("Jump") && controller.isGrounded)
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

            if (Input.GetMouseButtonDown(0))
            {
                raycaster.RayCast();
            }

            moveDirection.y = y;
            //Applying gravity to the controller
            moveDirection.y -= gravity * Time.deltaTime;

            y = moveDirection.y;
            //Making the character move
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
}
