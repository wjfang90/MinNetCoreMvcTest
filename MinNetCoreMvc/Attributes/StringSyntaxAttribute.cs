using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace MinNetCoreMvc.Attributes {
    [AttributeUsage(AttributeTargets.Parameter)]
    public class StringSyntaxAttribute : Attribute {
        /// <summary>
        /// 
        /// </summary>
        public string Pattern { get; set; }
        public StringSyntaxAttribute(string pattern) {
            this.Pattern = pattern;

            if (!pattern.StartsWith("Route")) {
                throw new ArgumentException("参数不正确");
            }
            
        }

    }
}
