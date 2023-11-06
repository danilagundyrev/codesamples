/// <summary>
/// This class handles OvrPlatform launch needed for MetaAvatars and stores current user's ID
/// The script should exist in the scene where the use of MetaAvatars is needed
/// </summary>
public class MetaAvatarsManager : MonoBehaviour
{
    public UInt64 UserId;
    private const string logScope = "sampleAvatar";

    private void Start()
    {
        StartCoroutine(StartOvrPlatform());
    }

    private IEnumerator StartOvrPlatform()
    {
        if (OvrPlatformInit.status == OvrPlatformInitStatus.NotStarted)
        {
            OvrPlatformInit.InitializeOvrPlatform();
        }

        while (OvrPlatformInit.status != OvrPlatformInitStatus.Succeeded)
        {
            if (OvrPlatformInit.status == OvrPlatformInitStatus.Failed)
            {
                OvrAvatarLog.LogError($"Error initializing OvrPlatform. Falling back to local avatar", logScope);
                yield break;
            }

            yield return null;
        }

        // user ID == 0 means we want to load logged in user avatar from CDN
        if (UserId == 0)
        {
            // Get User ID
            Users.GetLoggedInUser().OnComplete(message =>
            {
                if (!message.IsError)
                {
                    UserId = message.Data.ID;
                    OvrAvatarEntitlement.ResendAccessToken();
                }
                else
                {
                    var e = message.GetError();
                    OvrAvatarLog.LogError($"Error loading CDN avatar: {e.Message}. Falling back to local avatar",
                        logScope);
                }
            });
        }
    }
    
    public static IEnumerator TunePlayerCameraCullingMask(bool isVisible)
    {
        Camera camera = Player.Instance.Head.GetComponent<Camera>();
        if (camera.cullingMask == (1 << LayerManager.Loading))
        {
            yield return new WaitUntil(() => camera.cullingMask != (1 << LayerManager.Loading));
        }
        TuneCameraCullingMask(isVisible);
    }

    public static void TuneCameraCullingMask(bool isVisible)
    {
        Camera camera = Player.Instance.Head.GetComponent<Camera>();
        LayerMask playerLayerMask = isVisible
            ? camera.cullingMask & ~(1 << LayerMask.NameToLayer("Hidden"))
            : camera.cullingMask | (1 << LayerMask.NameToLayer("Hidden"));
        camera.cullingMask = playerLayerMask;
    }
}
