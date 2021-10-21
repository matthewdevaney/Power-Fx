﻿//------------------------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.PowerFx.Core.Public.Types;

namespace Microsoft.PowerFx.Tests
{
    [TestClass]
    public class DependencyFinderTests
    {
        [DataTestMethod]
        [DataRow("A + 3 + B + B", "A,B")]
        [DataRow("Filter(Accounts, Age>30)", "Accounts")] // record scope not included
        [DataRow("Filter(Accounts, ThisRecord.Age>30)", "Accounts")] // ThisRecord is implicit
        [DataRow("Filter(Accounts, Age>B)", "Accounts,B")] // captures
        [DataRow("With({B:15}, B> A)", "A")] // B is shadowed
        public void T1(string expr, string dependsOn)
        {
            // var expected = new HashSet<string>(dependsOn.Split(','));

            var engine = new RecalcEngine();

            var accountType = new TableType()
                .Add(new NamedFormulaType("Age", FormulaType.Number));

            var type = new RecordType()
                .Add(new NamedFormulaType("A", FormulaType.Number))
                .Add(new NamedFormulaType("B", FormulaType.Number))
                .Add(new NamedFormulaType("Accounts", accountType));
            var result = engine.Check(expr, type);

            Assert.IsTrue(result.IsSuccess);

            // sets should be equal
            var sorted = result.TopLevelIdentifiers.OrderBy(x=>x).ToArray();
            var actualStr = string.Join(',', sorted);

            Assert.AreEqual(dependsOn, actualStr);
        }
    }
}