using UnityEngine;

public class NPC : MonoBehaviour, Forest.Interaction.IInteractable
{
    public string title;
    public Color titleColor;
    public string description;
    public string[] initialDialogues = 
    {
        "How ya doin'?",
        "What's happening?",
        "What's goin' on?",
        "Good to see ya!"
    };
    public float initialDialogueDisplayTime;
    public Dialogue Dialogue;

    public void Interact()
    {
        Debug.Log("Speaking to NPC");
        DialogueManager.Instance.StartDialogue(this, Dialogue.RootNode);
    }
}


