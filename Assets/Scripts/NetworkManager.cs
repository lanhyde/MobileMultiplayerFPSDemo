using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Connection Status")] 
    public Text connectionStatusText;
    
    [Header("Login UI Panel")] 
    public InputField playerNameInput;
    public GameObject loginUIPanel;
    
    [Header("Game Options UI Panel")]
    public GameObject gameOptionsUIPanel;

    [Header("Create Room UI Panel")] 
    public GameObject createRoomUIPanel;
    public InputField roomNameInputField;
    public InputField maxPlayerInputField;
    
    [Header("Inside Room UI Panel")] 
    public GameObject insideRoomUIPanel;
    public Text roomInfoText;
    public GameObject playerEntityPrefab;
    public GameObject playerListContainer;
    
    [Header("Room List UI Panel")]
    public GameObject roomListUIPanel;
    public GameObject roomEntryPrefab;
    public GameObject roomListParentGameObject;
    public GameObject startGameButton;
    
    
    [Header("Join Random Room UI Panel")] 
    public GameObject joinRandomRoomUIPanel;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    private Dictionary<string, GameObject> roomListGameObjects = new Dictionary<string, GameObject>();
    private Dictionary<int, GameObject> playerListGameObjects = new Dictionary<int, GameObject>();
    #region Unity Methods
    // Start is called before the first frame update
    void Start()
    {
        ActivatePanel(loginUIPanel.name);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
        connectionStatusText.text = "Connection status: " + PhotonNetwork.NetworkClientState;
    }
    #endregion
    
    #region UI Callbacks

    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("Player name is invalid");
        }
    }

    public void OnCreateRoomButtonClicked()
    {
        string roomName = roomNameInputField.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = byte.Parse(maxPlayerInputField.text);
        
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public void OnCancelButtonClicked()
    {
        ActivatePanel(gameOptionsUIPanel.name);
    }

    public void OnShowRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(roomListUIPanel.name);
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(gameOptionsUIPanel.name);
        
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(joinRandomRoomUIPanel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartGameButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }
    #endregion
    
    #region Photon Callbacks

    public override void OnConnected()
    {
        Debug.Log("Connected to Internet");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " is connected to photon");
        ActivatePanel(gameOptionsUIPanel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " is created.");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + " joined to " + PhotonNetwork.CurrentRoom.Name);
        ActivatePanel(insideRoomUIPanel.name);

        startGameButton.SetActive(PhotonNetwork.LocalPlayer.IsMasterClient);

        roomInfoText.text =
            $"Room name: {PhotonNetwork.CurrentRoom.Name} Players/Max Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";

        if (playerListGameObjects == null)
        {
            playerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (var player in PhotonNetwork.PlayerList)
        {
            GameObject playerEntity = Instantiate(playerEntityPrefab);
            playerEntity.transform.SetParent(playerListContainer.transform);
            playerEntity.transform.localScale = Vector3.one;

            playerEntity.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;

            playerEntity.transform.Find("PlayerIndicator").gameObject
                .SetActive(player.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
            playerListGameObjects.Add(player.ActorNumber, playerEntity);   
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        roomInfoText.text =
            $"Room name: {PhotonNetwork.CurrentRoom.Name} Players/Max Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        GameObject playerEntity = Instantiate(playerEntityPrefab);
        playerEntity.transform.SetParent(playerListContainer.transform);
        playerEntity.transform.localScale = Vector3.one;

        playerEntity.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;
        playerEntity.transform.Find("PlayerIndicator").gameObject
            .SetActive(newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber);
        playerListGameObjects.Add(newPlayer.ActorNumber, playerEntity);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roomInfoText.text =
            $"Room name: {PhotonNetwork.CurrentRoom.Name} Players/Max Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}";
        Destroy(playerListGameObjects[otherPlayer.ActorNumber].gameObject);
        playerListGameObjects.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            startGameButton.SetActive(true);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomListView();
        
        foreach (var roomInfo in roomList)
        {
            if (!roomInfo.IsOpen || !roomInfo.IsVisible || roomInfo.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(roomInfo.Name))
                {
                    cachedRoomList.Remove(roomInfo.Name);
                }
            }
            else
            {
                if (cachedRoomList.ContainsKey(roomInfo.Name))
                {
                    cachedRoomList[roomInfo.Name] = roomInfo;
                }
                else
                {
                    cachedRoomList.Add(roomInfo.Name, roomInfo);
                }
            }
        }

        foreach (var room in cachedRoomList.Values)
        {
            GameObject roomInfoEntryGO = Instantiate(roomEntryPrefab);
            roomInfoEntryGO.transform.SetParent(roomListParentGameObject.transform);
            roomInfoEntryGO.transform.localScale = Vector3.one;

            roomInfoEntryGO.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomInfoEntryGO.transform.Find("RoomPlayersText").GetComponent<Text>().text =
                room.PlayerCount + " / " + room.MaxPlayers;
            roomInfoEntryGO.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() =>
            {
                OnJoinRoomButtonClicked(room.Name);
            });
            roomListGameObjects.Add(room.Name, roomInfoEntryGO);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomListView();
        cachedRoomList.Clear();
    }

    public override void OnLeftRoom()
    {
        ActivatePanel(gameOptionsUIPanel.name);
        foreach (var playerEntity in playerListGameObjects.Values)
        {
            Destroy(playerEntity);
        }
        playerListGameObjects.Clear();
        playerListGameObjects = null;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);

        string roomName = $"Room {Random.Range(1000, 10000)}";

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 20
        };
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    #endregion
    
    #region Public Methods

    public void ActivatePanel(string panelToBeActivated)
    {
        loginUIPanel.SetActive(panelToBeActivated.Equals(loginUIPanel.name));
        gameOptionsUIPanel.SetActive(panelToBeActivated.Equals(gameOptionsUIPanel.name));
        createRoomUIPanel.SetActive(panelToBeActivated.Equals(createRoomUIPanel.name));
        insideRoomUIPanel.SetActive(panelToBeActivated.Equals(insideRoomUIPanel.name));
        roomListUIPanel.SetActive(panelToBeActivated.Equals(roomListUIPanel.name));
        joinRandomRoomUIPanel.SetActive(panelToBeActivated.Equals(joinRandomRoomUIPanel.name));
    }
    #endregion

    #region Private Methods

    void OnJoinRoomButtonClicked(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    void ClearRoomListView()
    {
        foreach (var roomListGameObject in roomListGameObjects.Values)
        {
            Destroy(roomListGameObject);
        }
        roomListGameObjects.Clear();
    }

    #endregion
}
