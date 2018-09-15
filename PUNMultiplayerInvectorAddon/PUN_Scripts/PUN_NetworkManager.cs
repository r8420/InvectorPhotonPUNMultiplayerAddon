using UnityEngine;
using Photon.Pun;               //to acces Photon features
using Photon.Realtime;          //to access Photon callbacks
using UnityEngine.Events;       //to call actions on various states

public class PUN_NetworkManager : MonoBehaviourPunCallbacks
{

    [Tooltip("The version to connect with. Incompatible versions will not connect with each other.")]
    public string _gameVersion = "1.0";
    [Tooltip("The max number of player per room. When a room is full, it can't be joined by new players, and so a new room will be created.")]
    [SerializeField] private byte maxPlayerPerRoom = 4;
    [Tooltip("The _prefab that will be spawned in when a player successfully connects.")]
    public GameObject _playerPrefab = null;
    [Tooltip("The point where the player will start when they have successfully connected.")]
    public Transform _spawnPoint = null;
    [Tooltip("Shows the current connection process. This is great for UI to reference and use.")]
    public string _connectStatus = "";
    [Tooltip("Automatically sync all connected clients scenes. Make sure everyone is always on the same scene together.")]
    public bool _syncScenes = true;

    public UnityEvent _onJoinedRoom;
    public UnityEvent _onLeftRoom;
    public UnityEvent _onPlayerEnteredRoom;
    public UnityEvent _onPlayerLeftRoom;
    [HideInInspector] public bool _connecting = false;

    #region Internal Use Variables
    private PUN_NetworkManager nm = null;
    #endregion

    private void Awake()
    {
        if (nm == null)
        {
            nm = this;
            DontDestroyOnLoad(this.gameObject);
            this.gameObject.name = gameObject.name + " Instance";
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        PhotonNetwork.AutomaticallySyncScene = _syncScenes; //Automatically load scenes together (make sure everyone is always on the same scene)
    }

    #region Callable Methods
    /// <summary>
    /// Set the players network name
    /// </summary>
    /// <param name="name"></param>
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }
        PhotonNetwork.NickName = name;
    }

    /// <summary>
    /// Connect to a random Room. If none are available, will create one and connect to that.
    /// </summary>
    public void Connect()
    {
        _connecting = true;
        _connectStatus = "Finding a room...";
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.GameVersion = _gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    
    /// <summary>
    /// Makes the caller leave the room they are connected to.
    /// </summary>
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    
    /// <summary>
    /// Returns the numbers of players connected to this room
    /// </summary>
    /// <returns></returns>
    public int GetPlayerCount()
    {
        return PhotonNetwork.CurrentRoom.PlayerCount;
    }

    /// <summary>
    /// Loads level on all connected clients
    /// </summary>
    /// <param name="level"></param>
    public void NetworkLoadLevel(int level)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(level);
        }
    }
    #endregion

    #region Photon Callback Methods
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        _onPlayerEnteredRoom.Invoke();
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        _onPlayerLeftRoom.Invoke();
        base.OnPlayerLeftRoom(otherPlayer);
    }

    public override void OnConnectedToMaster()
    {
        if (_connecting == true)
        {
            _connectStatus = "Connected to master server, attemping to find a room...";
            PhotonNetwork.JoinRandomRoom();
        }
        base.OnConnectedToMaster();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        _connecting = false;
        _connectStatus = "Disconnected: " + cause;
        PhotonNetwork.DestroyPlayerObjects(_playerPrefab.GetComponent<PhotonView>().ViewID, _playerPrefab);
        base.OnDisconnected(cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        _connectStatus = "Failed to find a room, creating one...";
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayerPerRoom });
        base.OnJoinRandomFailed(returnCode, message);
    }

    public override void OnJoinedRoom()
    {
        _connecting = false;
        _connectStatus = "Successfully joined a room";
        if (_playerPrefab != null)
        {
            PhotonNetwork.Instantiate(_playerPrefab.name, _spawnPoint.position, _spawnPoint.rotation, 0);
        }
        _onJoinedRoom.Invoke();
        base.OnJoinedRoom();
    }

    public override void OnLeftRoom()
    {
        _onLeftRoom.Invoke();
        PhotonNetwork.DestroyPlayerObjects(_playerPrefab.GetComponent<PhotonView>().ViewID, _playerPrefab);
        base.OnLeftRoom();
    }
    #endregion
}
