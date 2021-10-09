using UnityEngine;
using UnityEngine.Video;

public class VPProducts : VPBasic
{
    //Specific counters and parameters for the current state tracking
    private bool isOfferPlayed = false;
    private int _offerVCsLength;
    private Offer _currentOffer;

    private void Awake()
    {
        _vp = GetComponent<VideoPlayer>();
        _waitingVCsLength = VideoPlayersHandler.instance.WaitingVCs.Length;
        _offerVCsLength = VideoPlayersHandler.instance.OfferProductsVCs.Length;
    }
    public void OnVideoPlayedEvent(VideoPlayer vp)
    {
        _currentCounter++;
        if (_currentCounter <= WaitingClipsToExit)
        {
            PlayWaitClip();
            if (!isWaiting)
            {
                isWaiting = true;

                //Invoke MicActivation event (listen to user)
                ActivateListenToUser.Invoke();

                //Change YSKResult mode
                if (!isOfferPlayed)
                {
                    YSKResult.CurrentMode = "Products";
                }
                else
                {
                    YSKResult.CurrentMode = "ProductsOffer";
                }
            }
        }
        else //all waiting clips have been played
        {
            if (!isOfferPlayed) //if the offer hasn't been played yet, then play it!
            {
                //play offer clip
                PlayOfferClip();
                DiactivateListenToUser.Invoke();

                //reset parameters
                ResetParametersBase();
                isOfferPlayed = true;
            }
            else //if offer has been played and no response received from a user, then exit
            {
                //reset parameters
                ResetParametersBase();

                //invoke exit event
                DiactivateListenToUser.Invoke();
                ExitToMainMenu.Invoke();
            }
        }
    }
    public void OnOfferRespondReceived(string phrase) //Invoke this method from YSKResultHandler
    {
        if (phrase.Contains("YES PLEASE"))
        {
            ResetParameters();
            DiactivateListenToUser.Invoke();

            //Play offered video
            if (_currentOffer != null)
            {
                _currentOffer.SetMainVC();
            }
        }
        if (phrase.Contains("NO THANKS"))
        {
            ResetParameters();
            DiactivateListenToUser.Invoke();
            ExitToMainMenu.Invoke();
        }
    }
    private void OnEnable()
    {
        //Subscribe VideoPlayer to the event
        _vp.loopPointReached += OnVideoPlayedEvent;

        //set basic parameters to their initial values
        ResetParameters();

        //set initial VC and play
        _vp.clip = VideoPlayersHandler.instance.ProductsVC;
        _vp.Play();
    }
    private void OnDisable()
    {
        //Unsubcribe to the event
        _vp.loopPointReached -= OnVideoPlayedEvent;
    }
    public void PlayOfferClip()
    {
        //Set offer clips
        _currentOffer = VideoPlayersHandler.instance.Offers[Random.Range(0, _offerVCsLength)];
        _vp.clip = _currentOffer.OfferVC;
        _currentVideoClip = _vp.clip;
        _vp.Play();
    }
}
