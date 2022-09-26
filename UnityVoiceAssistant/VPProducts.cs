using UnityEngine;
using UnityEngine.Video;

public class VPProducts : VPBase
{
    //Specific counters and parameters for the current state tracking
    private bool isOfferPlayed = false;
    private int _offerVCsLength;
    private Offer _currentOffer;

    protected override void Awake()
    {
        base.Awake()
        _offerVCsLength = VideoPlayersHandler.instance.OfferProductsVCs.Length;
    }
    
    private void OnVideoPlayedEvent(VideoPlayer vp)
    {
        _currentCounter++;
        if (_currentCounter <= WaitingClipsToExit)
        {
            PlayWaitClip();
            if (isWaiting)
            {
                return;
            }
            isWaiting = true;
            
            //Invoke MicActivation event (listen to user)
            ActivateListenToUser.Invoke();
            
            //Change YSKResult mode
            YSKResult.CurrentMode = (isOfferPlayer) ? "ProductsOffer" : "Products"
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
        if (phrase.Contains("YES"))
        {
            ResetParametersBase();
            DiactivateListenToUser.Invoke();

            //Play offered video
            if (_currentOffer != null)
            {
                _currentOffer.SetMainVC();
            }
        }
        
        if (phrase.Contains("NO"))
        {
            ResetParametersBase();
            DiactivateListenToUser.Invoke();
            ExitToMainMenu.Invoke();
        }
    }
    
    private void PlayOfferClip()
    {
        //Set offer clips
        _currentOffer = VideoPlayersHandler.instance.Offers[Random.Range(0, _offerVCsLength)];
        _vp.clip = _currentOffer.OfferVC;
        _currentVideoClip = _vp.clip;
        _vp.Play();
    }
    
    private void OnEnable()
    {
        //Subscribe VideoPlayer to the event
        _vp.loopPointReached += OnVideoPlayedEvent;

        //set basic parameters to their initial values
        ResetParametersBase();

        //set initial VC and play
        _vp.clip = VideoPlayersHandler.instance.ProductsVC;
        _vp.Play();
    }
    
    private void OnDisable()
    {
        //Unsubcribe to the event
        _vp.loopPointReached -= OnVideoPlayedEvent;
    }
}
