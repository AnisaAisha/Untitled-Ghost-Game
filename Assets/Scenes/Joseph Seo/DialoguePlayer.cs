using UnityEngine;
using Yarn.Unity;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(DialogueRunner))]
public class DialoguePlayer : MonoBehaviour
{
    public static DialoguePlayer Instance { get; private set; }
    [SerializeField] private DialogueRunner dialogueRunner;
    [SerializeField] private FancyDialogue fd;

    private string currentOrder;
    private int storyNum;
    private int seatNum = -1;

    private bool isSuccess = false;
    private bool isDeleting = false;

    

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    void Start() {
        dialogueRunner.AddCommandHandler<string, string>("play", PlayAnimation);
        dialogueRunner.AddCommandHandler<string>("setCam", SetCamera);
        dialogueRunner.AddCommandHandler<string>("startStory", StartStoryDialogue);
        dialogueRunner.AddCommandHandler<string, string>("setNext", SetNextDialogue);
        dialogueRunner.AddCommandHandler("end", EndDialogue);
        dialogueRunner.AddCommandHandler("reset", Reset);
        dialogueRunner.AddCommandHandler<bool>("reaperPitch", ReaperPitch);

        // dialogueRunner.onDialogueComplete += OnDialogueComplete;
        dialogueRunner.onDialogueComplete.AddListener(OnDialogueComplete);

    }

    public static void PlayAnimation(string name, string animation) {
        Debug.Log("Searching for ghost by then name" + name);
        GameObject target = GhostSpawningManager.Instance.GetSpawnedGhost(name);
        if (target == null) Debug.Log("Target is null");
        target.GetComponent<Animator>().Play(animation);
    }

    [YarnFunction("GetOrder")]
    public static string GetOrder() {
        // int selectedIndex = (int) (Random.value * 3);
        // string[] names = {"tea", "coffee", "milk"};
        // return names[selectedIndex];
        return DialoguePlayer.Instance.currentOrder;
    }

    // [YarnFunction("GetStoryNum")]
    // public static int GetStoryNum()
    // {
    //     return DialoguePlayer.Instance.storyNum;
    // }

    public void Reset() {
        fd.Reset();
    }

    public void ReaperPitch(bool val)
    {
        AudioManager.Instance.SetReaperPitch(val);
    }

    // This function physically hurts me to write
    // Also outdated with other camera changes
    public static void SetCamera(string cameraName)
    {
        Transform cameras = CameraManager.Instance.transform;
        Transform camToSwitchTo = cameras.Find(cameraName);
        if (camToSwitchTo)
        {
            for (int i = 0; i < cameras.childCount; i++)
            {
                Transform cam = cameras.GetChild(i);
                if (!cam.name.Equals(cameraName))
                {
                    cam.gameObject.SetActive(false);
                }
            }
            camToSwitchTo.gameObject.SetActive(true);
        }
    }

    public void StartStoryDialogue(string storyToStart) {
        Debug.Log(storyToStart);
        StartCoroutine(StoryDialogue(storyToStart));
    }

    private IEnumerator StoryDialogue(string storyTitle) {
        yield return new WaitForSeconds(0.01f);
        dialogueRunner.StartDialogue(DialogueManager.Instance.GetNextDialogue(storyTitle));
    }

    public void SetNextDialogue(string name, string dialogueNode) {
        // GameManager.Instance.state = State.Dialogue;
        DialogueManager.Instance.SetNextDialogue(name, dialogueNode);
    }

    public void EndDialogue()
    {
        CameraManager.Instance.SwapToMainCamera();
        Debug.Log("SeatNumber is " + seatNum);
        GhostSpawningManager.Instance.DeleteSpawnedGhost(seatNum);
        GameManager.Instance.orderManager.RemoveCompletedOrder();
        GameManager.Instance.state = State.Main;
    }

    // Specific Order Dialogue
    // Function called to queue up the order dialogue
    public void StartOrderDialogue(string ghostName, string recipe, int seatNum) {
        this.seatNum = seatNum;
        this.currentOrder = recipe;
        CameraManager.Instance.SwapToSeatCamera(seatNum);
        if (ghostName.Contains(" Ghost"))
        {
            ghostName = ghostName.Replace(" Ghost", "");
        }
        ghostName = ghostName.Replace(" ", "");
        dialogueRunner.StartDialogue(ghostName + "Order");
        GameManager.Instance.state = State.Dialogue;
    }

    public void CompleteOrderDialogue(string ghostName, int seatNum, bool res) {
        CameraManager.Instance.SwapToSeatCamera(seatNum);
        this.seatNum = seatNum;
        string parsedName = ghostName.Replace(" Ghost", "");
        parsedName = parsedName.Replace(" ", "");
        
        if (res) {
            isSuccess = true;
            Debug.Log("Starting dialogue: " + parsedName + "Success");
            dialogueRunner.StartDialogue(parsedName + "Success");
        } else {
            isDeleting = true;
            dialogueRunner.StartDialogue(parsedName + "Failure");
        }
    }

    //i feel like there is an easier way to do this... but alas...
    private void OnDialogueComplete()
    {
        if (isDeleting)
        {
            GameManager.Instance.ghostManager.IncrementStoryIndex(GameManager.Instance.orderManager.GetCurrActiveOrderName());
            if (GameManager.Instance.orderManager.GetCurrActiveOrderName() == "Reaper")
            {
                GameManager.Instance.UpdateArc();
            }
            GameManager.Instance.orderManager.RemoveCompletedOrder();
            isDeleting = false;
            isSuccess = false;
            GameManager.Instance.state = State.Main;
        }
        else if (isSuccess)
        {
            isDeleting = true;
        }
        else
        {
            GameManager.Instance.state = State.Main;
        }
    }
}
