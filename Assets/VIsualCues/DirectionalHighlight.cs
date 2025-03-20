using UnityEngine;
using UnityEngine.SceneManagement; // For scene transitions
using System.Collections;
using System.Collections.Generic;
using MidiJack;
using System.IO;
using System.Diagnostics;

public class DirectionalHighlight : MonoBehaviour
{
    [Header("References")]
    public CasiotoneKeyLayout keyLayout;
    public GameObject arrow;
    public GameObject nextTaskText; // UI text for "Next Task" message

    [Header("Highlight Colors")]
    public Color keyHighlightColor = Color.red;

    private int currentKeyIndex = 36; // Start at MIDI note 36 (C2)
    private int activeKey = -1;
    private float keyHighlightTime;
    private string csvFilePath = "";

    private int keysPerTask = 5; // Number of keys per task
    private int keysPressed = 0; // Track keys played in current set

    void Start()
    {
        // Set CSV file path
        csvFilePath = PlayerPrefs.GetString("csvFilePath", Application.persistentDataPath + "/reaction_times.csv");

        // Create the CSV file and write headers if it doesn't exist
        if (!File.Exists(csvFilePath))
        {
            WriteToCSV("MIDI Note,Key Highlight Time,Key Press Time,Reaction Time (Seconds),Accuracy,Timestamp", true);
        }

#if UNITY_EDITOR
        OpenCSVFile();
#endif

        StartCoroutine(WaitForKeysAndHighlight());
    }

    IEnumerator WaitForKeysAndHighlight()
    {
        while (keyLayout.GetKeyObject(currentKeyIndex) == null)
        {
            UnityEngine.Debug.Log("Waiting for keyboard to initialize...");
            yield return null;
        }

        UnityEngine.Debug.Log("Keyboard initialized, highlighting first key.");
        HighlightNextKey(currentKeyIndex);
        MidiMaster.noteOnDelegate += OnNoteOn;
    }

    void OnDestroy()
    {
        MidiMaster.noteOnDelegate -= OnNoteOn;
    }

    void HighlightNextKey(int index)
    {
        UnityEngine.Debug.Log("Highlighting MIDI Note: " + index);

        if (activeKey != -1)
        {
            keyLayout.HighlightKey(activeKey, Color.white);
        }

        activeKey = index;
        keyLayout.HighlightKey(activeKey, keyHighlightColor);
        keyHighlightTime = Time.time;

        UpdateArrowPosition();
    }

    void UpdateArrowPosition()
    {
        if (arrow == null || activeKey == -1) return;

        GameObject keyObject = keyLayout.GetKeyObject(activeKey);
        if (keyObject != null)
        {
            arrow.transform.position = keyObject.transform.position + new Vector3(0, 0.05f, 0);
            arrow.transform.LookAt(keyObject.transform.position + new Vector3(0, 0, 1));

            int nextKeyIndex = currentKeyIndex + 1;
            GameObject nextKeyObject = keyLayout.GetKeyObject(nextKeyIndex);

            if (nextKeyObject != null)
            {
                float direction = nextKeyObject.transform.position.x - keyObject.transform.position.x;

                if (direction < 0)
                {
                    arrow.transform.rotation = Quaternion.Euler(0, 0, 0);
                }
                else
                {
                    arrow.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
            }
        }
    }

    void OnNoteOn(MidiChannel channel, int note, float velocity)
    {
        float keyPressTime = Time.time;
        float reactionTime = keyPressTime - keyHighlightTime;
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string accuracy;

        if (note == activeKey)
        {
            UnityEngine.Debug.Log("Correct Key Pressed: " + note);
            keyLayout.HighlightKey(note, Color.white);
            accuracy = "Correct";

            keysPressed++; // Track how many keys are pressed

            if (keysPressed >= keysPerTask)
            {
                StartCoroutine(NextTaskSequence());
            }
            else
            {
                currentKeyIndex++;
                HighlightNextKey(currentKeyIndex);
            }
        }
        else
        {
            UnityEngine.Debug.Log("Wrong Key Pressed: " + note);
            accuracy = "Incorrect";
        }

        WriteToCSV($"{note},{keyHighlightTime},{keyPressTime},{reactionTime},{accuracy},{timestamp}");
    }

    IEnumerator NextTaskSequence()
    {
        // Show next task message
        if (nextTaskText != null)
        {
            nextTaskText.SetActive(true);
            nextTaskText.GetComponent<UnityEngine.UI.Text>().text = "Next Task will be ...";
        }

        UnityEngine.Debug.Log("Next task in 3 seconds...");

        yield return new WaitForSeconds(3); // Wait before scene change

        SaveData(); // Preserve data before transitioning

        // Load the next scene
        SceneManager.LoadScene("StoplightHighlight"); // Change to your actual scene name
    }

    void SaveData()
    {
        PlayerPrefs.SetInt("currentKeyIndex", currentKeyIndex);
        PlayerPrefs.SetString("csvFilePath", csvFilePath);
        PlayerPrefs.Save();
    }

    void WriteToCSV(string data, bool isHeader = false)
    {
        try
        {
            using (StreamWriter writer = new StreamWriter(csvFilePath, true, System.Text.Encoding.UTF8))
            {
                writer.WriteLine(data);
            }
        }
        catch (IOException e)
        {
            UnityEngine.Debug.LogError("Error writing to CSV file: " + e.Message);
        }
    }


    void OpenCSVFile()
    {
#if UNITY_EDITOR
        Process.Start(csvFilePath);
#endif
    }
}
