using System;

namespace LogExpert
{
    [Serializable]
    public enum SessionSaveLocation
    {
        DocumentsDir,
        SameDir,
        OwnDir,
        LoadedSessionFile
    }
}
