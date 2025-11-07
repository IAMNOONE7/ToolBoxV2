using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolBoxV2.Application.LocalMessages
{
    /// <summary>
    /// Factory interface responsible for creating new instances of 
    /// <see cref="ILocalMessageIncrementalBuilder"/>.
    /// 
    /// Why we need this:
    /// - The builder keeps an internal dictionary of messages that grows as we read rows.
    /// - When we start a new import, we want a fresh, empty builder — not one
    ///   that remembers old messages.
    /// - Using a factory allows us to create a new builder each time,
    ///   while keeping everything properly registered in Dependency Injection.
    /// </summary>
    public interface ILocalMessageIncrementalBuilderFactory
    {
        /// <summary>
        /// Creates a new instance of a builder that can incrementally build LocalMessages.
        /// </summary>
        ILocalMessageIncrementalBuilder Create();
    }
}
