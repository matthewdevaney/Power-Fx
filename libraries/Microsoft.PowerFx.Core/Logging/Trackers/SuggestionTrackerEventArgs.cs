﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.PowerFx.Core.Binding;
using Microsoft.PowerFx.Core.Syntax.Nodes;
using Microsoft.PowerFx.Core.Utils;

namespace Microsoft.PowerFx.Core.Logging.Trackers
{
    internal interface IAddSuggestionMessageEventArgs
    {
        string Message { get; }
        TexlNode Node { get; }
        TexlBinding Binding { get; }
    }

    internal class AddSuggestionMessageEventArgs : IAddSuggestionMessageEventArgs
    {
        public string Message { get; }
        public TexlNode Node { get; }
        public TexlBinding Binding { get; }

        public AddSuggestionMessageEventArgs(string message, TexlNode node, TexlBinding binding)
        {
            Contracts.AssertValue(message);
            Contracts.AssertValue(node);
            Contracts.AssertValue(binding);

            Message = message;
            Node = node;
            Binding = binding;
        }
    }
}
