using UnityEngine;
using UnityEngine.InputSystem;

namespace Forest.Interaction
{
    interface IInteractable
    {
        public void Interact();
        //public void OnHover(bool hovering);
    }

    public class InteractionController : MonoBehaviour
    {
        [SerializeField] float maxInteractionDistance;

        PlayerActions actions;
        InputAction interactAction;
        IInteractable currentInteractable;

        void Awake()
        {
            actions = new();
            interactAction = actions.Gameplay.Interact;
        }

        void Update()
        {
            CastInteractionRay();
            if (interactAction.triggered)
            {
                Interact();
            }
        }

        void CastInteractionRay()
        {
            Ray ray = new(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance))
            {
                if (hit.transform.TryGetComponent(out IInteractable interactable))
                {
                    currentInteractable = interactable;
                    //interactable.OnHover(true);
                    return;
                }
            }
            //currentInteractable?.OnHover(false);
            currentInteractable = null;
        }

        void Interact()
        {
            currentInteractable?.Interact();
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

