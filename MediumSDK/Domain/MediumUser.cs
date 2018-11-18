using Newtonsoft.Json;

namespace MediumSDK.Net.Domain
{
    public class MediumUser
    {
        /// <summary>
        /// Medium user ID
        /// </summary>
        [JsonProperty(PropertyName = "id")]
        public string Id { get; private set; }


        /// <summary>
        /// Medium username 
        /// </summary>
        [JsonProperty(PropertyName = "username")]
        public string UserName { get; private set; }


        /// <summary>
        /// Medium user real name
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; private set; }


        /// <summary>
        /// Medium user profile link 
        /// </summary>
        [JsonProperty(PropertyName = "url")]
        public string Url { get; private set; }


        /// <summary>
        /// Medium user avatar image link
        /// </summary>
        [JsonProperty(PropertyName = "imageUrl")]
        public string ImageUrl { get; private set; }
    }
}
