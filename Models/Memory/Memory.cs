using System.Numerics;
using System.Collections.Generic;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TwitchBot.Models.Memory
{
    public class Memory
    {
        /// <summary>
        /// The id of the host
        [JsonPropertyName("hostId")]
        public string HostId { get; set; }

        /// <summary>
        /// The current trial number
        [JsonPropertyName("trialNumber")]
        public Int64 TrialNumber { get; set; }

        /// <summary>
        /// Last date and time the host was online
        [JsonPropertyName("lastOnline")]
        public DateTime LastOnline { get; set; }
    }
}