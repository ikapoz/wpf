using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Markup.Primitives;

namespace CSS.UI
{
    public static class X
    {
        public static readonly DependencyProperty CSSProperty =
           DependencyProperty.RegisterAttached("CSS", typeof(string), typeof(X),
               new FrameworkPropertyMetadata(string.Empty, OnCSSChanged));


        public static void SetCSS(DependencyObject dp, string value)
        {
            dp.SetValue(CSSProperty, value);
        }

        public static string GetCSS(DependencyObject dp)
            => (string)dp.GetValue(CSSProperty);


        public static Dictionary<string, PropertyDescriptor> GetDependencyProperties(object element)
        {
            Dictionary<string, PropertyDescriptor> properties = new Dictionary<string, PropertyDescriptor>();
            foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(element,
                    new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) }))
                properties.Add(pd.Name, pd);

            return properties;
        }

        public static List<DependencyProperty> GetAttachedProperties(object element)
        {
            List<DependencyProperty> attachedProperties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if (markupObject != null)
                foreach (MarkupProperty mp in markupObject.Properties)
                    if (mp.IsAttached)
                        attachedProperties.Add(mp.DependencyProperty);

            return attachedProperties;
        }

        private static void OnCSSChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender == null)
                return;

            if (e.NewValue == null)
                return;

            var ownerProperties = GetDependencyProperties(sender);
            var cssAsString = (string)e.NewValue;
            var cssAsArray = cssAsString.Trim().Split(' ').Select(p => p.Trim()).ToArray();
            var allResources = GetAllResources(sender);
            var applyStyling = new Dictionary<string, CSSResource>();
            foreach (var css in cssAsArray)
                FindCSSResources(css, ownerProperties, allResources, applyStyling);

            foreach (var cssStyle in applyStyling)
            {
                var style = cssStyle.Value;
                style.PropertyDescriptor.SetValue(sender, style.Resource);
            }
        }

        private static object GetResource(List<ResourceDictionary> resources, string key)
        {
            foreach (var resource in resources)
            {
                if (resource.Contains(key))
                    return resource[key];

                foreach (var mergedResource in resource.MergedDictionaries)
                    if (mergedResource.Contains(key))
                        return mergedResource[key];
            }

            return null;
        }

        private static List<ResourceDictionary> GetAllResources(DependencyObject sender)
        {
            var resourceDictList = new List<ResourceDictionary>();
            DependencyObject parent = sender;
            do
            {
                var properties = GetDependencyProperties(parent);
                if (properties.ContainsKey(resourcesKey))
                {
                    var resourcesPD = properties[resourcesKey];
                    if (resourcesPD.GetValue(parent) is ResourceDictionary resources && (resources.Count > 0 | resources.MergedDictionaries.Count > 0))
                        resourceDictList.Add(resources);
                }

                if (properties.ContainsKey(parentKey))
                {
                    var parentPD = properties[parentKey];
                    parent = parentPD.GetValue(parent) as DependencyObject;
                }
            } while (parent != null);

            // we are going from local to more global defined resources
            resourceDictList.Reverse();

            if (Application.Current != null && Application.Current.Resources != null)
                resourceDictList.Add(Application.Current.Resources);

            return resourceDictList;
        }

        private static void FindCSSResources(string cssKey,
                    Dictionary<string, PropertyDescriptor> ownerProperties,
                    List<ResourceDictionary> allResources,
                    Dictionary<string, CSSResource> applyStyling)
        {
            var value = GetResource(allResources, cssKey);
            if (value == null)
                return;

            if (!(value is ResourceDictionary cssResources))
            {
                cssResources = new ResourceDictionary
                {
                    { cssKey, value }
                };
            }

            // local styling are last applied
            List<string> keys = new List<string>();
            foreach (string key in cssResources.Keys)
            {
                if (ownerProperties.ContainsKey(key))
                    keys.Add(key);
                else
                {
                    var css = cssResources[key];
                    if (css is CSS)
                    {
                        var cssAsArray = key.Trim().Split(' ').Select(p => p.Trim()).ToArray();
                        foreach (var childCSSKey in cssAsArray)
                            FindCSSResources(childCSSKey, ownerProperties, allResources, applyStyling);
                    }
                }
            }

            foreach (var key in keys)
            {
                var newValue = cssResources[key];
                var valueAsString = newValue?.ToString() ?? "";
                // check if static value is 
                var match = staticResourceRegex.Match(valueAsString);
                if (match.Success)
                {
                    var staticResourceKey = match.Groups["key"].Value;
                    var staticResource = GetResource(allResources, staticResourceKey);
                    if (staticResource != null)
                    {
                        if (applyStyling.ContainsKey(key))
                        {
                            var cssResource = applyStyling[key];
                            cssResource.Resource = staticResource;
                        }
                        else
                        {
                            applyStyling[key] = new CSSResource()
                            {
                                PropertyDescriptor = ownerProperties[key],
                                Resource = staticResource
                            };
                        }

                    }
                    else
                    {
                        if (Debugger.IsLogging())
                            Debugger.Log(3, "CSS", $"Static resource not found: {staticResourceKey}");
                    }
                }
                else
                {
                    if (applyStyling.ContainsKey(key))
                    {
                        var cssResource = applyStyling[key];
                        cssResource.Resource = newValue;
                    }
                    else
                    {
                        applyStyling[key] = new CSSResource()
                        {
                            PropertyDescriptor = ownerProperties[key],
                            Resource = newValue
                        };
                    }
                }
            }
        }

        private const string resourcesKey = "Resources";
        private const string parentKey = "Parent";
        private readonly static Regex staticResourceRegex = new Regex(@"\s*(?<key>\$[\w]+)",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

        private class CSSResource
        {
            public object Resource { get; set; }

            public PropertyDescriptor PropertyDescriptor { get; set; }
        }
    }
}
