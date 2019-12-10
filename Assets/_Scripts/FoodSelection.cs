using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class FoodSelection : MonoBehaviour {

    int dayIndex = 10;

    List<string> foodList;
    Transform scroolviewContent;
    public GameObject addFoodButtonPrefab;
    public GameObject removeFoodButtonPrefab;

    InputField customEntryText;

    TextBoxControl textBoxControlScript;

    public void SetDayIndex(int dIndex)
    {
        dayIndex = dIndex;
    }
    

    // mora biti public ker do tega dostopam iz druge skripte
    public void FindAndCreateAllFoodBtns () {
        textBoxControlScript = GameObject.Find("Manager").GetComponent<TextBoxControl>();
        customEntryText = GameObject.Find("InputField_CustomEntry").GetComponent<InputField>();
        FindFoodList();
    }
	
    void CreateButtonsForWholeList()
    {
        scroolviewContent = GameObject.Find("Content_FoodSelection").GetComponent<Transform>();

        foreach(Transform child in scroolviewContent)
        {
            Destroy(child.gameObject);
        }

        foreach (string singleFood in foodList)
        {
            GameObject newFood = Instantiate(addFoodButtonPrefab, scroolviewContent);
            newFood.GetComponentInChildren<Text>().text = singleFood;
        }
    }

    void FindFoodList() {

        foodList = new List<string>();

        DatabaseReference reff = FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/FoodList/");
        reff.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("task failed");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;

                if (snap.ChildrenCount> 0)
                {
                    foreach (var singleFood in snap.Children)
                    {
                        foodList.Add(singleFood.Value.ToString());
                    }
                }
            }
            CreateButtonsForWholeList();
        });
    }

    public void AddingFoodToADay(string foodName)
    {
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/").Child(dayIndex.ToString())
            .Child("Food").Child(foodName).SetValueAsync(foodName);
        textBoxControlScript.HideAddRemoveMenu();
    }

    public void CustomFoodEntry()
    {
        if(customEntryText.text == "")
        {
            return;
        }

        string newFoodEntry = customEntryText.text;
        customEntryText.text = "";
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/").Child(dayIndex.ToString())
            .Child("Food").Child(newFoodEntry).SetValueAsync(newFoodEntry);
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/FoodList/")
            .Child(newFoodEntry).SetValueAsync(newFoodEntry);
        textBoxControlScript.HideAddRemoveMenu();
    }


    public void CreateDailyFoodList(string[] dailyFoodList)
    { 
        textBoxControlScript = GameObject.Find("Manager").GetComponent<TextBoxControl>();
        scroolviewContent = GameObject.Find("Content_FoodSelection").GetComponent<Transform>();

        foreach (Transform child in scroolviewContent)
        {
            Destroy(child.gameObject);
        }

        foreach (string singleFood in dailyFoodList)
        {
            GameObject newFood = Instantiate(removeFoodButtonPrefab, scroolviewContent);
            newFood.GetComponentInChildren<Text>().text = singleFood;
        }
    }

    public void FoodRemovalFromADay(string foodName)
    {
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/").Child(dayIndex.ToString())
            .Child("Food").Child(foodName).RemoveValueAsync();
        textBoxControlScript.HideAddRemoveMenu();
    }

}
