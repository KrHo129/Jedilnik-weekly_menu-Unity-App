using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PushNotificationBtn : MonoBehaviour {

    PushNotification pushNotificationScript;
    string notificationMessage;

	// Use this for initialization
	void Start () {
        pushNotificationScript = GameObject.Find("Manager").GetComponent<PushNotification>();

    }
	
    public void BtnSendNotification()
    {
        notificationMessage = gameObject.GetComponentInChildren<Text>().text;
        pushNotificationScript.CallCloudFunction(notificationMessage);
    }

}
