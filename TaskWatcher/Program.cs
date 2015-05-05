using TaskWatcher.Common;

namespace TaskWatcher.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            PathHelper.InitializeAppFolder();

            RepositoryManager rm = TaskStore.LoadRepositoryManager();
            TaskManager tm = TaskStore.LoadTaskManager(rm.CurrentRepository);

            var processor = new CommandProcessor(tm, rm);
            processor.AddCommandsObject(new TaskCommands(tm));
            processor.AddCommandsObject(new RepositoryCommands(rm));
            processor.AddCommandsObject(processor);

            processor.Run(args);
        }
    }
}
