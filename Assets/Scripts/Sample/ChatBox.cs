using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatBox : MonoBehaviour
{
    public TMP_Text tMP_Text;
    public RectTransform rect_Layout;

    void Awake()
    {
        tMP_Text = transform.GetComponentInChildren<TMP_Text>();
    }

    public void SetContent(string role, string content)
    {
        SetRole(role);
        SetText(content);
        RebuildLayout();
    }

    public void SetText(string text)
    {
        tMP_Text.text += text;
        RebuildLayout();
    }
    
    void RebuildLayout()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect_Layout);
        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }

    public void SetRole(string role)
    {
        tMP_Text.text = role + ": ";

        if (role == "user")
        {
            tMP_Text.text = "<color=green>" + tMP_Text.text + "</color>\n";
        }
        else
        {
            tMP_Text.text = "<color=blue>" + tMP_Text.text + "</color>\n";
        }
    }

}
