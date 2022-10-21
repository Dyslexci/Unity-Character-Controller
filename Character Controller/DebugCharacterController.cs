using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugCharacterController : MonoBehaviour
{
    FirstPersonMovementController controller;
    StateFactory stateFactory;
    [SerializeField]
    TMP_Text stateText;
    [SerializeField]
    TMP_Text speedText;
    [SerializeField]
    TMP_Text staminaText;
    [SerializeField]
    Slider staminaSlider;

    private void Awake()
    {
        controller = GetComponent<FirstPersonMovementController>();
        stateFactory = GetComponent<StateFactory>();
        staminaSlider.maxValue = controller.stamina;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        stateText.text = stateFactory.movementState.ToString();
        speedText.text = controller.currentSpeed.ToString();
        staminaText.text = controller.stamina.ToString();
        staminaSlider.value = controller.stamina;
    }
}
