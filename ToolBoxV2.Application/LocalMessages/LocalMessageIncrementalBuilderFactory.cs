using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Application.LocalMessages
{
    /// <summary>
    /// Default implementation of the <see cref="ILocalMessageIncrementalBuilderFactory"/>.
    /// 
    /// It simply creates a new <see cref="LocalMessageIncrementalBuilder"/> each time.
    /// 
    /// By registering this factory in DI as a singleton, we can still resolve it from
    /// anywhere, but each call to Create() gives a new, clean builder instance.
    /// </summary>
    public class LocalMessageIncrementalBuilderFactory : ILocalMessageIncrementalBuilderFactory
    {
        // Creates a new, empty incremental builder ready to process rows.
        public ILocalMessageIncrementalBuilder Create()
        {
            // Always return a new builder — ensures no shared state between imports
            return new LocalMessageIncrementalBuilder();
        }
    }
}
