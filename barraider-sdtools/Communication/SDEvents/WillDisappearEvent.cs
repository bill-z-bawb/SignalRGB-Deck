﻿using BarRaider.SdTools.Payloads;
using Newtonsoft.Json;

namespace BarRaider.SdTools.Communication.SDEvents
{
    /// <summary>
    /// Payload for WillDisappearEvent event
    /// </summary>
    public class WillDisappearEvent : BaseEvent
    {
        /// <summary>
        /// Action Name
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; private set; }

        /// <summary>
        /// Unique Action UUID
        /// </summary>
        [JsonProperty("context")]
        public string Context { get; private set; }

        /// <summary>
        /// Stream Deck device UUID
        /// </summary>
        [JsonProperty("device")]
        public string Device { get; private set; }

        /// <summary>
        /// settings
        /// </summary>
        [JsonProperty("payload")]
        public AppearancePayload Payload { get; private set; }
    }
}
