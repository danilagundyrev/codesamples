using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;

public class VPBase : MonoBehaviour
{
    //VideoPlayer and current VideoClip
    protected VideoPlayer _vp;
    protected VideoClip _currentVideoClip;

    //Counters and parameters for the current state tracking
    protected const int _waitingClipsToExit = 2;
    protected int _waitingVCsLength;
    protected int _currentCounter = 0;

    protected bool isWaiting = false;

    public Button[] MenuButtons;

    //Basic events
    public UnityEvent ActivateListenToUser;
    public UnityEvent ExitToMainMenu;
    public UnityEvent DiactivateListenToUser;

    //Yandex Result Handler reference
    public YSKResultHandler YSKResult;


    public void OnYandexRespond(string phrase) //Invoke this method from YSKResultHandler
    {
        //process the received string from Yandex Cloud Service
        int temp = PhraseAnalysis.GetPhraseAnalysis(phrase);

        if (temp == -2) //empty string or string's length is less than 2 symbols
        {
          return;
        }
        if (temp >= 0)
        {
           //diactivate all menus and activate corresponding menu
           MenuHandler.instance.ActivateCurrentMenu(temp);

           //activate video player
           VideoPlayersHandler.instance.ActivateCurrentVideoPlayer(temp);
        }
        else //no keywords matches found, invoke hesitation event
        {
           //Invoke Hesitation event
           VideoPlayersHandler.instance.HesitationEvent(temp);                
        }
        
        ResetParametersBase();
        DiactivateListenToUser.Invoke();
    }
    
    public virtual void ResetParametersBase()
    {
        _currentCounter = 0;
        isWaiting = false;
    }
    
    public void PlayWaitClip()
    {
        //Set waiting clips
        _vp.clip = VideoPlayersHandler.instance.WaitingVCs[Random.Range(0, _waitingVCsLength)];
        _currentVideoClip = _vp.clip;
        _vp.Play();
    }
}
