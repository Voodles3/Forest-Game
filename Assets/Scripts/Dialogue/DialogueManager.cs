using UnityEngine;
using UnityEngine.UI;
using TMPro;
 
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
 
    public GameObject dialogueParent; // Main container for dialogue UI
    public TextMeshProUGUI dialogTitleText, dialogDescText; // Text components for title and body
    public GameObject responseButtonPrefab; // Prefab for generating response buttons
    public Transform responseButtonContainer; // Container to hold response buttons
 
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        HideDialogue();
    }

    public void StartDialogue(string title, string desc, DialogueNode node)
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        ShowDialogue();

        dialogTitleText.text = title;
        dialogDescText.text = desc;
        //DialogBodyText.text = node.dialogueText;
 
        // Remove any existing response buttons
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }
 
        // Create and setup response buttons based on current dialogue node
        foreach (DialogueResponse response in node.responses)
        {
            GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = response.responseText;
 
            buttonObj.GetComponent<Button>().onClick.AddListener(() => SelectResponse(title, desc, response));
        }
    }

    public void SelectResponse(string title, string desc, DialogueResponse response)
    {
        // Check if there's a follow-up node
        if (!response.nextNode.IsLastNode())
        {
            StartDialogue(title, desc, response.nextNode); // Start next dialogue
        }
        else
        {
            // If no follow-up node, end the dialogue
            HideDialogue();
        }
    }
 
    void HideDialogue()
    {
        dialogueParent.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    void ShowDialogue()
    {
        dialogueParent.SetActive(true);
    }
 
    public bool IsDialogueActive()
    {
        return dialogueParent.activeSelf;
    }
}