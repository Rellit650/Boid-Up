using UnityEngine.UI;
using UnityEngine;

public class ChatMsgText : MonoBehaviour
{
    [SerializeField] private InputField chatInput;

    public void OnChatSubmit()
    {
        if(chatInput.text.StartsWith("/"))
        {
            //Command
            Debug.Log("Send Command Here");
            int data;
            string[] stringArray = chatInput.text.Split(' ');
            if(stringArray.Length > 1)
            {
                Debug.Log("First: " + stringArray[0] + " Second: " + stringArray[1]);   //Splits string into parts based on spaces
                //TODO: check if first part of string array matches a command
                short messageCheck = CheckCommands(stringArray[0]);
                Debug.Log(messageCheck);
                if(int.TryParse(stringArray[1],out data))   //Parses number in second half of array into integer
                {
                    Debug.Log(data);
                }
            }
        }
        else
        {
            NetMessage_Chat message = new NetMessage_Chat(chatInput.text);
            FindObjectOfType<PlayerScript>().SendMessage(message);
        }
    }

    private short CheckCommands(string chatCommand)
    {
        switch (chatCommand)
        {
            case "/Teehee":
                Debug.Log("Command: Teehee");
                return 1;
            case "/Heehoo":
                Debug.Log("Command: Heehoo");
                return 2;
            default:
                break;
        }
        return 0;
    }
}
