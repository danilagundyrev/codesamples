using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class YandexSpeechKit : MonoBehaviour
{
    public string yskText;
    public YSKResultHandler YSKResult;
    public Text RecognizedText;
    public Text ErrorText;

    string apiToken = "apiTokenValue";
    string folderID = "folderIDValue";

    public void LaunchYSK()
    {
        string url = "https://stt.api.cloud.yandex.net/speech/v1/stt:recognize?lang=ru-RU&folderId=" + folderID + "&format=lpcm&sampleRateHertz=8000";
        string audiopath = Application.dataPath + "/StreamingAssets/Audio/testrecord.wav";
        StartCoroutine(postRequest(url, audiopath));
    }
    IEnumerator postRequest(string url, string filepath)
    {
        byte[] fileContent = File.ReadAllBytes(filepath);

        WWWForm formData = new WWWForm();
        formData.AddBinaryData("attachment", fileContent);

        UnityWebRequest uwr = UnityWebRequest.Post(url, formData);
        uwr.SetRequestHeader("Authorization", "Api-Key " + apiToken);

        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
            ErrorText.text = uwr.error;
        }
        else
        {
            yskText = uwr.downloadHandler.text;
            yskText = yskText.Replace('{', ' ');
            yskText = yskText.Replace('}', ' ');
            yskText = yskText.Replace('"', ' ');

            string finalText;
            finalText = yskText.Split(':')[1].Trim();

            YSKResult.OnResultRecievedFromServer(finalText.ToUpper());            

            Debug.Log(finalText);
        }
    }    
}
