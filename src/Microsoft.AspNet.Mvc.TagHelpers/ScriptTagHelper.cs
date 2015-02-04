﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Mvc.TagHelpers
{
	/// <summary>
	/// <see cref="ITagHelper"/> implementation targeting &lt;scipt&gt; elements that supports fallback src paths.
	/// </summary>
	public class ScriptTagHelper : TagHelper
    {
		private const string FallbackSrcAttributeName = "asp-fallback-src";
		private const string FallbackTestMethodAttributeName = "asp-fallback-test";
		private const string SrcAttributeName = "src";

		// NOTE: All attributes are required for the LinkTagHelper to process.
		private static readonly string[] RequiredAttributes = new[]
		{
			FallbackSrcAttributeName,
			FallbackTestMethodAttributeName,
			SrcAttributeName,
		};

		/// <summary>
		/// The URL of a Script tag to fallback to in the case the primary one fails (as specified in the src
		/// attribute).
		/// </summary>
		[HtmlAttributeName(FallbackSrcAttributeName)]
		public string FallbackSrc { get; set; }

		/// <summary>
		/// The script method defined in the primary script to use for the fallback test.
		/// </summary>
		[HtmlAttributeName(FallbackTestMethodAttributeName)]
		public string FallbackTestMethod { get; set; }

		// Protected to ensure subclasses are correctly activated. Internal for ease of use when testing.
		[Activate]
		protected internal ILogger<ScriptTagHelper> Logger { get; set; }

		/// <inheritdoc />
		public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
		{
            // TODO: Consider using the RazorStringWriter instead of a StringBuilder, because Script tags may contain large bodies.

            // TODO: This check doesn't handle the fact that SRC is required for the tag helper alone, but src without other properties is fine.
            // So scenarios that shouldn't produce an error are:
            // SRC is declared, but there are no asp-* attributes
            // SRC is not declared, and there are no asp-* attributes
            if (!context.AllRequiredAttributesArePresent(RequiredAttributes, Logger))
            {
                if (Logger.IsEnabled(LogLevel.Verbose))
                {
                    Logger.WriteVerbose("Skipping processing for {0} {1}", nameof(ScriptTagHelper), context.UniqueId);
                }

                return;
            }

			var content = new StringBuilder();

			// NOTE: Values in TagHelperOutput.Attributes are already HtmlEncoded

			// We've taken over rendering here so prevent the element rendering the outer tag
			output.TagName = null;

			// Rebuild the <script /> tag.
			content.Append("<script ");
			foreach (var attribute in output.Attributes)
			{
				content.AppendFormat(CultureInfo.InvariantCulture, "{0}=\"{1}\" ", attribute.Key, attribute.Value);
			}

			content.Append(">");

			var originalContent = await context.GetChildContentAsync();
			content.Append(originalContent);
			content.AppendLine("</script>");

			// Build the <script /> tag that checks the test method and if it fails, renders the extra script.
			content.Append("<script>");
			content.Append(this.FallbackTestMethod);
			content.Append(" || document.Write(\"<script> src=\\\"");
			content.Append(WebUtility.HtmlEncode(FallbackSrc));
			content.Append("\\\" ");

			foreach (var attribute in output.Attributes)
			{
				if (!attribute.Key.Equals(SrcAttributeName, System.StringComparison.OrdinalIgnoreCase))
				{
                    var encodedKey = JavaScriptUtility.JavaScriptStringEncode(attribute.Key);
                    var encodedValue = JavaScriptUtility.JavaScriptStringEncode(attribute.Value);

                    content.AppendFormat(CultureInfo.InvariantCulture, "{0}=\\\"{1}\\\" ", encodedKey, encodedValue);
				}
			}

            content.Append("><\\/script>\");</script>");

			output.Content = content.ToString();
		}
	}
}
