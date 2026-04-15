using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ScavengerHuntManager : MonoBehaviour
{
    [Header("Rules")]
    public int requiredCount = 5;
    public float timeLimitSeconds = 180f; // 3 minutes

    [Header("References")]
    public Transform xrCamera;           
    public GameObject worldMessagePrefab; 

    private float timeRemaining;
    private int depositedCount;
    private bool finished;

    
    private TextMesh messageText;

    void Start()
    {
        timeRemaining = timeLimitSeconds;

       
        GameObject msg = new GameObject("WorldMessage");
        msg.transform.localScale = Vector3.one * 0.01f; 
        messageText = msg.AddComponent<TextMesh>();
        messageText.text = "";
        messageText.fontSize = 120;
        messageText.anchor = TextAnchor.MiddleCenter;
    }

    void Update()
    {
        if (finished) { FollowCamera(); return; }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            Finish(false);
        }

        FollowCamera();
       
        messageText.text = $"Items: {depositedCount}/{requiredCount}\nTime: {Mathf.CeilToInt(timeRemaining)}s";
    }

    private void FollowCamera()
    {
        if (!xrCamera) return;


        Vector3 pos = xrCamera.position
            + xrCamera.forward * 1.0f   
            + xrCamera.up * 0.3f;       
        messageText.transform.position = pos;

       
        messageText.transform.rotation = Quaternion.LookRotation(messageText.transform.position - xrCamera.position);
    }

    public void OnItemDeposited()
    {
        if (finished) return;

        depositedCount++;
        if (depositedCount >= requiredCount)
        {
            Finish(true);
        }
    }

    private void Finish(bool won)
    {
        finished = true;
        messageText.text = won ? "YOU WON!" : "YOU LOST!";
        messageText.characterSize = 0.2f;
        messageText.fontSize = 200;
    }
}