﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.PowerFx.Core.Types;

namespace Microsoft.PowerFx.Core.Entities
{
    /// <summary>
    /// Information about column metadata used to define type for multi-choice columns.
    /// </summary>
    internal interface IDataColumnMetadata
    {
        string Name { get; }
        DType Type { get; }
        bool IsSearchable { get; }
        bool IsSearchRequired { get; }
        bool IsExpandEntity { get; }
        DataTableMetadata ParentTableMetadata { get; }
    }
}