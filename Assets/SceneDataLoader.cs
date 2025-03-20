using UnityEngine;
using System.IO;

public class SceneDataLoader : MonoBehaviour
{
    void Start()
    {
        // Retrieve the saved key index and CSV file path
        int keyIndex = PlayerPrefs.GetInt("currentKeyIndex", 36);
        string filePath = PlayerPrefs.GetString("csvFilePath");

        Debug.Log("Loaded Data: Key Index = " + keyIndex + ", CSV File = " + filePath);

        // Example: If you need to use this data in this scene, you can apply it here
        if (File.Exists(filePath))
        {
            Debug.Log("CSV file exists and will continue logging data.");
        }
        else
        {
            Debug.LogError("CSV file missing! Data might not be saved correctly.");
        }
    }
}
