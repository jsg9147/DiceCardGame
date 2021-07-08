using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class DBManager : MonoBehaviour
{
    [Header("Login")]
    public TMP_InputField LoginField;
    public TMP_InputField passwordLoginField;

    [Header("Register")]
    public TMP_InputField userIdRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_Text warningRegisterText;

    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    public PhotonManager photonManager;

    private bool connected;

    string verCheckURL = "http://localhost/";
    string getUsersURL = "http://localhost/mydb/GetUsers.php";
    string registerUserURL = "http://localhost/mydb/RegisterUser.php";

    void Start()
    {
        StartCoroutine(GetText(verCheckURL));
    }

    public void LoginBtn()
    {
        StartCoroutine(Login(LoginField.text, passwordLoginField.text));
    }

    public void RegisterBtn()
    {
        StartCoroutine(RegisterUser(userIdRegisterField.text, passwordRegisterField.text, usernameRegisterField.text, emailRegisterField.text));
    }

    public void ClearLoginFeilds()
    {
        LoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    public void ClearAllFeilds()
    {
        ClearLoginFeilds();
        ClearRegisterFeilds();
    }

    IEnumerator GetText(string _uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(_uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = _uri.Split('/');
            int page = pages.Length - 1;

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text); // webRequest.downloadHandler.text 이게 텍스트를 받아옴
                    break;
            }
        }
    }

    IEnumerator GetUsers(string _uri)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(_uri))
        {
            // Request and wait for the desired page.
            yield return www.SendWebRequest();

            string[] pages = _uri.Split('/');
            int page = pages.Length - 1;

            switch (www.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(pages[page] + ": Error: " + www.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(pages[page] + ": HTTP Error: " + www.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(pages[page] + ":\nReceived: " + www.downloadHandler.text); // webRequest.downloadHandler.text 이게 텍스트를 받아옴
                    break;
            }
        }
    }

    IEnumerator Login(string _userId, string _password)
    {
        WWWForm form = new WWWForm();

        form.AddField("userId", _userId);
        form.AddField("password", _password);

        using (UnityWebRequest www = UnityWebRequest.Post(getUsersURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                ClearAllFeilds();

                photonManager.Connect();
            }
        }
    }

    IEnumerator RegisterUser(string _userId, string _password, string _username, string _email)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            WWWForm form = new WWWForm();

            form.AddField("userId", _userId);
            form.AddField("password", _password);
            form.AddField("username", _username);
            form.AddField("email", _email);

            using (UnityWebRequest www = UnityWebRequest.Post(registerUserURL, form))
            {
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log(www.downloadHandler.text);
                    ClearAllFeilds();
                    UIManager.instance.LoginScreen();
                }
            }
        }
        
    }
}
