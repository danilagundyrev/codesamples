/// <summary>
/// This class represents network avatar entity which is mainly used to record
/// local avatar stream data and share it across the network.
/// </summary>
public class MetaAvatarNetworkSync : MultiplayerAvatar
{
    private GameObject _avatarPresetLocal;
    private GameObject _avatarPresetRemote;
    
    [Networked] public UInt64 UserID { get; set; }

    [Networked][Capacity(1200)] 
    public NetworkArray<byte> AvatarData { get; }
    [Networked] public int AvatarDataCount { get; set; }
    
    private StreamingMetaAvatar _streamingMetaAvatar;
    public StreamingMetaAvatar StreamingMetaAvatar => _streamingMetaAvatar;
    private Transform _chestTransform;
    private Coroutine _checkingMetaAvatarCoroutine;

    protected override void Awake()
    {
        base.Awake();
        
        _avatarPresetLocal = Resources.Load<GameObject>("Prefabs/Multiplayer/LocalAvatar");
        _avatarPresetRemote = Resources.Load<GameObject>("Prefabs/Multiplayer/RemoteAvatar");
    }

    public override void HideAvatar(bool isHidden)
    {
        base.HideAvatar(isHidden);

        //Oculus avatar initialization
        if (!isHidden && _streamingMetaAvatar == null)
        {
            SpawnMetaAvatar();
        }
        else
        {
            bool isStreamingAvatarNull = _streamingMetaAvatar == null;
            Logger.Log($"MetaAvatarNetworkSync: Meta Avatar initializing for player {Player.Name} failed. isHidden value: {isHidden}, isStreamingAvatarNull value: {isStreamingAvatarNull}");
            if (_checkingMetaAvatarCoroutine != null)
            {
                StopCoroutine(_checkingMetaAvatarCoroutine);
            }
            _checkingMetaAvatarCoroutine = StartCoroutine(CheckMetaAvatarPresence_Co());
        }
    }

    public override void Spawned()
    {
        base.Spawned();
        _checkingMetaAvatarCoroutine = StartCoroutine(CheckMetaAvatarPresence_Co());
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (_streamingMetaAvatar != null)
        {
            _streamingMetaAvatar.OnMetaAvatarSkeletonCreated -= OnMetaAvatarCreated;
        }
        
        if (_checkingMetaAvatarCoroutine != null)
        {
            StopCoroutine(_checkingMetaAvatarCoroutine);
        }

        base.Despawned(runner, hasState);
    }

    /// <summary>
    /// Spawns Meta avatar prefab (two different preset variants for local and remote player)
    /// </summary>
    private void SpawnMetaAvatar()
    {
        Logger.Log($"MetaAvatarNetworkSync: Meta Avatar initializing for player {Player.Name}");
        GameObject avatar = Instantiate(Player.IsLocal ? _avatarPresetLocal : _avatarPresetRemote, transform.position,
            transform.rotation, _mesh.transform);

        _streamingMetaAvatar = avatar.GetComponent<StreamingMetaAvatar>();
        _streamingMetaAvatar.OnMetaAvatarSkeletonCreated += OnMetaAvatarCreated;
    }

    private void OnMetaAvatarCreated()
    {
        _chestTransform = _streamingMetaAvatar.transform.Find("Joint Chest");
    }
    
    /// <summary>
    /// Checks presence of the Meta avatar, respawns the avatar if it wasn't spawned
    /// </summary>
    /// <returns></returns>
    private IEnumerator CheckMetaAvatarPresence_Co()
    {
        while (true)
        {
            Logger.Log($"Checking meta avatar presence for player {Player.Name}");
            if (MultiplayerPlayer.LocalPlayer && !MultiplayerPlayer.LocalPlayer.InGame)
            {
                if (!_mesh.activeSelf)
                {
                    Logger.Log($"Meta avatar mesh container for player {Player.Name} has been turned off, switching to turn on");
                    _mesh.SetActive(true);
                }

                if (_mesh.transform.childCount == 0)
                {
                    Logger.Log($"Meta avatar container is empty: the avatar of the player {Player.Name} is not initialized, spawning new avatar");
                    SpawnMetaAvatar();
                }

                bool isMetaAvatar = _mesh.GetComponentInChildren<StreamingMetaAvatar>();
                if (isMetaAvatar && _mesh.GetComponentInChildren<StreamingMetaAvatar>().transform.childCount == 0)
                {
                    Logger.Log($"Streaming Meta Avatar for player {Player.Name} exists, but the avatar parts are not there, respawning the avatar");
                    Destroy(_mesh.transform.GetChild(0).gameObject);
                    SpawnMetaAvatar();
                }
            }

            yield return new WaitForSecondsRealtime(5f);
        }
    }
}
