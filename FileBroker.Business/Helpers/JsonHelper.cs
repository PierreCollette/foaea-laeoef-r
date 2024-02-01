﻿using NJsonSchema;
using NJsonSchema.Validation;

namespace FileBroker.Business.Helpers
{
    public class JsonHelper
    {
        public static List<string> Validate<T>(string source, out List<UnknownTag> unknownTags)
        {
            var result = new List<string>();
            unknownTags = new List<UnknownTag>();

            var schema = JsonSchema.FromType<T>();
            schema.AllowAdditionalProperties = true;
            try
            {
                var errors = schema.Validate(source);

                foreach (ValidationError error in errors)
                {
                    string errorMessage = string.Empty;

                    if (error.ToString().Contains("ArrayExpected"))
                    {
                        errorMessage = string.Empty; // ignore these errors, the json conversion should handle them
                    }
                    else
                    {
                        errorMessage = $"Schema error for {typeof(T).Name} at line {error.LinePosition}: {error.Property} [{error.Kind}] {error.Path}";
                        string errorDescription = error.ToString();
                        int pos = errorDescription.IndexOf("NoAdditionalPropertiesAllowed:");
                        if (pos != -1)
                        {
                            errorDescription = errorDescription[pos..];
                            int lineBreakPos = errorDescription.IndexOf("\n");
                            if (lineBreakPos != -1)
                            {
                                errorDescription = errorDescription[..lineBreakPos];

                                string section = string.Empty;
                                string tag = string.Empty;

                                int firstDot = errorDescription.IndexOf(".");
                                int firstBracket = errorDescription.IndexOf("[");
                                if ((firstDot != -1) && (firstBracket != -1))
                                {
                                    firstDot++;
                                    section = errorDescription[firstDot..firstBracket];
                                }

                                int lastDot = errorDescription.LastIndexOf(".");
                                if (lastDot != -1)
                                {
                                    lastDot++;
                                    tag = errorDescription[lastDot..];
                                }

                                if (!string.IsNullOrEmpty(section) && !string.IsNullOrEmpty(tag))
                                {
                                    var unknownTag = new UnknownTag
                                    {
                                        Section = section,
                                        Tag = tag
                                    };
                                    unknownTags.Add(unknownTag);
                                    errorMessage = "";
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(errorMessage))
                        result.Add(errorMessage);
                }
            }
            catch (Exception e)
            {
                result.Add(e.Message);
            }

            return result;
        }
    }
}
