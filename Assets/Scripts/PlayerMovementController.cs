using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] public Joystick joystick;
    [SerializeField] public FixedTouchField fixedTouchField;
    private RigidbodyFirstPersonController rigidBodyController;

    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        rigidBodyController = GetComponent<RigidbodyFirstPersonController>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rigidBodyController.joystickInputAxis.x = joystick.Horizontal;
        rigidBodyController.joystickInputAxis.y = joystick.Vertical;
        rigidBodyController.mouseLook.lookInputAxis = fixedTouchField.TouchDist;

        if (Mathf.Abs(joystick.Horizontal) > 0.9f || Mathf.Abs(joystick.Vertical) > 0.9f)
        {
            rigidBodyController.movementSettings.ForwardSpeed = 4;
            animator.SetBool("IsRunning", true);
        }
        else
        {
            rigidBodyController.movementSettings.ForwardSpeed = 2;
            animator.SetBool("IsRunning", false);
        }
        animator.SetFloat("Horizontal", joystick.Horizontal);
        animator.SetFloat("Vertical", joystick.Vertical);
        
        
    }
}
