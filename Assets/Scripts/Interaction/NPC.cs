using UnityEngine;

namespace Forest.Interaction
{
    public class NPC : MonoBehaviour, IInteractable
    {
        public void Interact()
        {
            print("NPC");
        }
    }
}

