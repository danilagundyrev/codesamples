public class VPProducts : VPBase
{
    private bool _isOfferPlayed = false;
    private int _offerVideoClipsLength;
    private Offer _currentOffer;

    protected override void Awake()
    {
        base.Awake(); 
        _offerVideoClipsLength = VideoPlayersHandler.Instance.OfferProductsVideoClips.Length;
    }
    
    private void OnEnable()
    {
        VideoPlayer.loopPointReached += OnVideoPlayed;
        ResetParameters();
        VideoPlayer.clip = VideoPlayersHandler.Instance.ProductsVideoClips;
        VideoPlayer.Play();
    }
    
    private void OnDisable()
    {
        VideoPlayer.loopPointReached -= OnVideoPlayed;
    }
    
    private void OnVideoPlayed(VideoPlayer vp)
    {
        CurrentCounter++;
        if (CurrentCounter <= WaitingClipsToExit)
        {
            PlayWaitClip();
            if (IsWaiting)
            {
                return;
            }

            IsWaiting = true;
            OnActivateListenToUser.Invoke();
            YskResultHandler.CurrentMode = _isOfferPlayed ? "ProductsOffer" : "Products";
        }
        else
        {
            if (!_isOfferPlayed)
            {
                PlayOfferClip();
                OnDeactivateListenToUser.Invoke();
                _isOfferPlayed = true;
            }
            else
            {
                OnDeactivateListenToUser.Invoke();
                OnExitToMainMenu.Invoke();
            }
            ResetParameters();
        }
    }
    
    public void OnOfferResponseReceived(string phrase)
    {
        if (phrase.Contains("YES"))
        {
            PlayOfferedVideo();
        }
        else if (phrase.Contains("NO"))
        {
            OnDeactivateListenToUser.Invoke();
            OnExitToMainMenu.Invoke();
        }
    }
    
    private void PlayOfferClip()
    {
        _currentOffer = VideoPlayersHandler.Instance.Offers[Random.Range(0, _offerVideoClipsLength)];
        VideoPlayer.clip = _currentOffer.OfferVideoClip;
        VideoPlayer.Play();
    }
    
    private void PlayOfferedVideo()
    {
        ResetParameters();
        OnDeactivateListenToUser.Invoke();

        if (_currentOffer != null)
        {
            _currentOffer.SetMainVideoClip();
            VideoPlayer.clip = _currentOffer.MainVideoClip;
            VideoPlayer.Play();
        }
    }
}
