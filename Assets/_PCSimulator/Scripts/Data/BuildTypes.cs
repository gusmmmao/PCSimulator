using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PCSimulator.Data
{
    [Serializable]
    public class BuildConfiguration
    {
        [JsonProperty("build_id")]
        public string BuildId { get; set; }

        [JsonProperty("cpu")]
        public string CpuId { get; set; }

        [JsonProperty("motherboard")]
        public string MotherboardId { get; set; }

        [JsonProperty("ram")]
        public List<string> RamIds { get; set; } = new List<string>();

        [JsonProperty("gpu")]
        public string GpuId { get; set; }

        [JsonProperty("storage")]
        public string StorageId { get; set; }

        [JsonProperty("psu")]
        public string PsuId { get; set; }

        [JsonProperty("case")]
        public string CaseId { get; set; }

        public List<string> GetAllComponentIds()
        {
            var list = new List<string>();
            if (!string.IsNullOrEmpty(CpuId)) list.Add(CpuId);
            // if (!string.IsNullOrEmpty(MotherboardId)) list.Add(MotherboardId); // TODO: Implement Motherboard slot
            if (RamIds != null) list.AddRange(RamIds);
            if (!string.IsNullOrEmpty(GpuId)) list.Add(GpuId);
            if (!string.IsNullOrEmpty(StorageId)) list.Add(StorageId);
            if (!string.IsNullOrEmpty(PsuId)) list.Add(PsuId);
            // if (!string.IsNullOrEmpty(CaseId)) list.Add(CaseId); // TODO: Implement Case slot
            return list;
        }
    }
}
