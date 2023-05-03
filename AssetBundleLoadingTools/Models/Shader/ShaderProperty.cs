using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace AssetBundleLoadingTools.Models.Shader
{
    [System.Serializable]
    public class ShaderProperty
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public ShaderPropertyType PropertyType { get; set; } // might be a good idea to avoid directly referencing Unity assemblies for models, depending on what these end up being used in

        // More data **might** be needed, but for comparison purposes this *should* be enough

        [JsonConstructor]
        public ShaderProperty(string name, string displayName, ShaderPropertyType propertyType)
        {
            Name = name;
            DisplayName = displayName;
            PropertyType = propertyType;
        }
    }
}
