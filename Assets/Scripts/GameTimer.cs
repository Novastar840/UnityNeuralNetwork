using UnityEngine;
using System;
using System.Collections.Generic;

public class GameTimer
{
    public float Duration { get; }
    public float RemainingTime { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }
    
    public bool Looping { get; private set; }
    
    public bool AutoCleanup = true;

    public event Action OnComplete;
    
    private readonly bool UseRealTime;

    public GameTimer(float duration, bool loop = false, bool startTimer = true, bool useRealTime = false, bool autoCleanup = true)
    {
        Duration = duration;
        RemainingTime = duration;
        UseRealTime = useRealTime;
        AutoCleanup = autoCleanup;
        Looping = loop;
        ValidateTimerSystem();
        TimerSystem.Register(this);
        if (startTimer) Start();
    }

    public void Reset(bool startTimer = true)
    {
        IsRunning = startTimer;
        IsPaused = false;
        RemainingTime += Duration;
    }

    private void ValidateTimerSystem()
    {
        if (TimerSystem.Instance == null)
        {
            GameObject gameObject = new GameObject("TimerSystem");
            gameObject.AddComponent<TimerSystem>();
        }
    }
    
    public void Pause() => IsPaused = true;
    public void Resume() => IsPaused = false;
    public void Stop()
    {
        IsRunning = false;
        IsPaused = false;
        OnComplete =  null;
        if (AutoCleanup) TimerSystem.Unregister(this);
    }

    public void Start()
    {
        IsRunning = true;
        IsPaused = false;
    }
    
    internal void Tick()
    {
        if (!IsRunning || IsPaused) return;
        
        float delta = UseRealTime ? Time.unscaledDeltaTime : Time.deltaTime;
        RemainingTime -= delta;

        if (RemainingTime <= 0)
        {
            RemainingTime = 0f;
            IsRunning = false;
            OnComplete?.Invoke();
            if (Looping)
                Reset();
            else
                Stop();
        }
    }
}

public class TimerSystem : MonoBehaviour
{
    public static TimerSystem Instance { get;  private set; }
    private static readonly List<GameTimer> Timers = new List<GameTimer>();

    private void Awake()
    {
        ValidateInstance();
    }

    void Update()
    {
        for (int i = Timers.Count - 1; i >= 0; i--)
        {
            Timers[i].Tick();
        }
    }

    private void ValidateInstance()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static void Register(GameTimer timer)
    {
        if (Instance == null) return;
        
        Timers.Add(timer);
    }
    
    public static void Unregister(GameTimer timer) => Timers.Remove(timer);
}