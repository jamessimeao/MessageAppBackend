namespace ConsoleClient
{
    internal class Program
    {
        public static async Task Main()
        {
            bool productionUrls = false;
            const int usersQuantity = 3;
            Test test = new(productionUrls, usersQuantity);
            await test.ExecuteAsync();
        }
    }
}