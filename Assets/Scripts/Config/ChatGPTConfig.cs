using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "ChatGPTConfig", menuName = "NLPConfig/ChatGPTConfig")]
public class ChatGPTConfig : NLPConfig
{
    ChatGPTConfig()
    {
        prompt = "使用中文回答,并移除回答中的Markdown标示";
        modelName = "OpenAI";
    }

    public string OpenAIUrl = "https://api.openai.com/v1/chat/completions";
    public string api_key;
    public string OpenAIModel = "gpt-3.5-turbo";

    void OnValidate()
    {

    }
}
