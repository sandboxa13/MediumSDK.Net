using MediumSDK.Net.Domain;

namespace test
{
    class Program
    {
        private static MediumClient _mediumClient;

        static void Main(string[] args)
        {
            _mediumClient = new MediumClient(
                "ce250fa7c114",
                "bb152d21f43b20de5174495f488cd71aede8efaa",
                "text");

            _mediumClient.AuthenticateUser();
        }
    }
}
