public abstract class MetaAvatarBase : OvrAvatarEntity
{
    public bool IsLocalPlayer;
    protected override void Awake()
    {
        base.Awake();
        StartLoadingAvatar();
    }

    protected void OnDisable()
    {
        if (IsLocalPlayer)
        {
            MetaAvatarsManager.TuneCameraCullingMask(false);
        }
    }

    /// <summary>
    /// Initial avatar setup of the user Id and avatar status (local or remote)
    /// </summary>
    protected abstract void StartLoadingAvatar();
    
    protected IEnumerator LoadAvatarWithId()
    {
        var hasAvatarRequest = OvrAvatarManager.Instance.UserHasAvatarAsync(_userId);
        while (!hasAvatarRequest.IsCompleted)
        {
            yield return null;
        }

        LoadUser();
    }
    
    protected override void OnDefaultAvatarLoaded()
    {
        MetaAvatarVisibilitySetup();
    }
    
    protected override void OnUserAvatarLoaded()
    {
        MetaAvatarVisibilitySetup();
    }
    
    /// <summary>
    /// We want to set a specific layer for the avatar mesh with a rendered head
    /// so it would be visible for the screenshot camera, but not for the player center eye camera
    /// </summary>
    protected virtual void MetaAvatarVisibilitySetup()
    {
        StartCoroutine(MetaAvatarsManager.TunePlayerCameraCullingMask(true));
        OvrAvatarGpuSkinnedRenderable[] childrenMeshes =
            GetComponentsInChildren<OvrAvatarGpuSkinnedRenderable>(true);

        if (childrenMeshes.Length == 0)
        {
            Logger.Log("Couldn't get Avatar meshes, something went wrong");
            return;
        }

        foreach (OvrAvatarGpuSkinnedRenderable mesh in childrenMeshes)
        {
            int alwaysVisibleLayer = LayerMask.NameToLayer("AlwaysVisible");
            mesh.gameObject.layer = alwaysVisibleLayer;
            if (mesh.name.Contains("V1") && isLocalPlayer)
            {
                int hiddenLayer = LayerMask.NameToLayer("Hidden");
                mesh.gameObject.layer = hiddenLayer;
            }
        }
    }
}
