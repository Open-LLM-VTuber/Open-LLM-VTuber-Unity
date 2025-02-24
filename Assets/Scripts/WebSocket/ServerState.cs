
public class ServerState
{
    private readonly object _lock = new object();

    private bool _isInterrupted;
    private bool _isBackendSynced = false;
    private bool _isFrontendSynced = true;

    public bool IsInterrupted
    {
        get { lock (_lock) return _isInterrupted; }
        set { lock (_lock) _isInterrupted = value; }
    }

    public bool IsBackendSynced
    {
        get { return _isBackendSynced; }
        set { _isBackendSynced = value; }         
    }

    public bool IsFrontendSynced
    {
        get { return _isFrontendSynced; }
        set { _isFrontendSynced = value; }
    }
}