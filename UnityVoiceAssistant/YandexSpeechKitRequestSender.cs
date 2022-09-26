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

public class YandexSpeechKitRequestSender : MonoBehaviour
{
    [SerializeField]private string apiToken = "apiTokenValue";
    [SerializeField]private string folderID = "folderIDValue";
    private string _receivedText;
    
    public YSKResultHandler YSKResult;

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
        }
        else
        {
            _receivedText = ProcessReceivedText(uwr.downloadHandler.text);
            YSKResult.OnResultRecievedFromServer(_receivedText.ToUpper());
        }
    }
    
    private string ProcessReceivedText(string text)
    {
        string processedText = text;
        processedText = processedText.Replace('{', ' ');
        processedText = processedText.Replace('}', ' ');
        processedText = processedText.Replace('"', ' ');
        processedText = processedText.Split(':')[1].Trim();
        return processedText;
}
