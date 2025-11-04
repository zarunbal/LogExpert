using LogExpert.Core.Config;

namespace LogExpert.Core.Classes.Persister;

internal interface IPersister
{

    string SavePersistenceData (string logFileName, PersistenceData persistenceData, Preferences preferences);

    string SavePersistenceDataWithFixedName (string persistenceFileName, PersistenceData persistenceData);

    PersistenceData LoadPersistenceData (string logFileName, Preferences preferences);

    PersistenceData LoadPersistenceDataOptionsOnly (string logFileName, Preferences preferences);

    PersistenceData LoadPersistenceDataOptionsOnlyFromFixedFile (string persistenceFile);

    PersistenceData LoadPersistenceDataFromFixedFile (string persistenceFile);

    PersistenceData Load (string fileName);

    //string BuildPersisterFileName (string logFileName, Preferences preferences);

    //string BuildSessionFileNameFromPath (string logFileName);

    //void Save (string fileName, PersistenceData persistenceData);

    //PersistenceData LoadInternal (string fileName);

}
