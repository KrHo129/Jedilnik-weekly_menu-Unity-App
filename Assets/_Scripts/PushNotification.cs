using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Functions;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine.UI;
using System;

public class PushNotification : MonoBehaviour {

    TextBoxControl textBoxControlScript;

    FirebaseFunctions functions;

    List<string> notificationList;
    Transform notificationScroolviewContent;
    public GameObject notificationButtonPrefab;


    private void Start()
    {
        textBoxControlScript = gameObject.GetComponent<TextBoxControl>();
    }

    public void LookIntoDatabaseForOldEntries()
    {
        notificationList = new List<string>();

        DatabaseReference reff = FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/NotificationList/");
        reff.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("task failed");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;

                if (snap.ChildrenCount > 0)
                {
                    foreach (var singleNotification in snap.Children)
                    {
                        notificationList.Add(singleNotification.Value.ToString());
                    }
                }
            }
            CreateButtonsForWholeNotificationList();
        });
    }

    void CreateButtonsForWholeNotificationList()
    {
        notificationScroolviewContent = GameObject.Find("Content_NotificationSelection").GetComponent<Transform>();

        foreach (Transform child in notificationScroolviewContent)
        {
            Destroy(child.gameObject);
        }

        foreach (string singleNotification in notificationList)
        {
            GameObject newFood = Instantiate(notificationButtonPrefab, notificationScroolviewContent);
            newFood.GetComponentInChildren<Text>().text = singleNotification;
        }
    }

    public void CustomNotificationEntry()
    {
        InputField customEntryText = GameObject.Find("InputField_CustomMessageEntry").GetComponent<InputField>();

        if (customEntryText.text == "")
        {
            return;
        }

        string newNotificationEntry = customEntryText.text;
        customEntryText.text = "";
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/NotificationList/")
            .Push().SetValueAsync(newNotificationEntry);
        CallCloudFunction(newNotificationEntry);
        textBoxControlScript.BtnBackFormNotificationMenu();
    }



    // Use this for initialization
    public void CallCloudFunction (string notificationText) {

        if (functions == null)
        {
            functions = FirebaseFunctions.DefaultInstance;
        }
        NoficiataionPushing(notificationText);
        textBoxControlScript.BtnBackFormNotificationMenu();
    }
    

    private Task<string> NoficiataionPushing(string notificationText)
    {
        // Create the arguments to the callable function.
        var data = new Dictionary<string, object>();
        data["naslov"] = "Tedenski Meni";
        data["vsebina"] = notificationText;

        // Call the function and extract the operation from the result.
        var function = functions.GetHttpsCallable("PushCustomNotificaion");
        return function.CallAsync(data).ContinueWith((task) => {
            return (string)task.Result.Data;
        });
    }
}
