﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.PowerFx.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.AppMagic.Authoring.Texl
{
    internal abstract class CalendarFunction : BuiltinFunction
    {
        public override bool IsSelfContained => true;
        public override bool SupportsParamCoercion => true;

        public CalendarFunction(string functionInvariantName, TexlStrings.StringGetter functionDescription)
            : base(new DPath().Append(new DName(LanguageConstants.InvariantCalendarNamespace)), functionInvariantName, functionDescription, FunctionCategories.DateTime, DType.CreateTable(new TypedName(DType.String, new DName("Value"))), 0, 0, 0)
        { }

        public override IEnumerable<TexlStrings.StringGetter[]> GetSignatures()
        {
            yield return new TexlStrings.StringGetter[] { };
        }
    }

    // Calendar.MonthsLong()
    internal sealed class MonthsLongFunction : CalendarFunction
    {
        public MonthsLongFunction()
            : base("MonthsLong", TexlStrings.AboutCalendar__MonthsLong)
        { }
    }

    // Calendar.MonthsShort()
    internal sealed class MonthsShortFunction : CalendarFunction
    {
        public MonthsShortFunction()
            : base("MonthsShort", TexlStrings.AboutCalendar__MonthsShort)
        { }
    }

    // Calendar.WeekdaysLong()
    internal sealed class WeekdaysLongFunction : CalendarFunction
    {
        public WeekdaysLongFunction()
            : base("WeekdaysLong", TexlStrings.AboutCalendar__WeekdaysLong)
        { }
    }

    // Calendar.WeekdaysShort()
    internal sealed class WeekdaysShortFunction : CalendarFunction
    {
        public WeekdaysShortFunction()
            : base("WeekdaysShort", TexlStrings.AboutCalendar__WeekdaysShort)
        { }
    }
}