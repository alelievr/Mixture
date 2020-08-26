using System;

namespace Mixture
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DocumentationAttribute : Attribute
    {
        public string markdown;

        public DocumentationAttribute(string markdown)
        {
            this.markdown = markdown;
        }
    }
}