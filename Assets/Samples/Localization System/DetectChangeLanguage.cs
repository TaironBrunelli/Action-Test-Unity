using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DetectChangeLanguage : MonoBehaviour
{
    public Dropdown DropdownObj;
    public LocalizationManager LocalizationManager;
    public GameObject[] textUI;

    void Start()
    {
        DropdownObj = GetComponent<Dropdown>(); 
    }    
    
    public void OnLanguageChange()
    {
        switch (DropdownObj.value)
        {
            case 0:
                LocalizationManager.currentLanguageID = 0;
            break;

            case 1:
                LocalizationManager.currentLanguageID = 1;
            break;           
        }
        for (int i = 0; i < textUI.Length; i++)
        {
            textUI[i].SetActive(!textUI[i].activeSelf);
            textUI[i].SetActive(!textUI[i].activeSelf);
        }
    }
}