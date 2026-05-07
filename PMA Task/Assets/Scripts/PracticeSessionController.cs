using UnityEngine; 
using System.Collections;
using StarterAssets;
using UnityEngine.SceneManagement;

public class PracticeSessionController : MonoBehaviour
{
    [Header("References")]
    public OreMiner oreMiner;
    public GameObject expectancyPanel;
    public FirstPersonController playerController;
    public UDPSender U;
    public string thankYouSceneName = "ThankYou";

    [Header("Timing")]
    public float expectancyDelay = 5f;

    private float miningTimer = 0f;
    private bool expectancyTriggered = false;
    private bool isExpectancyRunning = false;

    [Header("Practice Progress")]
    public int expectancyCount = 0;
    public int maxExpectancyTrials = 3;

    void Update()
    {
        HandleMiningTimer();

         if (Input.GetKeyDown(KeyCode.F1))
        {
            UDPSender.sendString("S");
        }

        if (Input.GetKeyDown(KeyCode.F2))
        {
            UDPSender.sendString("M");
        }

        if (Input.GetKeyDown(KeyCode.F3))
        {
            UDPSender.sendString("N");
        }

        if (Input.GetKeyDown(KeyCode.F4))
        {
            UDPSender.sendString("G");
        }

    //Escape scene to go back to options page

    //End the scene and go to thank you screen
    }

    private void HandleMiningTimer()
    {
        // Don’t accumulate time during expectancy UI
        if (isExpectancyRunning) return;

        if (oreMiner != null && oreMiner.IsMining)
        {
            miningTimer += Time.deltaTime;

            if (!expectancyTriggered && miningTimer >= expectancyDelay)
            {
                expectancyTriggered = true;
                StartCoroutine(DoExpectancy());
            }
        }
        else
        {
            // Reset if mining stops before reaching threshold
            miningTimer = 0f;
            expectancyTriggered = false;
        }
    }

    private IEnumerator DoExpectancy()
    {
        isExpectancyRunning = true;

        // Stop player control
        if (playerController != null)
            playerController.enabled = false;

        // Lock mining interactions
        if (oreMiner != null)
            oreMiner.isExpectancyActive = true;

        // Show expectancy UI
        expectancyPanel.SetActive(true);

        ExpectancyUI ui = expectancyPanel.GetComponent<ExpectancyUI>();
        ui.ResetExpectancy();

        // Prevent accidental input
        ui.enabled = false;
        yield return new WaitForSeconds(1f);
        ui.enabled = true;

        // Wait for participant response
        while (!ui.expectancySelected)
        {
            yield return null;
        }
        expectancyCount++;

        // Hide UI
        expectancyPanel.SetActive(false);

        // Restore gameplay
        if (playerController != null)
            playerController.enabled = true;

        yield return new WaitForSeconds(0.2f);
        if (oreMiner != null)
            oreMiner.isExpectancyActive = false;

        // Reset session tracking
        miningTimer = 0f;
        expectancyTriggered = false;
        isExpectancyRunning = false;

        // Decide whether to continue or end practice
        if (expectancyCount >= maxExpectancyTrials)
        {
            yield return new WaitForSeconds(0.5f);
            SceneManager.LoadScene(thankYouSceneName);
        }
    }
}
