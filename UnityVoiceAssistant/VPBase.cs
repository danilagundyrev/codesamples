public class VPBase : MonoBehaviour
{
    protected const int WaitingClipsToExit = 2;
    
    protected VideoPlayer VideoPlayer;
    protected VideoClip CurrentVideoClip;
    
    protected int WaitingVideoClipsLength;
    protected int CurrentCounter = 0;

    protected bool IsWaiting = false;

    public Button[] MenuButtons;

    public UnityEvent OnActivateListenToUser;
    public UnityEvent OnExitToMainMenu;
    public UnityEvent OnDeactivateListenToUser;

    protected YSKResultHandler YskResultHandler;

    protected virtual void Awake()
    {
        VideoPlayer = GetComponent<VideoPlayer>();
        WaitingVideoClipsLength = VideoPlayersHandler.Instance.WaitingVideoClips.Length;
    }

    public void OnYandexRespond(string phrase)
    {
        int analysisResult = PhraseAnalysis.GetPhraseAnalysis(phrase);

        if (analysisResult == -2) // Invalid phrase
        {
            return;
        }

        if (analysisResult >= 0)
        {
            MenuHandler.Instance.ActivateCurrentMenu(analysisResult);
            VideoPlayersHandler.Instance.ActivateCurrentVideoPlayer(analysisResult);
        }
        else // No keyword matches found, invoke hesitation event
        {
            VideoPlayersHandler.Instance.HesitationEvent(analysisResult);                
        }
        
        ResetParameters();
        OnDeactivateListenToUser.Invoke();
    }
    
    protected virtual void ResetParameters()
    {
        CurrentCounter = 0;
        IsWaiting = false;
    }
    
    protected void PlayWaitClip()
    {
        VideoPlayer.clip = VideoPlayersHandler.Instance.WaitingVideoClips[Random.Range(0, WaitingVideoClipsLength)];
        CurrentVideoClip = VideoPlayer.clip;
        VideoPlayer.Play();
    }
}
