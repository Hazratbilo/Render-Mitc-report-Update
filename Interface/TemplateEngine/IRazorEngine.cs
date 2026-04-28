using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mitc_report_Update.Interface.TemplateEngine
{
    public interface IRazorEngine
    {
        Task<string> ParseAsync<TModel>(string viewName, TModel model);
    }
}