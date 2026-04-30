using UnityEngine; 
using System.Collections;
using StarterAssets;

public class PracticeSessionController : MonoBehaviour
{
    [Header("References")]
    public OreMiner oreMiner;
    public GameObject expectancyPanel;
    public FirstPersonController playerController;

    [Header("Timing")]
    public float expectancyDelay = 5f;

    private float miningTimer = 0f;
    private bool expectancyTriggered = false;
    private bool isExpectancyRunning = false;

    void Update()
    {
        HandleMiningTimer();
    }

    private void HandleMiningTimer()
    {
        Debug.Log("IsMining: " + oreMiner.IsMining + " | Timer: " + miningTimer);
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

        // Hide UI
        expectancyPanel.SetActive(false);

        // Restore gameplay
        if (playerController != null)
            playerController.enabled = true;

        if (oreMiner != null)
            oreMiner.isExpectancyActive = false;

        // Reset session tracking
        miningTimer = 0f;
        expectancyTriggered = false;
        isExpectancyRunning = false;
    }
}
