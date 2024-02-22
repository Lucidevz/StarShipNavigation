using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GetStarNames : MonoBehaviour
{
    public List<string> starNames = new List<string>();

    private void Start() {
        // Read from a text file in the assets folder
        string readFromFilePath = Application.streamingAssetsPath + "/TextFiles/" + "StarNames" + ".txt";

        // Put each line from text file into a new entry in the list 
        starNames = File.ReadAllLines(readFromFilePath).ToList();
    }

    public string GetStarName(int index) {
        return starNames[index];
    }

}
