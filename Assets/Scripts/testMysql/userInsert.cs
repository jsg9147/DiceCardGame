using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class userInsert : MonoBehaviour
{
    string URL = "http://localhost/mydb/userInsert.php";

    public string InputUserName, InputUserId, InputPassword, InputEmail;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AddUser(InputUserName, InputUserId, InputPassword, InputEmail);
        }
    }

    public void AddUser(string username, string userid, string password, string email)
    {
        WWWForm form = new WWWForm();
        form.AddField("addUsername", username);
        form.AddField("addUserid", userid);
        form.AddField("addPassword", password);
        form.AddField("addEmail", email);

        WWW www = new WWW(URL, form);
    }
}
