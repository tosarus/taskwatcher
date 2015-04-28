using System.Collections.Generic;

namespace TaskWatcher.Import
{
    public interface IImporter
    {
        ICollection<Common.TaskItem> ImportFromFile(string fileName);
        void ImportFromFileToTaskManager(string fileName, Common.TaskManager taskManager);
    }

    public class ImporterFactory
    {
        public IImporter CreateOldInfraImporter()
        {
            return new OldInfra.OldInfraImporter();
        }
    }
}
