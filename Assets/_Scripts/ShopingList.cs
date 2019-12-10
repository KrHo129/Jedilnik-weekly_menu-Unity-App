using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;

public class ShopingList : MonoBehaviour {
    
    ///  za ime uporabnika
    GameObject nameEntryGameObject;
    InputField nameEntryInputField;
    
    /// za vse ostalo
    public Sprite spriteShopingListNormal;
    public Sprite spriteShopingListNew;

    public GameObject shopingListChBoxPrefab;

    public GameObject shopingListDeletionConfirmationGO;

    GameObject notificationMenuGO;
    GameObject shopingListMenuGO;
    Transform shopingListContentTransform;
    Image btnShopingList;

    InputField shopingListEntryInputField;



	void Start () {
        nameEntryGameObject = GameObject.Find("Img_NameEntry");
        nameEntryInputField = GameObject.Find("InputField_Name").GetComponent<InputField>();
        if (PlayerPrefs.HasKey("myName"))
        {
            nameEntryGameObject.SetActive(false);
        }

        StartLiseningForEventsInShopingList();
        StartLisenigForNewDataEntryInShoppingList();

        CheckIfShopingListSeen();
    }

    public void SetObjects()
    {
        notificationMenuGO = GameObject.Find("Img_NotificationMenu");
        shopingListEntryInputField = GameObject.Find("InputField_ShopingListEntry").GetComponent<InputField>();
        shopingListMenuGO = GameObject.Find("Img_ShopingList");
        shopingListContentTransform = GameObject.Find("Content_ShopingList").GetComponent<Transform>();
        btnShopingList = GameObject.Find("Btn_ShopingList").GetComponent<Image>();
    }

    void CheckIfShopingListSeen()
    {
        DatabaseReference refference = FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/Content/");
        refference.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("task failed");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;

                if (snap.ChildrenCount <= 0)
                {
                    btnShopingList.sprite = spriteShopingListNormal;
                    return;
                }
                else
                {
                    ShopingListAddingLisener(null, null);
                }
            }
        });

    }

    #region Butons
    public void BtnSaveNameToPlayerPrefs()
    {
        if (nameEntryInputField.text == "")
        {
            return;
        }

        string tempName = nameEntryInputField.text;

        PlayerPrefs.SetString("myName", tempName);

        nameEntryGameObject.SetActive(false);
    }

    public void BtnAddToShopingList()
    {
        if (shopingListEntryInputField.text == "")
        {
            return;
        }

        string tempNewShopingListEntry = shopingListEntryInputField.text;
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/Content/").
            Child(tempNewShopingListEntry).SetValueAsync(tempNewShopingListEntry);
        shopingListEntryInputField.text = "";

        // odstranimo seznam ljudi ki so ta seznam že videli
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/").
            Child("SeenBy").RemoveValueAsync();

        string myOwnName = PlayerPrefs.GetString("myName");
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/SeenBy/").
            Child(myOwnName).SetValueAsync(myOwnName);
    }
    
    public void BtnOpenShopingListMenu()
    {
        if (notificationMenuGO.activeInHierarchy)
        {
            notificationMenuGO.SetActive(false);
        }
        if(!shopingListMenuGO.activeInHierarchy)
        {
            shopingListMenuGO.SetActive(true);
        }

        foreach (Transform item in shopingListContentTransform)
        {
            Destroy(item.gameObject);
        }

        DatabaseReference reff = FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/Content/");
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
                    foreach (var singleItem in snap.Children)
                    {
                        GameObject newItem = Instantiate(shopingListChBoxPrefab, shopingListContentTransform);
                        Text itemTextValue = newItem.GetComponentInChildren<Text>();
                        itemTextValue.text = singleItem.Value.ToString();
                    }
                }
                
                string myOwnName = PlayerPrefs.GetString("myName");
                FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/SeenBy/").
                    Child(myOwnName).SetValueAsync(myOwnName);

                btnShopingList.sprite = spriteShopingListNormal;
            }
        });
    }

    public void BtnCloseShopingListMenu()
    {
        shopingListMenuGO.SetActive(false);
    }

    public void BtnRemoveItems()
    {
        // pogledamo če smo sploh označili kakšen item in končamo funkcijo če nismo
        bool atLeastOneMarkedItem = false;
        foreach (Transform item in shopingListContentTransform)
        {
            if(item.GetComponent<Toggle>().isOn)
            {
                atLeastOneMarkedItem = true;
                break;
            }
        }

        if (!atLeastOneMarkedItem)
        {
            return;
        }

        shopingListDeletionConfirmationGO.SetActive(true);
    }

    public void BtnCancelDeleteion()
    {
        shopingListDeletionConfirmationGO.SetActive(false);
    }

    public void BtnConfirmDeleteion()
    {
        string tempItemName;

        foreach (Transform item in shopingListContentTransform)
        {
            if(item.GetComponent<Toggle>().isOn)
            {
                tempItemName = item.GetComponentInChildren<Text>().text;
                FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/Content/").
                    Child(tempItemName).RemoveValueAsync();
            }
        }
        
        shopingListDeletionConfirmationGO.SetActive(false);
    }

    #endregion

    void StartLisenigForNewDataEntryInShoppingList()
    {
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/Content/").ChildAdded += ShopingListAddingLisener;
    }

    void ShopingListAddingLisener(object sender, ChildChangedEventArgs args)
    {
        string myOwnName = PlayerPrefs.GetString("myName");

        DatabaseReference reff = FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/SeenBy/");
        reff.GetValueAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("task failed");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snap = task.Result;

                bool seenByMe = false;
                if (snap.ChildrenCount > 0)
                {
                    foreach (var person in snap.Children)
                    {
                        if (person.Value.ToString() == myOwnName)
                        {
                            seenByMe = true;
                            break;
                        }
                    }
                }

                if (!seenByMe)
                {
                    btnShopingList.sprite = spriteShopingListNew;
                }
                else
                {
                    btnShopingList.sprite = spriteShopingListNormal;
                }
            }
        });
    }

    void StartLiseningForEventsInShopingList()
    {
        FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/ShopingList/Content/").ValueChanged += ShopingListGeneralLisener;
    }
    
    void ShopingListGeneralLisener(object sender, ValueChangedEventArgs newArguments)
    {
        if (newArguments.DatabaseError != null)
        {
            Debug.Log("error while lisening for data change");
            return;
        }

        if (!shopingListMenuGO.activeInHierarchy)
        {
            return;
        }

        DataSnapshot snap = newArguments.Snapshot;

        foreach (Transform item in shopingListContentTransform)
        {
            Destroy(item.gameObject);
        }
        
        if (snap.ChildrenCount > 0)
        {
            foreach (var singleItem in snap.Children)
            {
                GameObject newItem = Instantiate(shopingListChBoxPrefab, shopingListContentTransform);
                Text itemTextValue = newItem.GetComponentInChildren<Text>();
                itemTextValue.text = singleItem.Value.ToString();
            }
        }
    }
}
