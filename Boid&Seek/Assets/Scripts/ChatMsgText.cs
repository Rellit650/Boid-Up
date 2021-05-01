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
            float data;
            string[] stringArray = chatInput.text.Split(' ');
            if(stringArray.Length > 1)
            {
                Debug.Log("First: " + stringArray[0] + " Second: " + stringArray[1]);   //Splits string into parts based on spaces
                //TODO: check if first part of string array matches a command
                uint messageCheck = CheckCommands(stringArray[0]);
                if(messageCheck != 0)
                {
                    if(float.TryParse(stringArray[1],out data))   //Parses number in second half of array into float
                    {
                        //Debug.Log(data);
                        //Send admin command to server
                        NetMessage_AdminCommand command = new NetMessage_AdminCommand(messageCheck, data);
                        FindObjectOfType<PlayerScript>().SendMessage(command);
                    }

                }
            }
        }
        else
        {
            NetMessage_Chat message = new NetMessage_Chat(chatInput.text);
            FindObjectOfType<PlayerScript>().SendMessage(message);
        }
    }

    private uint CheckCommands(string chatCommand)
    {
        switch (chatCommand)
        {
            case "/setSpeed":
                return 1;
            case "/setJump":
                return 2;
                //Above is for the clients, below is for the server
            case "/setBoidUpdate":
                return 3;
            case "/setTagDistance":
                return 4;
            case "/setTagTimer":
                return 5;
            default:
                break;
        }
        return 0;
    }
}
