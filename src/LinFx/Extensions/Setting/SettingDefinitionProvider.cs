﻿using System.Collections.Generic;
using System.Collections.Immutable;

namespace LinFx.Extensions.Setting
{
    public class SettingDefinitionProvider
    {
        public Dictionary<string, SettingDefinition> SettingDefinitions { get; }

        public SettingDefinitionProvider()
        {
            SettingDefinitions = new Dictionary<string, SettingDefinition>();
        }

        public virtual SettingDefinition GetOrNull(string name)
        {
            return SettingDefinitions.GetOrDefault(name);
        }

        public virtual IReadOnlyList<SettingDefinition> GetAll()
        {
            return SettingDefinitions.Values.ToImmutableList();
        }

        public virtual SettingDefinitionProvider AddOrUpdate(params SettingDefinition[] definitions)
        {
            if (definitions != null && definitions.Length > 0)
            {
                foreach (var definition in definitions)
                {
                    SettingDefinitions[definition.Name] = definition;
                }
            }
            return this;
        }
    }
}
