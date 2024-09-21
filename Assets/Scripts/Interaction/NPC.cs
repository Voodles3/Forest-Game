using System.Collections;
using System.Collections.Generic;
using System.IO.Pipes;
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

