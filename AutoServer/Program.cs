namespace AutoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new AutoService();
            service.Start(args);
        }
    }
}