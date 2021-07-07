using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class userUpdate : MonoBehaviour
{
    string URL = "http://localhost/mydb/userUpdate.php";
    public string InputUserName, InputUserId, InputPassword, InputEmail, WhereField, WhereCondition;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            UpdateUser(InputUserName, InputUserId, InputPassword, InputEmail, WhereField, WhereCondition);
    }

    public void UpdateUser(string username, string userid, string email, string password, string wF, string wC)
    {
        WWWForm form = new WWWForm();
        form.AddField("editUsername", username);
        form.AddField("editUserid", userid);
        form.AddField("editPassword", password);
        form.AddField("editEmail", email);

        form.AddField("whereField", wF);
        form.AddField("whereCondition", wC);

        WWW www = new WWW(URL, form);
    }

}
