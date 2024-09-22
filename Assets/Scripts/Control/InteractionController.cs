using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Forest.Interaction
{
    interface IInteractable
    {
        public void Interact();
    }

    public class InteractionController : MonoBehaviour
    {
        [SerializeField] float maxInteractionDistance;

        PlayerActions actions;
        InputAction interactAction;

        void Awake()
        {
            actions = new();
            interactAction = actions.Gameplay.Interact;
        }

        void Update()
        {
            if (interactAction.triggered)
            {
                Interact();
            }
        }

        void Interact()
        {
            Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance))
            {
                if (hit.transform.TryGetComponent(out IInteractable interactable))
                {
                    interactable.Interact();
                }
            }
        }

        void OnEnable()
        {
            actions.Gameplay.Enable();
        }

        void OnDisable()
        {
            actions.Gameplay.Disable();
        }
    }
}

