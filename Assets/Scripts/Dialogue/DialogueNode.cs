using System.Collections.Generic;
 
[System.Serializable]
public class DialogueNode
{
    public string dialogueText;
    public float responseDelay;
    public float displayTime;
    public List<DialogueResponse> responses;
 
    internal bool HasNoResponses()
    {
        return responses.Count <= 0;
    }
}