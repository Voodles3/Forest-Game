using UnityEngine;

public class NPC : MonoBehaviour, Forest.Interaction.IInteractable
{
    public string title;
    public string description;
    public Dialogue Dialogue;

    public void Interact()
    {
        Debug.Log("Speaking to NPC");
        DialogueManager.Instance.StartDialogue(title, description, Dialogue.RootNode);
    }
}


