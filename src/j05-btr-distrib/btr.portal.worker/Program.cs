namespace btr.portal.worker
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            return new WorkerRunCoordinator().Run(args);
        }
    }
}
