using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour {

    public RectTransform UserGamePlayer;

    public static int playerXLocation;
    public static int playerYLocation;

    public GameObject OtherPlayers;

    public Dictionary<string, UserDataStruct> gameUsers;

    // Use this for initialization
    void Start () {
        gameUsers = new Dictionary<string, UserDataStruct>();
    }
	
	public void StartGame()
    {
        // Choose a random location for the user

        playerXLocation = Random.Range(0, 10);
        playerYLocation = Random.Range(0, 10);

        UpdateUserLocation(UserGamePlayer, playerXLocation, playerYLocation);

        FirebaseUtils.Instance.StartGettingUsersLocations();

        EventManagerScript.Instance.StartListening(EventManagerScript.EVENT__USER_LOCATION_UPDATED, OnUserLocationUpdate);
    }

    public void MovePlayerLeft()
    {
        if (playerXLocation > 0)
        {
            playerXLocation--;
        }
        else
        {
            playerXLocation = Constants.BOARD_SIZE - 1;
        }

        UpdateUserLocation(UserGamePlayer, playerXLocation, playerYLocation);
    }

    public void MovePlayerRight()
    {
        if (playerXLocation < (Constants.BOARD_SIZE - 1))
        {
            playerXLocation++;
        }
        else
        {
            playerXLocation = 0;
        }

        UpdateUserLocation(UserGamePlayer, playerXLocation, playerYLocation);
    }

    public void MovePlayerDown()
    {
        if (playerYLocation > 0)
        {
            playerYLocation--;
        }
        else
        {
            playerYLocation = Constants.BOARD_SIZE - 1;
        }

        UpdateUserLocation(UserGamePlayer, playerXLocation, playerYLocation);
    }

    public void MovePlayerUp()
    {
        if (playerYLocation < (Constants.BOARD_SIZE - 1))
        {
            playerYLocation++;
        }
        else
        {
            playerYLocation = 0;
        }

        UpdateUserLocation(UserGamePlayer, playerXLocation, playerYLocation);
    }

    private void UpdateUserLocation(RectTransform userRT, int xLocation, int yLocation)
    {
        float newBoardPositionX = (Constants.SCREEN_WIDTH / Constants.BOARD_SIZE) * xLocation - (Constants.SCREEN_WIDTH / 2) + (UserGamePlayer.sizeDelta.x / 2);
        float newBoardPositionY = ((Constants.SCREEN_HEIGHT - 100) / Constants.BOARD_SIZE) * yLocation - (Constants.SCREEN_HEIGHT / 2) + (UserGamePlayer.sizeDelta.y / 2);

        Debug.Log("newBoardPositionX: " + newBoardPositionX + ", newBoardPositionY: " + newBoardPositionY);

        userRT.anchoredPosition = new Vector2(newBoardPositionX, newBoardPositionY);

        // Only if it's us update the server
        if (userRT == UserGamePlayer)
        {
            FirebaseUtils.Instance.SetUserLocation(xLocation, yLocation);
        }
    }

    private void OnUserLocationUpdate(object obj)
    {
        UserDataStruct userData = obj as UserDataStruct;

        Debug.Log("User : " + userData.id + " location is updated. x: " + userData.x + ", y: " + userData.y);

        UserDataStruct userOldRecord = new UserDataStruct();

        if (gameUsers.TryGetValue(userData.id, out userOldRecord)) {
            // This user exists - update its data
            userOldRecord.x = userData.x;
            userOldRecord.y = userData.y;
        }
        else
        {
            // This is a new user - insert it
            gameUsers.Add(userData.id, userData);
        }

        UpdateOtherUsersLocation();
    }

    private void UpdateOtherUsersLocation()
    {
        // Bad bad practice :-(

        if (OtherPlayers != null)
        {
            // Remove all users
            foreach (Transform child in OtherPlayers.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }

        // Go over the users and show them on screen
        foreach (KeyValuePair<string, UserDataStruct> entry in gameUsers)
        {
            // Create a user
            GameObject otherUserGO = Instantiate(Resources.Load("OtherUser")) as GameObject;

            // Add him to the users hierarchy
            otherUserGO.transform.SetParent(OtherPlayers.transform, false);

            // Set the user location
            UpdateUserLocation(otherUserGO.transform as RectTransform, entry.Value.x, entry.Value.y);
        }
    }
}
