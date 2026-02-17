using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ExpectancyUI : MonoBehaviour
{
    public TextMeshProUGUI expectancyText;
    public Slider expectancySlider;

    [HideInInspector] public int expectancyValue = 5;   // start at 5
    [HideInInspector] public bool expectancySelected = false;

    //Controlling the speed of moving the slider
    private float keyHoldTimer = 0f;
    public float keyHoldDelay = 0.15f; // Adjust this value to control speed

    void Start()
    {
        expectancySelected = false;

        // Configure slider
        expectancySlider.minValue = 0;
        expectancySlider.maxValue = 10;
        expectancySlider.wholeNumbers = true;
        expectancySlider.value = 5;
        expectancySlider.interactable = false; // Disable mouse interaction

        UpdateExpectancyText();
    }

    void Update()
    {
        if (!expectancySelected)
        {
            if (Input.GetKey(KeyCode.Alpha1))
            {
                keyHoldTimer += Time.deltaTime;
                if (keyHoldTimer >= keyHoldDelay)
                {
                    expectancyValue = Mathf.Max(0, expectancyValue - 1);
                    expectancySlider.value = expectancyValue;
                    keyHoldTimer = 0f;
                }
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                keyHoldTimer += Time.deltaTime;
                if (keyHoldTimer >= keyHoldDelay)
                {
                    expectancyValue = Mathf.Min(10, expectancyValue + 1);
                    expectancySlider.value = expectancyValue;
                    keyHoldTimer = 0f;
                }
            }
            else
            {
                keyHoldTimer = 0f;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                expectancySelected = true;
                Debug.Log("Expectancy confirmed: " + expectancyValue);
            }
        }
    }

    void UpdateExpectancyText()
    {
        expectancyText.text = "Warning: Lightning storm approaches. \nOn a scale from only seeking shelter to only mining, where do you stand?";
    }

    public void ResetExpectancy()
    {
        expectancyValue = 5; // reset to middle each trial
        expectancySelected = false;
        expectancySlider.value = 5;
        UpdateExpectancyText();
    }
}