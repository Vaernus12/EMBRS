using Newtonsoft.Json;
using System;

namespace EMBRS
{
    [Serializable]
    public class TournamentSponsor
    {
        [JsonProperty] private string _sponsorName;
        [JsonProperty] private string _sponsorUrl;
        [JsonProperty] private string _imageUrl;

        public TournamentSponsor(string sponsor, string url, string image)
        {
            _sponsorName = sponsor;
            _sponsorUrl = url;
            _imageUrl = image;
        }

        public string GetSponsorName()
        {
            return _sponsorName;
        }

        public string GetSponsorUrl()
        {
            return _sponsorUrl;
        }

        public string GetSponsorImageUrl()
        {
            return _imageUrl;
        }
    }
}
