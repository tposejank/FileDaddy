﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FNF_Mod_Manager
{
    public class GameBananaItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("Preview().sSubFeedImageUrl()")]
        public Uri SubFeedImage { get; set; }
        [JsonPropertyName("Preview().sStructuredDataFullsizeUrl()")]
        public Uri EmbedImage { get; set; }
        [JsonPropertyName("Updates().bSubmissionHasUpdates()")]
        public bool HasUpdates { get; set; }

        [JsonPropertyName("Updates().aGetLatestUpdates()")]
        public GameBananaItemUpdate[] Updates { get; set; }
        [JsonPropertyName("Files().aFiles()")]
        public Dictionary<string, GameBananaItemFile> Files { get; set; }

    }
    public class GameBananaItemFile
    {
        [JsonPropertyName("_sFile")]
        public string FileName { get; set; }

        [JsonPropertyName("_nFilesize")]
        public long Filesize { get; set; }

        [JsonPropertyName("_sDownloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("_sDescription")]
        public string Description { get; set; }

        [JsonPropertyName("_tsDateAdded")]
        public long DateAddedLong { get; set; }
    }
    public class GameBananaItemUpdate
    {
        [JsonPropertyName("_sTitle")]
        public string Title { get; set; }

        [JsonPropertyName("_aChangeLog")]
        public GameBananaItemUpdateChange[] Changes { get; set; }

        [JsonPropertyName("_sText")]
        public string Text { get; set; }

        [JsonPropertyName("_tsDateAdded")]
        public long DateAddedLong { get; set; }
    }
    public class GameBananaItemUpdateChange
    {
        [JsonPropertyName("cat")]
        public string Category { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
