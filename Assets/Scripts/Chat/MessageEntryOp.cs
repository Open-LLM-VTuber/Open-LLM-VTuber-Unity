
using UnityEngine;

public class MessageEntryOp : MonoBehaviour
{
    public GameObject messageEntry;
    public GameObject msgSplitLine;

    private MessageEntryContent entryContent;

    void Start()
    {
        entryContent = messageEntry.GetComponent<MessageEntryContent>();      
    }

    public void Delete()
    {
        HistoryUIManager.DeleteHistory(entryContent.HistoryUid);
        Destroy(messageEntry);
        Destroy(msgSplitLine);
    }
}
