﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.PowerFx.Core.Entities;

namespace Microsoft.PowerFx.Core.Types
{
    /// <summary>
    /// Information about expand entity type.
    /// </summary>
    internal interface IExpandInfo
    {
        string Identity { get; }
        bool IsTable { get; }
        string Name { get; }
        string PolymorphicParent { get; }
        IExternalDataSource ParentDataSource { get; }
        ExpandPath ExpandPath { get; }
        void UpdateEntityInfo(IExternalDataSource dataSource, string relatedEntityPath);
        IExpandInfo Clone();
        string ToDebugString();
    }
}