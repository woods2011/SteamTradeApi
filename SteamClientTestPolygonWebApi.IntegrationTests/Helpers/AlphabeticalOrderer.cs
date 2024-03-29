﻿using Xunit.Abstractions;
using Xunit.Sdk;

namespace SteamClientTestPolygonWebApi.IntegrationTests.Helpers;

public class AlphabeticalOrderer : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(
        IEnumerable<TTestCase> testCases) where TTestCase : ITestCase =>
        testCases.OrderBy(testCase => testCase.TestMethod.Method.Name);
}