using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolBoxV2.Application.Common;
using ToolBoxV2.Application.XMLEditor;
using ToolBoxV2.Infrastracture.Diagnostics;
using ToolBoxV2.Infrastracture.Excel;

namespace ToolBoxV2.Infrastracture
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IDiagnosticLogger, SimpleDiagnosticLogger>();
            services.AddSingleton<IExcelReader, ClosedXMLExcelReader>();
            services.AddSingleton<IXMLReaderService, XMLEditor.XMLReaderService>();
            services.AddSingleton<IXMLExportService, XMLExportService>();
            services.AddSingleton<IXMLNodeEditService, XMLNodeEditService>();
            services.AddSingleton<IXMLTemplateService, XMLTemplateService>();
            // later: services.AddSingleton<ILocExporter, LocExporter>();

            return services;
        }
    }
}
