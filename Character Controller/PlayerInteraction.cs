using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Tooltip("Distance at which the player can interact with objects")]
    public float interactionDistance = 3f;
    [Tooltip("Layers where interactable objects can be found")]
    public LayerMask interactableLayers;
    [Tooltip("Text displayed on-screen with a tooltip/description")]
    public TMP_Text interactableText;
    [Tooltip("Something")]
    public GameObject interactionHold;
    [Tooltip("Image displaying progress bar for held interaction types")]
    public Image interactionProgress;
    private Camera cam;
    private PlayerInputs inputs;
    private bool interactKeyPressed;
    FirstPersonMovementController controller;

    private void Awake()
    {
        cam = GetComponentInChildren<Camera>();
        inputs = GetComponent<PlayerInputs>();
        interactionHold.SetActive(false);
        controller = GetComponent<FirstPersonMovementController>();
    }

    void FixedUpdate()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance, interactableLayers))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                HandleInteraction(interactable);
                interactableText.text = interactable.GetDescription();
                interactionHold.SetActive(interactable.interactionType == Interactable.InteractionType.Hold);
            }
            return;
        }
        interactableText.text = "";
        interactionHold.SetActive(false);
        inputs.interact = false;
    }

    private void HandleInteraction(Interactable interactable)
    {
        if (inputs.interact)
        {
            if (interactKeyPressed)
            {
                inputs.interact = false;
                interactable.ResetHoldTime();
                return;
            }
            switch (interactable.interactionType)
            {
                case Interactable.InteractionType.Click:
                    interactable.Interact(controller);
                    inputs.interact = false;
                    break;
                case Interactable.InteractionType.Hold:
                    interactable.IncreaseHoldTime();
                    if (interactable.GetHoldTime() > 1f)
                    {
                        interactable.Interact(controller);
                        interactable.ResetHoldTime();
                        inputs.interact = false;
                        interactKeyPressed = false;
                    }
                    interactionProgress.fillAmount = interactable.GetHoldTime();
                    break;

                default:
                    throw new Exception("Unsupported interactable type.");
            }
        } else
        {
            interactable.ResetHoldTime();
        }
    }
}
