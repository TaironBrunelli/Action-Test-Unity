using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
 
[RequireComponent(typeof (Text))]
public class LocalizationUIText : MonoBehaviour
{
    public string key;
    private int currentlanguageSave;

    public LocalizationManager LocalizationManager;
    

    void Start()
    {
        // Get the string value from localization manager from key 
        // and set the text component text value to the  returned string value 
        GetComponent<Text>().text = LocalizationManager.Instance.GetText(key);  
        currentlanguageSave = LocalizationManager.currentLanguageID;
    }

    void OnEnable()
    {
        if (currentlanguageSave != LocalizationManager.currentLanguageID)
        {
            GetComponent<Text>().text = LocalizationManager.Instance.GetText(key);
            currentlanguageSave = LocalizationManager.currentLanguageID;
        } 
    }


}