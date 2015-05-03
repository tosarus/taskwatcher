namespace TaskWatcher.Import
{
    public interface IImporter
    {
        void ImportFromFileToTaskManager(string fileName, Common.TaskManager taskManager);
    }

    public class ImporterFactory
    {
        public IImporter CreateOldInfraImporter()
        {
            return new OldInfra.OldInfraImporter();
        }

        public IImporter CreateVer1Importer()
        {
            return new Ver1.Ver1Importer();
        }
    }
}
