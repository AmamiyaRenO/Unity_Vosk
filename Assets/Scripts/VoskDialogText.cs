using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class VoskDialogText : MonoBehaviour 
{
    public VoskSpeechToText VoskSpeechToText;
    public Text DialogText;

    // Game states
    private enum GameState
    {
        ROOM_ENTRANCE,    // Room entrance
        ROOM_CENTER,      // Room center
        ROOM_BOOKSHELF,   // In front of bookshelf
        ROOM_DESK,        // In front of desk
        ROOM_DOOR         // In front of door
    }

    private GameState currentState = GameState.ROOM_ENTRANCE;
    private bool hasKey = false;
    private bool hasReadNote = false;

    // Command regex patterns
    private Dictionary<string, Regex> commandRegexes = new Dictionary<string, Regex>()
    {
        {"look", new Regex(@"look|see|check|examine")},
        {"take", new Regex(@"take|get|pick|grab")},
        {"use", new Regex(@"use|open|unlock")},
        {"move", new Regex(@"go|move|walk|head")},
        {"read", new Regex(@"read|check")},
        {"help", new Regex(@"help|hint|what can i do")}
    };

    // Location regex patterns
    private Dictionary<string, Regex> locationRegexes = new Dictionary<string, Regex>()
    {
        {"center", new Regex(@"center|middle|center of the room")},
        {"bookshelf", new Regex(@"bookshelf|shelf|book case")},
        {"desk", new Regex(@"desk|table")},
        {"door", new Regex(@"door|exit")}
    };

    void Awake()
    {
        VoskSpeechToText.OnTranscriptionResult += OnTranscriptionResult;
        ShowWelcomeMessage();
    }

    void ShowWelcomeMessage()
	{
        string welcomeMessage = "You find yourself in a mysterious room.\n" +
                              "There is a bookshelf, a desk, and a door in the room.\n" +
                              "You can explore the room using voice commands:\n" +
                              "- Say 'look' or 'examine' to look around\n" +
                              "- Say 'go to bookshelf' or 'go to desk' to move\n" +
                              "- Say 'help' to get hints\n\n" +
                              "Try saying 'look' to start exploring!";
        AddResponse(welcomeMessage);
    }

    void AddResponse(string response, bool showActions = false)
    {
        DialogText.text = response + "\n";
        DialogText.text += GetCurrentLocationDescription() + "\n";
        if (showActions)
        {
            DialogText.text += GetAvailableActions();
        }
    }

    string GetCurrentLocationDescription()
    {
        switch (currentState)
        {
            case GameState.ROOM_ENTRANCE:
                return "Current Location: Room Entrance";
            case GameState.ROOM_CENTER:
                return "Current Location: Room Center";
            case GameState.ROOM_BOOKSHELF:
                return "Current Location: In front of Bookshelf";
            case GameState.ROOM_DESK:
                return "Current Location: In front of Desk";
            case GameState.ROOM_DOOR:
                return "Current Location: In front of Door";
            default:
                return "";
	}
    }

    string GetAvailableActions()
    {
        string availableActions = "\nAvailable actions:";
        switch (currentState)
        {
            case GameState.ROOM_BOOKSHELF:
                if (!hasKey)
                    availableActions += "\n- Say 'take book' to move the special book";
                break;
            case GameState.ROOM_DESK:
                if (!hasReadNote)
                    availableActions += "\n- Say 'read note' to read what's written";
                break;
            case GameState.ROOM_DOOR:
                if (hasKey)
                    availableActions += "\n- Say 'use key' to unlock the door";
                break;
        }
        // 始终显示所有可去的地方
        availableActions += "\n- Say 'go to desk' to check the desk";
        availableActions += "\n- Say 'go to bookshelf' to check the bookshelf";
        availableActions += "\n- Say 'go to door' to check the door";
        availableActions += "\n- Say 'go to center' to go to the center of the room";
        // 通用操作
        availableActions += "\n- Say 'look' to look around again\n";
        return availableActions;
	}

    private void OnTranscriptionResult(string obj)
    {
        Debug.Log(obj);
        var result = new RecognitionResult(obj);

        foreach (var phrase in result.Phrases)
        {
            if (string.IsNullOrEmpty(phrase.Text)) continue;
            string command = phrase.Text.ToLower();
            if (ProcessCommand(command))
                break; // 只要有一个短语被处理就停止
        }
    }

    bool ProcessCommand(string command)
    {
        if (commandRegexes["help"].IsMatch(command)) { ShowHelp(); return true; }
        if (commandRegexes["look"].IsMatch(command)) { LookAround(); return true; }
        if (commandRegexes["move"].IsMatch(command))
        {
            foreach (var location in locationRegexes)
            {
                if (location.Value.IsMatch(command))
                {
                    MoveTo(location.Key);
                    return true;
                }
            }
            AddResponse("Where do you want to go? Try saying 'go to bookshelf', 'go to desk', or 'go to door'.");
            return true;
			}
        if (commandRegexes["take"].IsMatch(command)) { TryTakeItem(command); return true; }
        if (commandRegexes["use"].IsMatch(command)) { TryUseItem(command); return true; }
        if (commandRegexes["read"].IsMatch(command)) { TryReadItem(command); return true; }
        return false;
    }

    void ShowHelp()
    {
        string helpMessage = "You can use the following commands:\n" +
                           "- 'look' or 'examine': Look around\n" +
                           "- 'go to bookshelf'/'go to desk'/'go to door': Move to a location\n" +
                           "- 'take book': Pick up items\n" +
                           "- 'use key': Use items\n" +
                           "- 'read note': Read items\n" +
                           "- 'help': Show this help message";
        AddResponse(helpMessage);
    }

    void LookAround()
    {
        string locationDescription = "";
        switch (currentState)
        {
            case GameState.ROOM_ENTRANCE:
                locationDescription = "You are at the entrance of the room. There is a desk in the center, a bookshelf to the right, and a door in the distance.";
                break;
            case GameState.ROOM_CENTER:
                locationDescription = "You are in the center of the room. You can see every corner clearly. There seems to be something on the desk.";
                break;
            case GameState.ROOM_BOOKSHELF:
                locationDescription = "The bookshelf is filled with books.";
                if (!hasKey)
                    locationDescription += " One book looks different and seems to be movable.";
                else
                    locationDescription += " You have already found the key.";
                break;
            case GameState.ROOM_DESK:
                if (!hasReadNote)
                    locationDescription = "There is a note on the desk with some writing on it.";
                else
                    locationDescription = "There is nothing special on the desk anymore.";
                break;
            case GameState.ROOM_DOOR:
                if (hasKey)
                    locationDescription = "This is a regular door. Now that you have the key, you can try to open it.";
                else
                    locationDescription = "This is a locked door. You need a key to open it.";
                break;
        }
        AddResponse(locationDescription, true);
    }

    void MoveTo(string location)
    {
        switch (location)
        {
            case "center":
                currentState = GameState.ROOM_CENTER;
                AddResponse("You walk to the center of the room.", true);
                break;
            case "bookshelf":
                currentState = GameState.ROOM_BOOKSHELF;
                AddResponse("You walk to the bookshelf.", true);
                break;
            case "desk":
                currentState = GameState.ROOM_DESK;
                AddResponse("You walk to the desk.", true);
                break;
            case "door":
                currentState = GameState.ROOM_DOOR;
                AddResponse("You walk to the door.", true);
                break;
			}
    }

    void TryTakeItem(string command)
    {
        if (currentState == GameState.ROOM_BOOKSHELF && !hasKey && command.Contains("book"))
        {
            hasKey = true;
            AddResponse("You move the special book and find a key hidden behind it!", true);
        }
        else
        {
            AddResponse("There is nothing to take here.", true);
			}
        }

    void TryUseItem(string command)
    {
        if (currentState == GameState.ROOM_DOOR && hasKey && command.Contains("key"))
        {
            AddResponse("You use the key to open the door! Congratulations, you have solved the puzzle!\n\n" +
                       "Game Over. You can start a new game or try to discover other hidden secrets.", true);
        }
        else
        {
            AddResponse("There is nothing to use here.", true);
        }
    }

    void TryReadItem(string command)
    {
        if (currentState == GameState.ROOM_DESK && !hasReadNote && command.Contains("note"))
        {
            hasReadNote = true;
            AddResponse("You read the note: 'To leave this room, you need to find a key. The key is hidden somewhere...'", true);
        }
        else if (currentState == GameState.ROOM_DESK && hasReadNote && command.Contains("note"))
        {
            AddResponse("You have already read the note.", true);
        }
        else
        {
            AddResponse("There is nothing to read here.", true);
        }
    }
}
