using System;
using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace TaskWatcher.Common
{
    public static class TaskStore
    {
        public static T LoadFromFile<T>(string fileName)
            where T : class
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            try
            {
                string str = File.ReadAllText(fileName);
                return Deserialize<T>(str);
            }
            catch
            {
                return null;
            }
        }

        public static void SaveToFile(string fileName, object obj)
        {
            string rootDirectory = Path.GetDirectoryName(fileName);
            if (!String.IsNullOrEmpty(rootDirectory) && !Directory.Exists(rootDirectory))
            {
                Directory.CreateDirectory(rootDirectory);
            }

            string str = Serialize(obj);
            File.WriteAllText(fileName, str);
        }

        public static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        public static T Deserialize<T>(string str)
        {
            return JsonConvert.DeserializeObject<T>(str);
        }

        public static RepositoryManager LoadRepositoryManager()
        {
            RepositoryContainer settings;
            if (File.Exists(PathHelper.SettingsPath))
            {
                settings = LoadFromFile<RepositoryContainer>(PathHelper.SettingsPath);
            }
            else
            {
                settings = new RepositoryContainer { Repositories = new List<Repository>() };
            }
            return new RepositoryManager(settings);
        }

        public static void SaveRepositoryManager(RepositoryManager repoManager)
        {
            if (repoManager == null)
            {
                throw new ArgumentNullException("repoManager");
            }

            var settings = new RepositoryContainer {
                                                      Repositories = new List<Repository>(repoManager.Repositories),
                                                      CurrentRepository = repoManager.CurrentRepository.Name
                                                  };
            SaveToFile(PathHelper.SettingsPath, settings);
        }

        public static TaskManager LoadTaskManager(Repository repository)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }

            var tasks = LoadFromFile<IEnumerable<TaskItem>>(repository.Path);
            return new TaskManager(tasks, repository.Name);
        }

        public static void SaveTaskManager(Repository repository, TaskManager taskManager)
        {
            if (repository == null)
            {
                throw new ArgumentNullException("repository");
            }
            if (taskManager == null)
            {
                throw new ArgumentNullException("taskManager");
            }

            SaveToFile(repository.Path, taskManager.Tasks);
        }
    }
}
