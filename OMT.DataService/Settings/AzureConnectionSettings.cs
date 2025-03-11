using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OMT.DataService.Settings
{
    public class AzureConnectionSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Container { get; set; } = string.Empty;
        public string AccountKey { get; set; } = string.Empty;

    }
}