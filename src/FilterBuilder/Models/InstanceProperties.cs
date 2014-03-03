﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InstanceProperties.cs" company="Orcomp development team">
//   Copyright (c) 2008 - 2014 Orcomp development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.FilterBuilder.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Catel;
    using Catel.Data;
    using Catel.Reflection;

    public class InstanceProperties
    {
        public InstanceProperties(Type type)
        {
            Argument.IsNotNull("type", type);

            var finalProperties = new Dictionary<string, IPropertyMetadata>();

            var regularProperties = new List<PropertyInfo>();
            regularProperties.AddRange(type.GetProperties().Where(m => m.CanRead && m.PropertyType == typeof(string)));
            regularProperties.AddRange(type.GetProperties().Where(m => m.CanRead && (m.PropertyType == typeof(int) || m.PropertyType == typeof(int?))));
            regularProperties.AddRange(type.GetProperties().Where(m => m.CanRead && (m.PropertyType == typeof(DateTime) || m.PropertyType == typeof(DateTime?))));
            regularProperties.AddRange(type.GetProperties().Where(m => m.CanRead && m.PropertyType == typeof(bool)));
            regularProperties.AddRange(type.GetProperties().Where(m => m.CanRead && m.PropertyType == typeof(TimeSpan)));
            regularProperties.AddRange(type.GetProperties().Where(m => m.CanRead && (m.PropertyType == typeof(decimal) || m.PropertyType == typeof(decimal?))));

            foreach (var property in regularProperties.Distinct())
            {
                finalProperties[property.Name] = new PropertyMetadata(type, property);
            }

            var catelProperties = new List<PropertyData>();
            if (typeof(ModelBase).IsAssignableFromEx(type))
            {
                var propertyDataManager = PropertyDataManager.Default;
                var catelTypeInfo = propertyDataManager.GetCatelTypeInfo(type);
                catelProperties.AddRange(catelTypeInfo.GetCatelProperties().Values.Where(x => !x.IsModelBaseProperty));

                // Remove Catel properties
                RemoveCatelProperty(finalProperties, "BusinessRuleErrorCount");
                RemoveCatelProperty(finalProperties, "BusinessRuleWarningCount");
                RemoveCatelProperty(finalProperties, "FieldErrorCount");
                RemoveCatelProperty(finalProperties, "FieldWarningCount");
                RemoveCatelProperty(finalProperties, "HasErrors");
                RemoveCatelProperty(finalProperties, "HasWarnings");
                RemoveCatelProperty(finalProperties, "IsDirty");
                RemoveCatelProperty(finalProperties, "IsEditable");
                RemoveCatelProperty(finalProperties, "IsInEditSession");
                RemoveCatelProperty(finalProperties, "IsReadOnly");
            }

            foreach (var property in catelProperties.Distinct())
            {
                finalProperties[property.Name] = new PropertyMetadata(type, property);
            }

            Properties = new List<IPropertyMetadata>(finalProperties.Values.OrderBy(m => m.Name));
        }

        #region Properties
        public List<IPropertyMetadata> Properties { get; private set; }
        #endregion

        #region Methods
        public IPropertyMetadata GetProperty(string propertyName)
        {
            return (from property in Properties
                    where string.Equals(property.Name, propertyName)
                    select property).FirstOrDefault();
        }

        private void RemoveCatelProperty(Dictionary<string, IPropertyMetadata> properties, string propertyName)
        {
            if (properties.ContainsKey(propertyName))
            {
                properties.Remove(propertyName);
            }
        }
        #endregion
    }
}