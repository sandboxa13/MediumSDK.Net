namespace MediumSDK.Domain
{
    public class MediumUser
    { 
        /// <summary>
        /// Medium user ID
        /// </summary>
        public string Id { get; private set; }


        /// <summary>
        /// Medium username 
        /// </summary>
        public string UserName { get; private set; }


        /// <summary>
        /// Medium user real name
        /// </summary>
        public string Name { get; private set; }


        /// <summary>
        /// Medium user profile link 
        /// </summary>
        public string Url { get; private set; }


        /// <summary>
        /// Medium user avatar image link
        /// </summary>
        public string ImageUrl { get; private set; }
    }
}
