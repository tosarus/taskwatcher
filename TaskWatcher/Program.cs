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
            StateManager sm = TaskStore.LoadStateManager();

            var processor = new CommandProcessor(tm, rm, sm);
            processor.AddCommandsObject(new TaskCommands(tm, sm));
            processor.AddCommandsObject(new RepositoryCommands(rm));
            processor.AddCommandsObject(new StateCommands(sm));
            processor.AddCommandsObject(processor);

            processor.Run(args);
        }
    }
}
