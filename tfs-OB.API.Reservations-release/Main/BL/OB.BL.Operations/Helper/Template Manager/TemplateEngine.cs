using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RazorEngine.Templating;
using RazorEngine.Configuration;
using RazorEngine;

namespace OB.BL.Operations.Helper
{
    public static class TemplateEngine<T>
    {
        static TemplateEngine()
        {
            var config = new TemplateServiceConfiguration();
            config.TemplateManager = new TemplateManager();
            var service = RazorEngineService.Create(config);
            Engine.Razor = service;
        }

        public static string GenerateFile(string templateKey, T model)
        {
            return Engine.Razor.RunCompile(templateKey, model.GetType(), model);
        }

        public static string GenerateFile(string templateKey, List<T> model)
        {
            var result = new StringBuilder();
            foreach (var item in model)
            {
                result.AppendLine(Engine.Razor.RunCompile(templateKey, item.GetType(), item));
            }
            
            return result.ToString();
        }
    }
}
