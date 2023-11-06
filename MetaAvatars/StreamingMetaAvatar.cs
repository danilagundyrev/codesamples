/// <summary>
/// This class is an extension of Oculus' OvrAvatarEntity that handles MetaAvatars behaviour.
/// It handles MetaAvatar loading into the scene and streaming MetaAvatar data.
/// </summary>
public class StreamingMetaAvatar : MetaAvatarBase
{
    public Action OnMetaAvatarSkeletonCreated;
    public MetaAvatarNetworkSync NetworkSync;
    public byte[] AvatarBytes;
    public bool IsAvatarBytesInitialized;

    protected override void StartLoadingAvatar()
    {
        NetworkSync = GetComponentInParent<MetaAvatarNetworkSync>();
        _userId = Convert.ToUInt64(NetworkSync.UserID);
        IsLocalPlayer = NetworkSync.Player.IsLocal;

        if (IsLocalPlayer)
        {
            SetBodyTracking(FindObjectOfType<XRInputManager>());
            SetLipSync(FindObjectOfType<OvrAvatarLipSyncContext>());
        }
        StartCoroutine(LoadAvatarWithId());
    }

    public void OnAvatarCreated()
    {
        MetaAvatarVisibilitySetup();
        StartCoroutine(StreamAvatarData());
    }

    public void OnSkeletonCreated()
    {
        OnMetaAvatarSkeletonCreated?.Invoke();
    }

    /// <summary>
    /// Checks if the NetworkArray of bytes is successfully received, then initializes local bytes array
    /// which should be played by this entity
    /// </summary>
    /// <returns></returns>
    private bool InitializeRemoteAvatarBytesArray()
    {
        if (IsAvatarBytesInitialized)
        {
            return true;
        }

        if (NetworkSync.AvatarDataCount == 0)
        {
            IsAvatarBytesInitialized = false;
            return false;
        }

        if (AvatarBytes.Length == 0)
        {
            IsAvatarBytesInitialized = false;
            AvatarBytes = new byte[NetworkSync.AvatarDataCount];
            return false;
        }

        IsAvatarBytesInitialized = true;
        return true;
    }

    /// <summary>
    /// As local avatar, we record our data and share it across the network through the MetaAvatarNetworkSync object
    /// As remote avatar, we get the data from MetaAvatarNetworkSync object that has been shared across the network and play it 
    /// </summary>
    /// <returns></returns>
    private IEnumerator StreamAvatarData()
    {
        if (IsLocalPlayer)
        {
            AvatarBytes = RecordStreamData(activeStreamLod); // grab our avatar animation data as bytes array
            NetworkSync.AvatarDataCount = AvatarBytes.Length;
            NetworkSync.AvatarData.CopyFrom(AvatarBytes, 0, AvatarBytes.Length);
        }
        else
        {
            if (!InitializeRemoteAvatarBytesArray())
            {
                yield return null;
            }
            else
            {
                PlayAvatarData();
            }
        }

        yield return new WaitForEndOfFrame();
        StartCoroutine(StreamAvatarData());
    }

    /// <summary>
    /// Changes avatar's active view (first person or third person). Normally a local avatar has first person view,
    /// which should be changed to the third person view (with a visible head) when the user wants to take a picture of himself.
    /// </summary>
    /// <param name="view">FirstPerson or ThirdPerson</param>
    public void ChangeAvatarActiveView(string view = "FirstPerson")
    {
        switch (view)
        {
            case "FirstPerson":
                SetActiveView(CAPI.ovrAvatar2EntityViewFlags.FirstPerson);
                break;
            case "ThirdPerson":
                SetActiveView(CAPI.ovrAvatar2EntityViewFlags.ThirdPerson);
                break;
            default:
                Logger.LogWarning($"Invalid view parameter {view}, please check its value");
                break;
        }
    }

    /// <summary>
    /// Copies NetworkArray of bytes into local bytes array and plays it on this entity
    /// </summary>
    private void PlayAvatarData()
    {
        SetPlaybackTimeDelay(0.1f);
        Array.Copy(NetworkSync.AvatarData.ToArray(), 0, AvatarBytes, 0, NetworkSync.AvatarDataCount);
        ApplyStreamData(AvatarBytes);
    }
}
