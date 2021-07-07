using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class userSelect : MonoBehaviour
{
    // php 파일 있는 url 만들어야 할듯?
    string URL = "http://localhost/mydb/userSelect.php";

    public string UserId, UserPassword;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SearchUser(UserId, UserPassword));

        /*
        WWW users = new WWW(URL);
        yield return users;
        string usersDataString = users.text;
        usersData = usersDataString.Split(';');
        */
        
    }

    IEnumerator SearchUser(string userid, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("userId", userid);
        form.AddField("password", password);

        WWW users = new WWW(URL, form);

        yield return users;

        print(users.text);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
