using UnityEngine;

public class NPC : MonoBehaviour, Forest.Interaction.IInteractable
{
    public string Name;
    public Dialogue Dialogue;

    // Trigger dialogue for this NPC
    public void Interact()
    {
        Debug.Log("Speaking to NPC");
        DialogueManager.Instance.StartDialogue(Name, Dialogue.RootNode);
    }
}


