﻿namespace StyleCop.Analyzers.Test.SpacingRules
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleCop.Analyzers.SpacingRules;
    using TestHelper;
    using Xunit;

    /// <summary>
    /// This class contains unit tests for <see cref="SA1007OperatorKeywordMustBeFollowedBySpace"/> and
    /// <see cref="OpenCloseSpacingCodeFixProvider"/>.
    /// </summary>
    public class SA1007UnitTests : CodeFixVerifier
    {
        [Fact]
        public async Task TestOperatorKeywordCasesAsync()
        {
            string testCode = @"
using System;
class ClassName
{
    public static bool operator==(ClassName x, ClassName y) { return false; }
    public static bool operator!=(ClassName x, ClassName y) { return false; }
    public static explicit operator@Boolean(ClassName x) { return false; }
}
";

            string fixedCode = @"
using System;
class ClassName
{
    public static bool operator ==(ClassName x, ClassName y) { return false; }
    public static bool operator !=(ClassName x, ClassName y) { return false; }
    public static explicit operator @Boolean(ClassName x) { return false; }
}
";

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithLocation(5, 24),
                this.CSharpDiagnostic().WithLocation(6, 24),
                this.CSharpDiagnostic().WithLocation(7, 28),
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new SA1007OperatorKeywordMustBeFollowedBySpace();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new OpenCloseSpacingCodeFixProvider();
        }
    }
}
