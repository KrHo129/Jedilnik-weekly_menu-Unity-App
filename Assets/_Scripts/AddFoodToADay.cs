using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class AddFoodToADay : MonoBehaviour {

    FoodSelection foodSelectionScript;


    private void Start()
    {
        foodSelectionScript = GameObject.Find("Manager").GetComponent<FoodSelection>();
    }

    public void AddFoodToTheDay()
    {
        string foodName = gameObject.GetComponentInChildren<Text>().text;
        foodSelectionScript.AddingFoodToADay(foodName);
    }
}
