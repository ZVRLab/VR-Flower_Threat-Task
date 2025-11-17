using UnityEngine;
using TMPro;

public class ExpectancyUI : MonoBehaviour
{
    public TextMeshProUGUI expectancyText;

    [HideInInspector] public int expectancyValue = 5;   // start at 5
    [HideInInspector] public bool expectancySelected = false;

    void Start()
    {
        expectancySelected = false;
        UpdateExpectancyText();
    }

    void Update()
    {
        if (!expectancySelected)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                expectancyValue = Mathf.Max(0, expectancyValue - 1);
                UpdateExpectancyText();
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                expectancyValue = Mathf.Min(9, expectancyValue + 1);
                UpdateExpectancyText();
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
        expectancyText.text = "How likely are you to mine?: " + expectancyValue.ToString();
    }

    public void ResetExpectancy()
    {
        expectancyValue = 5; // reset to middle each trial
        expectancySelected = false;
        UpdateExpectancyText();
    }
}
