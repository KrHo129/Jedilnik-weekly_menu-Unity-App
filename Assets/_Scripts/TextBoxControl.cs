using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;

public class TextBoxControl : MonoBehaviour
{

    GameObject loadingCoverGO;
    GameObject oneDayMenuGO;
    int oneDayIndex = 10;
    Text oneDayName;
    Text oneDayContent;

    GameObject AddRemoveMenuGO;

    private string tempObjectName;
    private string tempDayName;

    // textboxes for food 
    private Text[,] allTextBoxes;
    // gameobjects for food (SetActive true/false)
    private GameObject[,] allTextImageGO;
    // dates textBoxes
    private Text[] dateTextBoxes;
    // texboxes with three dots (more)
    private GameObject[] dotsGO;
    // array trenutnih datumov
    private string[] allCurrentDates;
    // seznam vseh hran po dnevih
    static string[][] foodsByDay;

    // food selection script reference
    FoodSelection foodSelectionScript;

    GameObject blockCustomEntryImg;

    // notifications
    GameObject notificationMenu;
    PushNotification pushNotificationScript;

    // shoping list
    GameObject shopingListMenuGameObject;
    ShopingList shopingListScript;

    // Use this for initialization
    void Start()
    {
        blockCustomEntryImg = GameObject.Find("Img_BlockCustomEntry");
        foodSelectionScript = gameObject.GetComponent<FoodSelection>();
        pushNotificationScript = gameObject.GetComponent<PushNotification>();
        shopingListScript = gameObject.GetComponent<ShopingList>();


        FindGameObjectsAndTexts();
        SetDates();
        CheckAndCorrectCurrentDatabaseDates();
        StartLiseningForEvents();
    }

    void StartLiseningForEvents()
    {
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/").ValueChanged += HandleChangedDataInDatabase;
    }

    void HandleChangedDataInDatabase(object sender, ValueChangedEventArgs newArguments)
    {
        if (newArguments.DatabaseError != null)
        {
            Debug.Log("error while lisening for data change");
            return;
        }

        // vse na neaktivno
        foreach (GameObject box in allTextImageGO)
        {
            box.SetActive(false);
        }
        foreach (GameObject dots in dotsGO)
        {
            dots.SetActive(false);
        }

        DataSnapshot snap = newArguments.Snapshot;

        for (int i = 0; i < 7; i++)
        {
            foodsByDay[i] = new string[0];
            
            int childCount = (int)snap.Child(i.ToString()).Child("Food").ChildrenCount;
            if (childCount > 0)
            {
                foodsByDay[i] = new string[childCount];
                int j = 0;
                foreach (var food in snap.Child(i.ToString()).Child("Food").Children)
                {
                    foodsByDay[i][j] = food.Value.ToString();
                    if (j > 2)
                    {
                        dotsGO[i].SetActive(true);
                    }
                    else
                    {
                        allTextBoxes[i, j].text = foodsByDay[i][j];
                        allTextImageGO[i, j].SetActive(true);
                    }
                    j++;
                }
            }
        }

        if (oneDayIndex<7)
        {
            oneDayContent.text = WholeListForTheDay(oneDayIndex);
        }

    }




    private void CheckAndCorrectCurrentDatabaseDates()
    {
        DatabaseReference reff = FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/");
        reff.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("task failed");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;

                for (int i = 0; i < 7; i++)
                {
                    string tempDate = "";
                    if (snap.Child(i.ToString()).Child("Date").Value != null)
                    {
                        tempDate = snap.Child(i.ToString()).Child("Date").Value.ToString();
                    }

                    if (tempDate != allCurrentDates[i])
                    {
                        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/").Child(i.ToString())
                            .Child("Date").SetValueAsync(allCurrentDates[i]);
                        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/").Child(i.ToString())
                            .Child("Food").RemoveValueAsync();
                    }
                }
            }
            CheckDatabaseFoodMenu();
        });
    }

    private void CheckDatabaseFoodMenu()
    {
        foodsByDay = new string[7][];

        DatabaseReference reff = FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/");
        reff.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("task failed");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;

                for (int i = 0; i < 7; i++)
                {
                    foodsByDay[i] = new string[0];

                    string tempDate = "";
                    if (snap.Child(i.ToString()).Child("Date").Value != null)
                    {
                        tempDate = snap.Child(i.ToString()).Child("Date").Value.ToString();
                    }

                    if (tempDate != allCurrentDates[i])
                    {
                        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/").Child(i.ToString())
                            .Child("Date").SetValueAsync(allCurrentDates[i]);
                        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/Days/").Child(i.ToString())
                            .Child("Food").RemoveValueAsync();
                    }
                    else
                    {
                        int childCount = (int)snap.Child(i.ToString()).Child("Food").ChildrenCount;
                        if (childCount > 0)
                        {
                            foodsByDay[i] = new string[childCount];
                            int j = 0;
                            foreach (var food in snap.Child(i.ToString()).Child("Food").Children)
                            {
                                foodsByDay[i][j] = food.Value.ToString();
                                if (j > 2)
                                {
                                    dotsGO[i].SetActive(true);
                                }
                                else
                                {
                                    allTextBoxes[i, j].text = foodsByDay[i][j];
                                    allTextImageGO[i, j].SetActive(true);
                                }
                                j++;
                            }
                        }

                    }
                }
            }
        });
    }

    private void SetDates()
    {
        allCurrentDates = new string[7];
        // pridobimo trenuten datum iz naprave
        DateTime currentDate = System.DateTime.Now;
        // spremenimo tako da je ponedeljek 0 (prej je nedelja 0)
        int currentDayOfTheWeek = ((int)currentDate.DayOfWeek + 6) % 7;

        // izpišemo datume ...
        for (int i = 0; i < 7; i++)
        {
            if (currentDayOfTheWeek > 6)
            {
                currentDayOfTheWeek -= 7;
            }
            allCurrentDates[currentDayOfTheWeek] = currentDate.AddDays(i).ToString("dd.MM.yyyy");
            dateTextBoxes[currentDayOfTheWeek].text = allCurrentDates[currentDayOfTheWeek];
            currentDayOfTheWeek++;
        }
    }

    private void FindGameObjectsAndTexts()
    {
        // iniciaziramo array
        allTextBoxes = new Text[7, 3];
        allTextImageGO = new GameObject[7, 3];
        dateTextBoxes = new Text[7];
        dotsGO = new GameObject[7];

        // gremo od ponedeljka do petka
        for (int i = 0; i < 7; i++)
        {
            // zgeneriramo ime dneva glede na intiger
            switch (i)
            {
                case 0:
                    tempDayName = "Pon";
                    break;
                case 1:
                    tempDayName = "Tor";
                    break;
                case 2:
                    tempDayName = "Sre";
                    break;
                case 3:
                    tempDayName = "Cet";
                    break;
                case 4:
                    tempDayName = "Pet";
                    break;
                case 5:
                    tempDayName = "Sob";
                    break;
                case 6:
                    tempDayName = "Ned";
                    break;
            }

            // poiščemo vseh sedem "more" "gumbov"
            tempObjectName = "Img_More_" + tempDayName;
            dotsGO[i] = GameObject.Find(tempObjectName);
            dotsGO[i].SetActive(false);

            // poiščemo vseh 7 textboxov z dnevi
            tempObjectName = "Txt_DayDate_" + tempDayName;
            dateTextBoxes[i] = GameObject.Find(tempObjectName).GetComponent<Text>();
            dateTextBoxes[i].text = "1.2.3";

            // ker so trije boxi na dan
            for (int j = 1; j <= 3; j++)
            {
                // zgeneriramo ime textboxa
                tempObjectName = "Txt_Food_" + tempDayName + "_" + j;
                // dodamo textbox v array
                allTextBoxes[i, (j - 1)] = GameObject.Find(tempObjectName).GetComponent<Text>();
                tempObjectName = "Img_Food_" + tempDayName + "_" + j;
                allTextImageGO[i, j - 1] = GameObject.Find(tempObjectName);
                allTextImageGO[i, j - 1].SetActive(false);
            }
        }

        shopingListMenuGameObject = GameObject.Find("Img_ShopingList");
        shopingListScript.SetObjects();
        oneDayName = GameObject.Find("Txt_OneDay_DayName").GetComponent<Text>();
        oneDayContent = GameObject.Find("Txt_OneDayContent").GetComponent<Text>();
        AddRemoveMenuGO = GameObject.Find("Img_AddRemoveMenu");
        oneDayMenuGO = GameObject.Find("Img_OneDayMenu");
        loadingCoverGO = GameObject.Find("Img_StartLoadingCover");
        notificationMenu = GameObject.Find("Img_NotificationMenu");
        AddRemoveMenuGO.SetActive(false);
        oneDayMenuGO.SetActive(false);
        loadingCoverGO.SetActive(false);
        notificationMenu.SetActive(false);
        shopingListMenuGameObject.SetActive(false);
        oneDayIndex = 10;
    }



    string DayNameFromInt(int dayIntiger)
    {
        string dayName = "";
        switch (dayIntiger)
        {
            case 0:
                dayName = "Ponedeljek";
                break;
            case 1:
                dayName = "Torek";
                break;
            case 2:
                dayName = "Sreda";
                break;
            case 3:
                dayName = "Četrtek";
                break;
            case 4:
                dayName = "Petek";
                break;
            case 5:
                dayName = "Sobota";
                break;
            case 6:
                dayName = "Nedelja";
                break;
        }
        return dayName;
    }

    string WholeListForTheDay (int dayIntiger)
    {
        string tempStr = "";

        foreach (string str in foodsByDay[dayIntiger])
        {
            tempStr += "  -  " + str + "\n";
        }
        return tempStr;
    }


    #region Buttons

    public void Btn0Ponedeljek()
    {
        oneDayMenuGO.SetActive(true);
        oneDayIndex = 0;
        oneDayName.text = DayNameFromInt(oneDayIndex);
        oneDayContent.text = WholeListForTheDay(oneDayIndex);
    }

    public void Btn1Torek()
    {
        oneDayMenuGO.SetActive(true);
        oneDayIndex = 1;
        oneDayName.text = DayNameFromInt(oneDayIndex);
        oneDayContent.text = WholeListForTheDay(oneDayIndex);

    }

    public void Btn2Sreda()
    {
        oneDayMenuGO.SetActive(true);
        oneDayIndex = 2;
        oneDayName.text = DayNameFromInt(oneDayIndex);
        oneDayContent.text = WholeListForTheDay(oneDayIndex);
    }

    public void Btn3Cetrtek()
    {
        oneDayMenuGO.SetActive(true);
        oneDayIndex = 3;
        oneDayName.text = DayNameFromInt(oneDayIndex);
        oneDayContent.text = WholeListForTheDay(oneDayIndex);
    }

    public void Btn4Petek()
    {
        oneDayMenuGO.SetActive(true);
        oneDayIndex = 4;
        oneDayName.text = DayNameFromInt(oneDayIndex);
        oneDayContent.text = WholeListForTheDay(oneDayIndex);
    }

    public void Btn5Sobota()
    {
        oneDayMenuGO.SetActive(true);
        oneDayIndex = 5;
        oneDayName.text = DayNameFromInt(oneDayIndex);
        oneDayContent.text = WholeListForTheDay(oneDayIndex);
    }

    public void Btn6Nedelja()
    {
        oneDayMenuGO.SetActive(true);
        oneDayIndex = 6;
        oneDayName.text = DayNameFromInt(oneDayIndex);
        oneDayContent.text = WholeListForTheDay(oneDayIndex);
    }

    public void BtnBackFromOneDayMenu()
    {
        oneDayMenuGO.SetActive(false);
        oneDayIndex = 10;
    }

    public void BtnBackFromAddRemoveMenu()
    {
        AddRemoveMenuGO.SetActive(false);
    }

    public void BtnAddFoodToADay ()
    {
        foodSelectionScript.SetDayIndex(oneDayIndex);
        AddRemoveMenuGO.SetActive(true);
        foodSelectionScript.FindAndCreateAllFoodBtns();
        if(blockCustomEntryImg.activeInHierarchy)
        {
            blockCustomEntryImg.SetActive(false);
        }
    }

    public void BtnRemoveFoodFromADay()
    {
        foodSelectionScript.SetDayIndex(oneDayIndex);
        AddRemoveMenuGO.SetActive(true);
        foodSelectionScript.CreateDailyFoodList(foodsByDay[oneDayIndex]);
        if (!blockCustomEntryImg.activeInHierarchy)
        {
            blockCustomEntryImg.SetActive(true);
        }
    }

    public void BtnNotificationMenu()
    {
        if (shopingListMenuGameObject.activeInHierarchy)
        {
            shopingListMenuGameObject.SetActive(false);
        }
        if (!notificationMenu.activeInHierarchy)
        {
            notificationMenu.SetActive(true);
        }
        pushNotificationScript.LookIntoDatabaseForOldEntries();
    }

    public void BtnBackFormNotificationMenu()
    {
        notificationMenu.SetActive(false);
    }



    public void QuitApp()
    {
        Application.Quit();
    }

    #endregion

    public void HideAddRemoveMenu()
    {
        AddRemoveMenuGO.SetActive(false);
    }

    #region android Back button handling

    int backBtnPressedCounter = 0;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            CheckOpenMenusAndCloseLastOne();
        }
    }

    void CheckOpenMenusAndCloseLastOne()
    {
        if(shopingListMenuGameObject.activeInHierarchy)
        {
            shopingListMenuGameObject.SetActive(false);
            backBtnPressedCounter = 0;
        }
        else if (notificationMenu.activeInHierarchy)
        {
            notificationMenu.SetActive(false);
            backBtnPressedCounter = 0;
        }
        else if(AddRemoveMenuGO.activeInHierarchy)
        {
            AddRemoveMenuGO.SetActive(false);
            backBtnPressedCounter = 0;
        }
        else if(oneDayMenuGO.activeInHierarchy)
        {
            oneDayMenuGO.SetActive(false);
            backBtnPressedCounter = 0;
        }
        else if (backBtnPressedCounter > 0)
        {
            backBtnPressedCounter = 0;
            Application.Quit();
        }
        else
        {
            backBtnPressedCounter++;
        }
    }
    #endregion

}
