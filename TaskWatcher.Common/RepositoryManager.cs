using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TaskWatcher.Common
{
    public class RepositoryManager
    {
        private readonly Dictionary<string, Repository> _repositories;

        internal RepositoryManager(RepositoryContainer settings)
            : this(settings.Repositories)
        {
            if (_repositories.ContainsKey(settings.CurrentRepository))
            {
                CurrentRepository = _repositories[settings.CurrentRepository];
            }
        }

        public RepositoryManager(IEnumerable<Repository> repositories)
        {
            _repositories = (repositories ?? Enumerable.Empty<Repository>()).ToDictionary(r => r.Name);
            CurrentRepository = GetOrCreateDefaultRepository();
        }

        public IEnumerable<Repository> Repositories
        {
            get
            {
                return _repositories.Values;
            }
        }

        public Repository CurrentRepository { get; private set; }

        public Repository GetOrCreateDefaultRepository()
        {
            return GetOrCreateRepository(PathHelper.DefaultRepositoryName);
        }

        public Repository CreateRepository(string repoName, string repoPath = null)
        {
            if (String.IsNullOrEmpty(repoName))
            {
                throw new ArgumentException("repoName can't be null or empty", "repoName");
            }

            if (_repositories.ContainsKey(repoName))
            {
                string message = String.Format("Repository '{0}' already exists", repoName);
                throw new InvalidOperationException(message);
            }

            repoPath = PreparePath(repoName, repoPath);
            var repository = new Repository {
                                                Name = repoName,
                                                Path = repoPath,
                                            };
            _repositories.Add(repository.Name, repository);
            return repository;
        }

        public Repository GetOrCreateRepository(string repoName, string repoPath = null)
        {
            if (String.IsNullOrEmpty(repoName))
            {
                throw new ArgumentException("repoName can't be null or empty", "repoName");
            }

            if (!_repositories.ContainsKey(repoName))
            {
                repoPath = PreparePath(repoName, repoPath);
                var repository = new Repository {
                                                    Name = repoName,
                                                    Path = repoPath,
                                                };
                _repositories.Add(repoName, repository);
            }

            return _repositories[repoName];
        }

        private static string PreparePath(string name, string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                path = String.Format("{0}.tasks", name);
            }

            if (!Path.IsPathRooted(path))
            {
                string rootDirectory = !String.IsNullOrEmpty(PathHelper.ApplicationFolder)
                                       ? PathHelper.ApplicationFolder
                                       : Directory.GetCurrentDirectory();
                path = Path.Combine(rootDirectory, path);
            }
            return path;
        }

        public bool HasRepository(string repoName)
        {
            if (String.IsNullOrEmpty(repoName))
            {
                throw new ArgumentException("Argument can't be null or empty", "repoName");
            }

            return _repositories.ContainsKey(repoName);
        }

        public Repository GetRepository(string repoName)
        {
            if (String.IsNullOrEmpty(repoName))
            {
                throw new ArgumentException("Argument can't be null or empty", "repoName");
            }

            Repository repository;
            if (!_repositories.TryGetValue(repoName, out repository))
            {
                string message = String.Format("Repository '{0}' not found", repoName);
                throw new InvalidOperationException(message);
            }
            return repository;
        }

        public Repository SetCurrentRepository(string repoName)
        {
            Repository repository = GetRepository(repoName);
            CurrentRepository = repository;
            return repository;
        }

        public Repository DeleteRepository(string repoName)
        {
            Repository repository = GetRepository(repoName);
            _repositories.Remove(repository.Name);
            if (CurrentRepository == repository)
            {
                CurrentRepository = GetOrCreateDefaultRepository();
            }
            return repository;
        }

        public Repository SetRepositoryPath(string repoName, string repoPath = null)
        {
            Repository repository = GetRepository(repoName);
            repository.Path = PreparePath(repository.Name, repoPath);
            return repository;
        }
    }
}
