using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

#region 请求构造体
//构造请求体
public class ChatGPTRequest
{
    public string model { get; set; }
    public List<ChatContent> messages { get; set; }
}
#endregion

#region 返回结果构造体
public class ChatGPTResponse
{
    public string id { get; set; }
    public string object_ { get; set; }
    public int created { get; set; }
    public string model { get; set; }
    public List<ChatGPTContent> choices { get; set; }
    public ChatGPTUsage usage { get; set; }
    public string system_fingerprint { get; set; }
}

public class ChatGPTContent
{
    public int index { get; set; }
    public ChatContent message { get; set; }
    public string logprobs { get; set; }
    public string finish_reason { get; set; }
}

public class ChatGPTUsage
{
    public int prompt_tokens { get; set; }
    public int completion_tokens { get; set; }
    public int total_tokens { get; set; }
}
#endregion

public class ChatGPTHelper : NLPBaseHelper
{
    static CancellationToken cancellation;
    public ChatGPTConfig OpenAIConfig;

    private void Awake()
    {
        ResetDataList();
        Init();
    }
    // private void OnValidate()
    // {
    //     ResetDataList();
    // }

    void ResetDataList()
    {
        sendDataList.Clear();
        if (OpenAIConfig != null)
            sendDataList.Add(new ChatContent { role = "system", content = OpenAIConfig.prompt });
    }

    public override void Init()
    {

    }

    /// <summary>
    /// 发送请求
    /// </summary>
    /// <param name="message">请求的信息</param>
    /// <param name="callback">处理得到回应的回调</param>
    /// <returns></returns> <summary>
    public override void PostMessage(string message)
    {
        sendDataList.Add(new ChatContent { role = "user", content = message });
        CheckSendLength(OpenAIConfig.maxHistory);
        StartCoroutine(RequestMessage());
    }

    /// <summary>
    /// 请求信息
    /// </summary>
    /// <param name="callback"></param>
    /// <returns></returns> <summary>
    private IEnumerator RequestMessage()
    {
        using (UnityWebRequest request = new UnityWebRequest(OpenAIConfig.OpenAIUrl, "POST"))
        {
            // 请求头
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {OpenAIConfig.api_key}");

            // Body生成
            ChatGPTRequest jsonRequest = new ChatGPTRequest
            {
                model = OpenAIConfig.OpenAIModel,
                messages = sendDataList
            };

            string requestJson = JsonConvert.SerializeObject(jsonRequest);
            Debug.Log(requestJson);
            byte[] data = Encoding.UTF8.GetBytes(requestJson);

            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();
            // "正在处理中..." 提示

            yield return request.SendWebRequest();
            if (request.responseCode == 200)
            {
                string _msg = request.downloadHandler.text;
                HandlingRespond(_msg);
            }
            else
            {
                Debug.LogError("请求失败: " + request.responseCode);
            }
            // string _msg = request.downloadHandler.text;
            // Debug.Log(_msg);
        }
    }

    /// <summary>
    /// 处理返回的信息
    /// </summary>
    /// <param name="requestMsg"></param>
    /// <param name="callback"></param> <summary>
    private void HandlingRespond(string requestMsg)
    {
        Debug.Log(requestMsg);
        var jsonObj = JsonConvert.DeserializeObject<ChatGPTResponse>(requestMsg);

        var resp = jsonObj.choices[0].message.content;
        Debug.Log("请求成功: " + resp);
        Debug.Log($"本次消耗token数: {jsonObj.usage.total_tokens}");

        sendDataList.Add(new ChatContent { role = "assistant", content = resp });
        onReceiveMessage.Invoke(resp);
    }
}
