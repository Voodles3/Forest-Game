using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] Color disabledResponseColor;

    [Header("References")]
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
        StartCoroutine(DisplayDialogueText(npc.initialDialogueDisplayTime, 0f, npc.initialResponses[Random.Range(0, npc.initialResponses.Length)]));
 
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

    public void ContinueDialogue(DialogueNode node)
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
        foreach (Transform child in responseButtonContainer)
        {
            Button button = child.GetComponent<Button>();
            button.interactable = false;
            button.GetComponentInChildren<TextMeshProUGUI>().color = disabledResponseColor;
        }

        if (currentDialogueTextCoroutine != null) { StopCoroutine(currentDialogueTextCoroutine); }
        currentDialogueTextCoroutine = 
        StartCoroutine(DisplayDialogueText(dialogueNode.displayTime, dialogueNode.responseDelay, dialogueNode.dialogueText, dialogueNode));

        if (dialogueNode.HasNoResponses())
        {
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

        if (GetDialogueActive() && dialogueNode != null) { ContinueDialogue(dialogueNode); }

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

    public void ExitDialogue()
    {
        dialogBoxParent.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
 
    void ShowDialogue()
    {
        dialogBoxParent.SetActive(true);
    }
 
    public bool GetDialogueActive()
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