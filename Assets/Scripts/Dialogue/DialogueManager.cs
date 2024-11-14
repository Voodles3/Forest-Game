using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
 
    public GameObject dialogBoxParent;
    public GameObject dialogueTextParent;
    public TextMeshProUGUI dialogTitleText, dialogDescText, dialogueText;
    public GameObject responseButtonPrefab;
    public Transform responseButtonContainer;

    Coroutine currentDialogueTextCoroutine;

    PlayerActions actions;
    InputAction exitMenuAction;
 
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

        actions = new();
        exitMenuAction = actions.Gameplay.ExitMenu;

        HideDialogue();
    }

    public void StartDialogue(NPC npc, DialogueNode node)
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        ShowDialogue();

        dialogTitleText.text = npc.title;
        dialogTitleText.color = npc.titleColor;
        dialogDescText.text = npc.description;

        // Display a random initial dialogue line
        if (currentDialogueTextCoroutine != null) { StopCoroutine(currentDialogueTextCoroutine); }
        currentDialogueTextCoroutine = 
        StartCoroutine(DisplayDialogueText(npc.initialDialogueDisplayTime, 0f, npc.initialDialogues[Random.Range(0, npc.initialDialogues.Length - 1)]));
 
        // Remove any existing response buttons
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }
 
        // Create and set up response buttons based on current dialogue node
        foreach (DialogueResponse response in node.responses)
        {
            GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = response.responseText;
 
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnResponseClicked(response.nextNode));
        }
    }

    public void StartDialogue(DialogueNode node) // This overload will be called after the Dialog menu is already open and a button has been pressed
    {
        foreach (Transform child in responseButtonContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (DialogueResponse response in node.responses)
        {
            GameObject buttonObj = Instantiate(responseButtonPrefab, responseButtonContainer);
            buttonObj.GetComponentInChildren<TextMeshProUGUI>().text = response.responseText;
 
            buttonObj.GetComponent<Button>().onClick.AddListener(() => OnResponseClicked(response.nextNode));
        }
    }

    public void OnResponseClicked(DialogueNode dialogueNode)
    {
        // Check if there's a follow-up node
        if (!dialogueNode.HasNoResponses())
        {
            if (currentDialogueTextCoroutine != null) { StopCoroutine(currentDialogueTextCoroutine); }
            currentDialogueTextCoroutine = 
            StartCoroutine(DisplayDialogueText(dialogueNode.displayTime, dialogueNode.responseDelay, dialogueNode.dialogueText, dialogueNode));
        }
        else
        {
            if (currentDialogueTextCoroutine != null) { StopCoroutine(currentDialogueTextCoroutine); }
            currentDialogueTextCoroutine = 
            StartCoroutine(DisplayDialogueText(dialogueNode.displayTime, dialogueNode.responseDelay, dialogueNode.dialogueText, dialogueNode));
            ExitDialogue();
        }
    }

    IEnumerator DisplayDialogueText(float displayTime, float responseDelay, string text, DialogueNode dialogueNode = null)
    {
        dialogueTextParent.SetActive(true);
        dialogueText.text = text;

        float endTime = Time.time + responseDelay;
        while (Time.time <= endTime)
        {
            yield return null;
        }

        if (IsDialogueActive() && dialogueNode != null) { StartDialogue(dialogueNode); }

        endTime = Time.time + (displayTime - responseDelay);
        while (Time.time <= endTime)
        {
            yield return null;
        }

        dialogueTextParent.SetActive(false);
    }
 
    void HideDialogue()
    {
        dialogBoxParent.SetActive(false);
        dialogueTextParent.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnExitDialogue(InputAction.CallbackContext ctx) => ExitDialogue();

    void ExitDialogue()
    {
        dialogBoxParent.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    void ShowDialogue()
    {
        dialogBoxParent.SetActive(true);
    }
 
    public bool IsDialogueActive()
    {
        return dialogBoxParent.activeSelf;
    }

    void OnEnable()
    {
        actions.Gameplay.Enable();
        exitMenuAction.started += OnExitDialogue;
    }

    void OnDisable()
    {
        actions.Gameplay.Disable();
    }
}