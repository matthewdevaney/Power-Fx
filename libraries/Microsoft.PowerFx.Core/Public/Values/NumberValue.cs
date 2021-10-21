﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.Contracts;
using Microsoft.PowerFx.Core.IR;
using Microsoft.PowerFx.Core.Public.Types;

namespace Microsoft.PowerFx.Core.Public.Values
{
    public class NumberValue : PrimitiveValue<double>
    {
        internal NumberValue(IRContext irContext, double value) : base(irContext, value)
        {
            Contract.Assert(IRContext.ResultType == FormulaType.Number);
        }

        public override void Visit(IValueVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
