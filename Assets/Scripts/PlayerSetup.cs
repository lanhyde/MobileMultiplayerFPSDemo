using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class PlayerSetup : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] fps_hands_childGameObjects;
    [SerializeField] private GameObject[] soldier_childGameObjects;

    [SerializeField] private GameObject playerUIPrefab;

    private PlayerMovementController playerMovementController;

    public Camera fpsCamera;

    private Animator animator;

    private Shooting shooter;
    // Start is called before the first frame update
    void Start()
    {
        shooter = GetComponent<Shooting>();
        animator = GetComponent<Animator>();
        playerMovementController = GetComponent<PlayerMovementController>();
        if (photonView.IsMine)
        {
            foreach (var gameObject in fps_hands_childGameObjects)
            {
                gameObject.SetActive(true);
            }

            foreach (var gameObject in soldier_childGameObjects)
            {
                gameObject.SetActive(false);
            }

            GameObject playerUIGameObject = Instantiate(playerUIPrefab);
            playerMovementController.joystick =
                playerUIGameObject.transform.Find("Fixed Joystick").GetComponent<Joystick>();
            playerMovementController.fixedTouchField = playerUIGameObject.transform.Find("RotationTouchField")
                .GetComponent<FixedTouchField>();
            
            playerUIGameObject.transform.Find("FireButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                shooter.Fire();
            });
            fpsCamera.enabled = true;
            animator.SetBool("IsSoldier", false);
        }
        else
        {
            foreach (var gameObject in fps_hands_childGameObjects)
            {
                gameObject.SetActive(false);
            }

            foreach (var gameObject in soldier_childGameObjects)
            {
                gameObject.SetActive(true);
            }

            playerMovementController.enabled = false;
            GetComponent<RigidbodyFirstPersonController>().enabled = false;
            fpsCamera.enabled = false;
            
            animator.SetBool("IsSoldier", true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
