using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace AssetBundleLoadingTools.Models.Shader
{
    [Serializable]
    public class ShaderProperty
    {
        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string DisplayName { get; set; } // Unsure if this is actually needed for comparisons. It did catch a different shader ONCE. Probably better to have too much data than not enough

        [JsonProperty]
        public ShaderPropertyType PropertyType { get; set; } // might be a good idea to avoid directly referencing Unity assemblies for models, depending on what these end up being used in

        // Default property value might be needed?
        public ShaderProperty(string name, string displayName, ShaderPropertyType propertyType)
        {
            Name = name;
            DisplayName = displayName;
            PropertyType = propertyType;
        }

        [JsonConstructor]
        public ShaderProperty() { }
    }
}
