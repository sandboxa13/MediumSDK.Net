using System.Threading.Tasks;
using MediumSDK.Net.Domain;

namespace MediumSDK.Net
{
    public interface IMediumClient
    {
        Token Token { get; set; }

        MediumUser User { get; set; }

        string ClientSecret { get; set; }

        string ClientId { get; set; }

        string State { get; set; }

        Task<Token> AuthenticateUser();

        Task<MediumUser> GetUser();
    }
}
