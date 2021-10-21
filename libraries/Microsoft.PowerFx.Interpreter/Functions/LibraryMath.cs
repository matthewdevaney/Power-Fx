﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.PowerFx.Core.IR;
using System.Collections.Generic;
using System.Linq;
using Microsoft.PowerFx.Core.Public.Types;
using Microsoft.PowerFx.Core.Public.Values;

namespace Microsoft.PowerFx.Functions
{
    // Direct ports from JScript. 
    internal static partial class Library
    {
        // Support for aggregators. Helpers to ensure that Scalar and Tabular behave the same.
        private interface IAggregator
        {
            void Apply(FormulaValue value);
            FormulaValue GetResult(IRContext irContext);
        }

        private class SumAgg : IAggregator
        {
            protected int _count;
            protected double _accumulator;

            public void Apply(FormulaValue value)
            {
                if (value is BlankValue) { return; }
                var n1 = (NumberValue)value;

                _accumulator += n1.Value;
                _count++;
            }
            public virtual FormulaValue GetResult(IRContext irContext)
            {
                if (_count == 0) { return new BlankValue(irContext); }
                return new NumberValue(irContext, _accumulator);
            }
        }

        private class AverageAgg : SumAgg
        {
            public override FormulaValue GetResult(IRContext irContext)
            {
                if (_count == 0) { return new BlankValue(irContext); }
                return new NumberValue(irContext, _accumulator / _count);
            }
        }

        private static FormulaValue RunAggregator(IAggregator agg, IRContext irContext, FormulaValue[] values)
        {
            foreach (var value in values)
            {
                agg.Apply(value);
            }
            return agg.GetResult(irContext);
        }

        private static FormulaValue RunAggregator(IAggregator agg, EvalVisitor runner, SymbolContext context, IRContext irContext, FormulaValue[] args)
        {
            var arg0 = (TableValue)args[0];
            var arg1 = (LambdaFormulaValue)args[1];

            foreach (DValue<RecordValue> row in arg0.Rows)
            {
                if (row.IsValue)
                {
                    var childContext = context.WithScopeValues(row.Value);
                    var value = arg1.Eval(runner, childContext);

                    if (value is NumberValue number)
                    {
                        value = FiniteChecker(irContext, 0, number);
                    }

                    if (value is ErrorValue error)
                    {
                        return error;
                    }
                    agg.Apply(value);
                }
            }

            return agg.GetResult(irContext);
        }

        private static FormulaValue Sqrt(IRContext irContext, NumberValue[] args)
        {
            var n1 = args[0];

            var result = System.Math.Sqrt(n1.Value);

            return new NumberValue(irContext, result);
        }

        //  Sum(1,2,3)     
        internal static FormulaValue Sum(IRContext irContext, FormulaValue[] args)
        {
            return RunAggregator(new SumAgg(), irContext, args);
        }

        //  Sum(  [1,2,3], Value * Value)     
        public static FormulaValue SumTable(EvalVisitor runner, SymbolContext symbolContext, IRContext irContext, FormulaValue[] args)
        {
            return RunAggregator(new SumAgg(), runner, symbolContext, irContext, args);
        }

        // Average ignores blanks.
        //  Average(1,2,3)
        public static FormulaValue Average(IRContext irContext, FormulaValue[] args)
        {
            return RunAggregator(new AverageAgg(), irContext, args);
        }

        //  Average(  [1,2,3], Value * Value)     
        public static FormulaValue AverageTable(EvalVisitor runner, SymbolContext symbolContext, IRContext irContext, FormulaValue[] args)
        {
            var arg0 = (TableValue)args[0];

            if (arg0.Rows.Count() == 0)
            {
                return CommonErrors.DivByZeroError(irContext);
            }

            return RunAggregator(new AverageAgg(), runner, symbolContext, irContext, args);
        }

        // https://docs.microsoft.com/en-us/powerapps/maker/canvas-apps/functions/function-mod
        public static FormulaValue Mod(IRContext irContext, NumberValue[] args)
        {
            var arg0 = args[0];
            var arg1 = args[1];

            return new NumberValue(irContext, arg0.Value % arg1.Value);
        }

        // https://docs.microsoft.com/en-us/powerapps/maker/canvas-apps/functions/function-sequence
        public static FormulaValue Sequence(IRContext irContext, NumberValue[] args)
        {
            double records = args[0].Value;
            double start = args[1].Value;
            double step = args[2].Value;

            var rows = LazySequence(records, start, step).Select(n => new NumberValue(IRContext.NotInSource(FormulaType.Number), n));

            return new InMemoryTableValue(irContext, StandardTableNodeRecords(irContext, rows.ToArray()));
        }

        private static IEnumerable<double> LazySequence(double records, double start, double step)
        {
            double x = start;
            for (int i = 1; i <= records; i++)
            {
                yield return x;
                x += step;
            }
        }

        public static FormulaValue Abs(IRContext irContext, NumberValue[] args)
        {
            var arg0 = args[0];
            var x = arg0.Value;
            var val = System.Math.Abs(x);
            return new NumberValue(irContext, val);
        }

        // Char is used for PA string escaping 
        public static FormulaValue RoundUp(IRContext irContext, NumberValue[] args)
        {
            var numberArg = args[0].Value;
            var digitsArg = args[1].Value;

            var x = RoundUp(numberArg, digitsArg);
            return new NumberValue(irContext, x);
        }

        public static double RoundUp(double number, double digits)
        {
            if (digits == 0)
            {
                return number < 0 ? Math.floor(number) : Math.ceil(number);
            }

            double multiplier = Math.pow(10, digits < 0 ? Math.ceil(digits) : Math.floor(digits));
            // Contracts.Assert(multiplier != 0);

            // Deal with catastrophic loss of precision
            if (!isFinite(multiplier))
            {
                return number < 0 ? Math.floor(number) : Math.ceil(number);
            }

            // TASK: 74286: Spec corner case behavior: NaN, +Infinity, -Infinity.
            return number < 0 ?
                Math.floor(number * multiplier) / multiplier :
                Math.ceil(number * multiplier) / multiplier;
        }

        public static FormulaValue RoundDown(IRContext irContext, NumberValue[] args)
        {
            var numberArg = args[0].Value;
            var digitsArg = args[1].Value;

            var x = RoundDown(numberArg, digitsArg);
            return new NumberValue(irContext, x);
        }

        public static double RoundDown(double number, double digits)
        {
            if (digits == 0)
            {
                return number < 0 ? Math.ceil(number) : Math.floor(number);
            }

            var multiplier = Math.pow(10, digits < 0 ? Math.ceil(digits) : Math.floor(digits));
            // DebugContracts.assert(multiplier !== 0);

            // Deal with catastrophic loss of precision
            if (!isFinite(multiplier))
            {
                return number < 0 ? Math.ceil(number) : Math.floor(number);
            }

            // TASK: 74286: Spec corner case behavior: NaN, +Infinity, -Infinity.
            return number < 0 ?
                Math.ceil(number * multiplier) / multiplier :
                Math.floor(number * multiplier) / multiplier;
        }

        public static FormulaValue Int(IRContext irContext, NumberValue[] args)
        {
            var arg0 = args[0];
            var x = arg0.Value;
            var val = System.Math.Floor(x);
            return new NumberValue(irContext, val);
        }
    }
}
