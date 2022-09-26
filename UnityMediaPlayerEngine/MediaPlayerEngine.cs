using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using RenderHeads.Media.AVProVideo;
using Networking;
public class MediaPlayerEngine : MonoBehaviour
{
    private const float _frameRate = 50f; 
    private const string _saveFileName = "replays.save";
    private string _saveFilePath;
    
    private MediaPlayer _videoPlayer;
    private VideoReplayController _replayController;
    private VideoReplay _replay;
    private VideoReplayTool _replayTool;
    
    public static MediaPlayerEngine Instance;
    
    public PlayerHand.HandAction ActionHold;
    
    
    #region MonoBehaviourCallbacks
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogWarning("More than one MediaPlayerEngine instance has been found!");
        }
        Instance = this;
        
        _saveFilePath = GetSaveFilePath();
        _replayController = GetComponent<VideoReplayController>();
        _videoPlayer = GetComponent<MediaPlayer>();
    }

    protected void Start()
    {
        PreLoad();
    }
    
    private void Update()
    {
        //Rewind
        if (OVRInput.Get(OVRInput.RawButton.RThumbstickUp))
        {
            RewindControl(Player.Instance.RightHand, Direction4.Up);
        }
        if (OVRInput.Get(OVRInput.RawButton.RThumbstickDown))
        {
            RewindControl(Player.Instance.RightHand, Direction4.Down);
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickLeft))
        {
            RewindControl(Player.Instance.RightHand, Direction4.Left);
        }
        if (OVRInput.GetDown(OVRInput.RawButton.RThumbstickRight))
        {
            RewindControl(Player.Instance.RightHand, Direction4.Right);
        }
        
        //Play and Pause a video
        if (OVRInput.GetDown(OVRInput.RawButton.B))
        {
            PlayPauseControl();
        }

        //Custom controller button press event
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            OnTriggerPress();
        }
    }
    #endregion

    #region MediaPlayerControl
    
    private void Play()
    {
        _videoPlayer.Play();
    }

    private void Pause()
    {
        VP.Pause();
    }

    private void RewindControl(PlayerHand hand, Direction4 direction)
    {
        Action handlingMethod = null;

        switch (direction)
        {
            case Direction4.Up:
                handlingMethod = OnJoystickUp;
                break;
            case Direction4.Down:
                handlingMethod = OnJoystickDown;
                break;
            case Direction4.Left:
                handlingMethod = OnJoystickLeft;
                break;
            case Direction4.Right:
                handlingMethod = OnJoystickRight;
                break;
        }

        if (!OVRInput.Get(OVRInput.RawButton.RHandTrigger))
        {
            handlingMethod();
        }
    }
    
    private void OnTriggerPress()
    {
        if (ActionHold != null && ActionHold.GetInvocationList().Length > 0)
        {
            ActionHold(Player.Instance.RightHand);
        }
    }

    private void OnJoystickLeft()
    {
        if (!VP.Control.IsPaused())
        {
            VP.Pause();
        }

        if (VP.Control.GetCurrentTime() != 0)
        {
            VP.Control.Seek(VP.Control.GetCurrentTime() - 1f / _frameRate);
        }
    }

    private void OnJoystickRight()
    {
        if (!VP.Control.IsPaused())
        {
            VP.Pause();
        }

        if (VP.Control.GetCurrentTime() != VP.Info.GetDuration())
        {
            VP.Control.Seek(VP.Control.GetCurrentTime() + 1f / _frameRate);
        }
    }
    
    public void PlayPauseControl()
    {
        if (!_videoPlayer.Control.IsFinished())
        {
            if (_videoPlayer.Control.IsPlaying())
            {
                Pause();
            }
            else
            {
                Play();
            }
        }
        else
        {
            _videoPlayer.Rewind(false);
        }
    }
    #endregion
    
    #region DataProcessing

    public static string GetCurrentReplayFolder()
    {
        string replaysPath = Path.Combine(Application.persistentDataPath, "Replays");
        string replaysIdPath = Path.Combine(replaysPath, ReplayManager.CurrentReplay.id);
        return replaysIdPath;
    }
    
    private string GetSaveFilePath()
    {
        string finalPath = Path.Combine(GetCurrentReplayFolder(), _saveFileName);
        return finalPath;
    }
    
    private void PreLoad()
    {
        FileInfo[] fis = new DirectoryInfo(GetCurrentReplayFolder()).GetFiles("*.mp4", SearchOption.TopDirectoryOnly);
        LoadCurrentReplayObjects(fis);
    }
    
    /// <summary>
    /// Asynchronously loads replay file
    /// </summary>
    private void LoadCurrentReplayObjects(FileInfo[] fis)
    {
        AscManager.Run(() =>
        {
            _replay = new VideoReplay(true);
            try
            {
                _replayTool = JsonLoader.Load<VideoReplayTool>(_saveFilePath)
                foreach (VideoReplay replay in _replayTool.ReplaysToProcess)
                {
                    foreach (VideoReplayObject replayObject in replay.Replays)
                    {
                        string videoName = replayObject.fileName + ".mp4";
                        if (Array.Exists(fis, x => x.Name == videoName))
                        {
                            _replay.AddReplay(replayObject);
                        }
                    }
                }
                _replay.GetCurrentCategoryReplays();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                return false;
            }
            return true;
        }, () =>
        {
            _replayController.InitReplayData();
        });
    }

    /// <summary>
    /// Searches through all available replays and returns a list of the chosen replay objects
    /// </summary>
    /// <param name="replay">Replay you want to get all available replay objects from</param>
    /// <returns>List of all replay objects for a chosen replay</returns>
    public List<VideoReplayObject> GetReplayObjects(string replay)
    {
        List<VideoReplayObject> replayObjects = new List<VideoReplayObject>();
        foreach (VideoReplay replay in _replayTool.ReplaysToProcess)
        {
            foreach (VideoreplayObject replayObject in replay.Replays)
            {
                if (replayObject.fileName.Contains(replay))
                {
                    replayObjects.Add(replayObject);
                }
            }
        }

        return replayObjects;
    }
    #endregion
}
