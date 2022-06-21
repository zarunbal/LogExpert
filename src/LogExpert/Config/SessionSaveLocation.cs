using System;

namespace LogExpert.Config
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