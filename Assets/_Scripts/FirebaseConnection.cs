namespace Firebase.Sample.Database
{
    using Firebase;
    using Firebase.Database;
    using Firebase.Unity.Editor;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using Firebase.Messaging;
    using System.Threading.Tasks;

    public class FirebaseConnection : MonoBehaviour
    {
        DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
        

        protected virtual void Start()
        {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    InitializeFirebase();
                }
                else
                {
                    Debug.LogError(
                      "Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }

        // Initialize the Firebase database:
        protected virtual void InitializeFirebase()
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            // NOTE: You'll need to replace this url with your Firebase App's database
            // path in order for the database connection to work correctly in editor.
            app.SetEditorDatabaseUrl("https://testing-krho.firebaseio.com/");
            if (app.Options.DatabaseUrl != null)
                app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
            FirebaseMessaging.SubscribeAsync("DruzinaVrhovec").ContinueWith(task => {
                LogTaskCompletion(task, "SubscribeAsync");
            });
        }

        protected bool LogTaskCompletion(Task task, string operation)
        {
            bool complete = false;
            if (task.IsCanceled)
            {
                Debug.Log(operation + " canceled.");
            }
            else if (task.IsFaulted)
            {
                Debug.Log(operation + " encounted an error.");
            }
            else if (task.IsCompleted)
            {
                complete = true;
            }
            return complete;
        }



        /*

        public void customAdd ()
        {
            FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/FoodList/").Child("Solata").SetValueAsync("Solata");
            FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/FoodList/").Child("Pesa").SetValueAsync(0);

            FirebaseDatabase.DefaultInstance.RootReference.Child("lunch-schedule").Child("FoodList").Child("riž").RemoveValueAsync();
            
        }


        public void customlisten ()
        {
            DatabaseReference reff = FirebaseDatabase.DefaultInstance.GetReference("/lunch-schedule/FoodList/");
            reff.GetValueAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    t.text = "Failed";
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snap = task.Result;


                    string str = "";
                    foreach (var childSnapshot in snap.Children)
                    {
                        str += childSnapshot.Value;
                    }
                    t.text = str;
                }
            });
        }
        */
    }
}