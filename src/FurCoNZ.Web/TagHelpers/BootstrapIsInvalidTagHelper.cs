using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace FurCoNZ.Web.TagHelpers
{
    /// <summary>
    /// If an input has server side validation errors, add the .is-invalid class
    /// </summary>
    [HtmlTargetElement("input", TagStructure = TagStructure.WithoutEndTag)]
    public class BootstrapIsInvalidTagHelper : TagHelper
    {
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        [HtmlAttributeName("asp-for")]
        public ModelExpression For { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);

            if (ViewContext == null || For == null)
                return;

            var id = NameAndIdProvider.GetFullHtmlFieldName(ViewContext, For.Name);
            if (!ViewContext.ModelState.TryGetValue(id, out var modelStateEntry))
                return;

            if (modelStateEntry.ValidationState != Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                return;

            output.MergeAttributes(new TagBuilder(output.TagName) { Attributes = { { "class", "is-invalid" } } });
        }
    }
}
