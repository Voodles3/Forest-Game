using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Forest.Interaction
{
    public interface IInteractable
    {
        public void Interact();
    }

    public class InteractionController : MonoBehaviour
    {
        public static InteractionController Instance { get; private set; }

        [SerializeField] float maxInteractionDistance;
        [SerializeField] LayerMask interactable;

        PlayerActions actions;
        InputAction interactAction;
        IInteractable currentInteractable;
        GameObject currentHoveredObject;

        public GameObject CurrentHoveredObject
        {
            get { return currentHoveredObject; }
        }

        void Awake()
        {
            Singleton();

            actions = new();
            interactAction = actions.Gameplay.Interact;
        }
        
        void Singleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
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
            if (Physics.Raycast(ray, out RaycastHit hit, maxInteractionDistance, interactable))
            {
                if (hit.transform.TryGetComponent(out IInteractable interactable))
                {
                    currentInteractable = interactable;
                    currentHoveredObject = hit.transform.gameObject;
                    return;
                }
            }
            currentInteractable = null;
            currentHoveredObject = null;
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

