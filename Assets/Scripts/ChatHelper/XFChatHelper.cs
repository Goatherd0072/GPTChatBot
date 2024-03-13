using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class XFChatHelper : NLPBaseHelper
{
    #region 请求构造体
    //构造请求体
    class JsonRequest
    {
        public Header header { get; set; }
        public Parameter parameter { get; set; }
        public Payload payload { get; set; }
    }

    class Header
    {
        public string app_id { get; set; }
        public string uid { get; set; }
    }

    class Parameter
    {
        public Chat chat { get; set; }
    }

    class Chat
    {
        public string domain { get; set; }
        public double temperature { get; set; }
        public int max_tokens { get; set; }
    }

    class Payload
    {
        public Message message { get; set; }
    }

    class Message
    {
        public List<ChatContent> text { get; set; }
    }
    #endregion

    static CancellationToken cancellation;
    public SparkChatConfig sparkConfig;
    string authUrl;

    private void Awake()
    {
        Init();
    }
    private void OnValidate()
    {
        ResetDataList();
    }

    void ResetDataList()
    {
        sendDataList.Clear();
        sendDataList.Add(new ChatContent { role = "system", content = sparkConfig.prompt });
    }

    public override void Init()
    {
        // ResetDataList();
        authUrl = GetAuthUrl(sparkConfig.xfUrl);
    }

    public override void PostMessage(string message, Action<string> callback)
    {
        sendDataList.Add(new ChatContent { role = "user", content = message });
        CheckSendLength(sparkConfig.maxHistory);
        RequestMessage(callback);
    }
    public async void RequestMessage(Action<string> callback)
    {
        using (ClientWebSocket webSocket = new ClientWebSocket())
        {
            try
            {
                await webSocket.ConnectAsync(new Uri(authUrl), cancellation);
                JsonRequest request = new JsonRequest
                {
                    header = new Header() { app_id = sparkConfig.x_appid, uid = "12345" },
                    parameter = new Parameter()
                    {
                        chat = new Chat()
                        {
                            domain = sparkConfig.xfDomain, //模型领域，默认为星火通用大模型
                            temperature = 0.5, //温度采样阈值，用于控制生成内容的随机性和多样性，值越大多样性越高；范围（0，1）
                            max_tokens = 1024, //生成内容的最大长度，范围（0，4096）
                        }
                    },
                    payload = new Payload()
                    {
                        message = new Message() { text = sendDataList }
                    }
                };

                string jsonString = JsonConvert.SerializeObject(request);

                //连接成功，开始发送数据
                var frameData2 = System.Text.Encoding.UTF8.GetBytes(jsonString.ToString());
                Debug.Log($"发送数据： {jsonString}");

                await webSocket.SendAsync(
                    new ArraySegment<byte>(frameData2),
                    WebSocketMessageType.Text,
                    true,
                    cancellation
                );

                // 接收流式返回结果进行解析
                byte[] receiveBuffer = new byte[2048];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(receiveBuffer),
                    cancellation
                );
                string resp = "";
                while (!result.CloseStatus.HasValue)
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string receivedMessage = Encoding.UTF8.GetString(
                            receiveBuffer,
                            0,
                            result.Count
                        );
                        //将结果构造为json

                        JObject jsonObj = JObject.Parse(receivedMessage);
                        int code = (int)jsonObj["header"]["code"];

                        if (0 == code)
                        {
                            int status = (int)jsonObj["payload"]["choices"]["status"];

                            JArray textArray = (JArray)jsonObj["payload"]["choices"]["text"];
                            string content = (string)textArray[0]["content"];
                            resp += content;
                            callback(content);
                            if (status != 2)
                            {
                                Debug.Log($"已接收到数据： {receivedMessage}");
                            }
                            else
                            {
                                Debug.Log($"最后一帧： {receivedMessage}");
                                int totalTokens = (int)
                                    jsonObj["payload"]["usage"]["text"]["total_tokens"];
                                Debug.Log($"整体返回结果： {resp}");
                                Debug.Log($"本次消耗token数： {totalTokens}");
                                sendDataList.Add(new ChatContent() { role = "assistant", content = resp });
                                EmotionAnalyze(resp);
                                // callback(resp);
                                break;
                            }
                        }
                        else
                        {
                            Debug.LogError($"请求报错： {receivedMessage}");
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Debug.Log("已关闭WebSocket连接");
                        break;
                    }

                    result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(receiveBuffer),
                        cancellation
                    );
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception: " + e);
            }
        }

    }

    #region  协程WebSocet
    // public override IEnumerator RequestMessage(Action<string> callback)
    // {
    //     using (ClientWebSocket webSocket = new ClientWebSocket())
    //     {
    //         yield return StartCoroutine(Connect(webSocket));
    //         JsonRequest request = new JsonRequest
    //         {
    //             header = new Header() { app_id = x_appid, uid = "12345" },
    //             parameter = new Parameter()
    //             {
    //                 chat = new Chat()
    //                 {
    //                     domain = xfDomain, //模型领域，默认为星火通用大模型
    //                     temperature = 0.5, //温度采样阈值，用于控制生成内容的随机性和多样性，值越大多样性越高；范围（0，1）
    //                     max_tokens = 1024, //生成内容的最大长度，范围（0，4096）
    //                 }
    //             },
    //             payload = new Payload()
    //             {
    //                 message = new Message() { text = sendDataList }
    //             }
    //         };

    //         string jsonString = JsonConvert.SerializeObject(request);


    //         // Send the request
    //         yield return StartCoroutine(Send(webSocket, jsonString));

    //         // Receive the response
    //         yield return StartCoroutine(Receive(webSocket, callback));
    //     }
    // }

    // private IEnumerator Connect(ClientWebSocket webSocket)
    // {
    //     Task task = webSocket.ConnectAsync(new Uri(authUrl), cancellation);
    //     while (!task.IsCompleted)
    //     {
    //         yield return null;
    //     }
    //     task.GetAwaiter().GetResult();
    // }

    // private IEnumerator Send(ClientWebSocket webSocket, string jsonString)
    // {
    //     var frameData2 = System.Text.Encoding.UTF8.GetBytes(jsonString.ToString());
    //     Debug.Log($"发送数据： {jsonString}");
    //     Task task = webSocket.SendAsync(
    //         new ArraySegment<byte>(frameData2),
    //         WebSocketMessageType.Text,
    //         true,
    //         cancellation
    //     );
    //     while (!task.IsCompleted)
    //     {
    //         yield return null;
    //     }
    //     task.GetAwaiter().GetResult();
    // }

    // private IEnumerator Receive(ClientWebSocket webSocket, Action<string> callback)
    // {
    //     string resp = "";
    //     byte[] receiveBuffer = new byte[2048];
    //     WebSocketReceiveResult result;
    //     do
    //     {
    //         Task<WebSocketReceiveResult> task = webSocket.ReceiveAsync(
    //             new ArraySegment<byte>(receiveBuffer),
    //             cancellation
    //         );
    //         while (!task.IsCompleted)
    //         {
    //             yield return null;
    //         }
    //         result = task.GetAwaiter().GetResult();

    //         // Process the received data...
    //         if (result.MessageType == WebSocketMessageType.Text)
    //         {
    //             string receivedMessage = Encoding.UTF8.GetString(
    //                 receiveBuffer,
    //                 0,
    //                 result.Count
    //             );
    //             //将结果构造为json

    //             JObject jsonObj = JObject.Parse(receivedMessage);
    //             int code = (int)jsonObj["header"]["code"];

    //             if (0 == code)
    //             {
    //                 int status = (int)jsonObj["payload"]["choices"]["status"];

    //                 JArray textArray = (JArray)jsonObj["payload"]["choices"]["text"];
    //                 string content = (string)textArray[0]["content"];
    //                 resp += content;
    //                 callback(content);
    //                 if (status != 2)
    //                 {
    //                     Debug.Log($"已接收到数据： {receivedMessage}");
    //                 }
    //                 else
    //                 {
    //                     Debug.Log($"最后一帧： {receivedMessage}");
    //                     int totalTokens = (int)
    //                         jsonObj["payload"]["usage"]["text"]["total_tokens"];
    //                     Debug.Log($"整体返回结果： {resp}");
    //                     Debug.Log($"本次消耗token数： {totalTokens}");
    //                     sendDataList.Add(new ChatContent() { role = "assistant", content = resp });
    //                     // callback(resp);
    //                     break;
    //                 }
    //             }
    //             else
    //             {
    //                 Debug.LogError($"请求报错： {receivedMessage}");
    //             }
    //         }
    //         else if (result.MessageType == WebSocketMessageType.Close)
    //         {
    //             Debug.Log("已关闭WebSocket连接");
    //             break;
    //         }
    //     }
    //     while (!result.CloseStatus.HasValue);
    // }

    // public async override void RequestMessage(Action<string> callback)
    // {
    //     using (ClientWebSocket webSocket = new ClientWebSocket())
    //     {
    //         try
    //         {
    //             await webSocket.ConnectAsync(new Uri(authUrl), cancellation);
    //             JsonRequest request = new JsonRequest
    //             {
    //                 header = new Header() { app_id = x_appid, uid = "12345" },
    //                 parameter = new Parameter()
    //                 {
    //                     chat = new Chat()
    //                     {
    //                         domain = xfDomain, //模型领域，默认为星火通用大模型
    //                         temperature = 0.5, //温度采样阈值，用于控制生成内容的随机性和多样性，值越大多样性越高；范围（0，1）
    //                         max_tokens = 1024, //生成内容的最大长度，范围（0，4096）
    //                     }
    //                 },
    //                 payload = new Payload()
    //                 {
    //                     message = new Message() { text = dataList }
    //                 }
    //             };

    //             string jsonString = JsonConvert.SerializeObject(request);

    //             //连接成功，开始发送数据
    //             var frameData2 = System.Text.Encoding.UTF8.GetBytes(jsonString.ToString());
    //             Debug.Log($"发送数据： {jsonString}");

    //             await webSocket.SendAsync(
    //                 new ArraySegment<byte>(frameData2),
    //                 WebSocketMessageType.Text,
    //                 true,
    //                 cancellation
    //             );

    //             // 接收流式返回结果进行解析
    //             byte[] receiveBuffer = new byte[2048];
    //             WebSocketReceiveResult result = await webSocket.ReceiveAsync(
    //                 new ArraySegment<byte>(receiveBuffer),
    //                 cancellation
    //             );
    //             string resp = "";
    //             while (!result.CloseStatus.HasValue)
    //             {
    //                 if (result.MessageType == WebSocketMessageType.Text)
    //                 {
    //                     string receivedMessage = Encoding.UTF8.GetString(
    //                         receiveBuffer,
    //                         0,
    //                         result.Count
    //                     );
    //                     //将结果构造为json

    //                     JObject jsonObj = JObject.Parse(receivedMessage);
    //                     int code = (int)jsonObj["header"]["code"];

    //                     if (0 == code)
    //                     {
    //                         int status = (int)jsonObj["payload"]["choices"]["status"];

    //                         JArray textArray = (JArray)jsonObj["payload"]["choices"]["text"];
    //                         string content = (string)textArray[0]["content"];
    //                         resp += content;
    //                         callback(content);
    //                         if (status != 2)
    //                         {
    //                             Debug.Log($"已接收到数据： {receivedMessage}");
    //                         }
    //                         else
    //                         {
    //                             Debug.Log($"最后一帧： {receivedMessage}");
    //                             int totalTokens = (int)
    //                                 jsonObj["payload"]["usage"]["text"]["total_tokens"];
    //                             Debug.Log($"整体返回结果： {resp}");
    //                             Debug.Log($"本次消耗token数： {totalTokens}");
    //                             dataList.Add(new Content() { role = "assistant", content = resp });
    //                             // callback(resp);
    //                             break;
    //                         }
    //                     }
    //                     else
    //                     {
    //                         Debug.LogError($"请求报错： {receivedMessage}");
    //                     }
    //                 }
    //                 else if (result.MessageType == WebSocketMessageType.Close)
    //                 {
    //                     Debug.Log("已关闭WebSocket连接");
    //                     break;
    //                 }

    //                 result = await webSocket.ReceiveAsync(
    //                     new ArraySegment<byte>(receiveBuffer),
    //                     cancellation
    //                 );
    //             }
    //         }
    //         catch (Exception e)
    //         {
    //             Debug.LogError("Exception: " + e);
    //         }
    //     }

    // }

    #endregion

    /// <summary>
    /// WebSocket协议通用鉴权URL生成
    /// https://www.xfyun.cn/doc/spark/general_url_authentication.html#_1-%E9%89%B4%E6%9D%83%E8%AF%B4%E6%98%8E
    /// </summary>
    /// <param name="hostUrl"></param>
    /// <returns></returns>
    string GetAuthUrl(string hostUrl)
    {
        string date = DateTime.UtcNow.ToString("r");

        Uri uri = new Uri(hostUrl);
        StringBuilder builder = new StringBuilder("host: ")
            .Append(uri.Host)
            .Append("\n")
            . //
            Append("date: ")
            .Append(date)
            .Append("\n")
            . //
            Append("GET ")
            .Append(uri.LocalPath)
            .Append(" HTTP/1.1");

        string sha = HMACsha256(sparkConfig.api_secret, builder.ToString());
        string authorization = string.Format(
            "api_key=\"{0}\", algorithm=\"{1}\", headers=\"{2}\", signature=\"{3}\"",
            sparkConfig.api_key,
            "hmac-sha256",
            "host date request-line",
            sha
        );
        //System.Web.HttpUtility.UrlEncode

        string NewUrl = "https://" + uri.Host + uri.LocalPath;

        string path1 =
            "authorization"
            + "="
            + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(authorization));
        date = date.Replace(" ", "%20").Replace(":", "%3A").Replace(",", "%2C");
        string path2 = "date" + "=" + date;
        string path3 = "host" + "=" + uri.Host;

        NewUrl = NewUrl + "?" + path1 + "&" + path2 + "&" + path3;
        return NewUrl.Replace("http://", "ws://").Replace("https://", "wss://");
    }

    /// <summary>
    /// HMACsha256加密
    /// </summary>
    /// <param name="apiSecretIsKey"></param>
    /// <param name="buider"></param>
    /// <returns></returns>
    string HMACsha256(string apiSecretIsKey, string buider)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(apiSecretIsKey);
        System.Security.Cryptography.HMACSHA256 hMACSHA256 =
            new System.Security.Cryptography.HMACSHA256(bytes);
        byte[] date = System.Text.Encoding.UTF8.GetBytes(buider);
        date = hMACSHA256.ComputeHash(date);
        hMACSHA256.Clear();

        return Convert.ToBase64String(date);
    }
}