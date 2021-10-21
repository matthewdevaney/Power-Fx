﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics;
using Microsoft.PowerFx.Core.IR;

namespace Microsoft.PowerFx.Core.Public.Values
{
    [DebuggerDisplay("Blank() ({Type})")]
    public class BlankValue : FormulaValue
    {
        internal BlankValue(IRContext irContext): base(irContext)
        {
        }

        public override object ToObject()
        {
            return null;
        }

        public override string ToString()
        {
            return $"Blank()";
        }

        public override void Visit(IValueVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}