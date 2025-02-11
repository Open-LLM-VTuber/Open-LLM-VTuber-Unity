
public class ServerState
{
    private readonly object _lock = new object();

    private bool _allowUnityAudio;
    private bool _isInterrupted;

    public bool AllowUnityAudio
    {
        get { lock (_lock) return _allowUnityAudio; }
        private set { lock (_lock) _allowUnityAudio = value; }
    }

    public bool IsInterrupted
    {
        get { lock (_lock) return _isInterrupted; }
        private set { lock (_lock) _isInterrupted = value; }
    }

    public void SetAllowUnityAudio(bool allow)
    {
        AllowUnityAudio = allow;
    }

    public void SetInterrupted(bool interrupted)
    {
        IsInterrupted = interrupted;
    }
}