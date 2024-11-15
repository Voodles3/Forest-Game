using UnityEngine;

public class NPC : MonoBehaviour, Forest.Interaction.IInteractable
{
    public string title;
    public Color titleColor;
    public string description;
    public string[] initialResponses = {};
    public float initialDialogueDisplayTime;
    public Dialogue Dialogue;

    public void Interact()
    {
        if (!DialogueManager.Instance.GetDialogueActive())
        {
            Debug.Log("Speaking to NPC");
            DialogueManager.Instance.StartDialogue(this, Dialogue.RootNode);
        }
        else
        {
            DialogueManager.Instance.ExitDialogue();
        }
    }
}


