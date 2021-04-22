using UnityEngine.UI;
using UnityEngine;

public class ChatMsgText : MonoBehaviour
{
    [SerializeField] private InputField chatInput;

    public void OnChatSubmit()
    {
        NetMessage_Chat message = new NetMessage_Chat(chatInput.text);
        FindObjectOfType<PlayerScript>().SendMessage(message);
    }
}
