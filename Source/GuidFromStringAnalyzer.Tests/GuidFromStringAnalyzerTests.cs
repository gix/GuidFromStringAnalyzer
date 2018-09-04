namespace GuidFromStringAnalyzer.Tests
{
    using System;
    using GuidFromStringAnalysis;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class UnitTest : CodeFixVerifier
    {
        [Fact]
        public void GuidEquality()
        {
            // Check that the fix-its below are actually sane.
            Assert.Equal(
                new Guid(0x45ACA620, 0xA438, 0x43C8, 0xBC, 0xD5, 0xFC, 0x87, 0x97, 0xCB, 0xEC, 0x88),
                new Guid("45ACA620-A438-43C8-BCD5-FC8797CBEC88"));
            Assert.Equal(
                new Guid(0x54EC8794, 0x49AA, 0x48FB, 0x95, 0x3E, 0xC6, 0xE5, 0x93, 0x55, 0xDB, 0xFB),
                new Guid("54EC8794-49AA-48FB-953E-C6E59355DBFB"));
        }

        [Fact]
        public void Empty()
        {
            const string source = @"";

            VerifyCSharpDiagnostic(source);
        }

        [Fact]
        public void NoFix()
        {
            const string source = @"using System;
class Foo
{
    public static readonly Guid Id = new Guid(0x45ACA620, 0xA438, 0x43C8, 0xBC, 0xD5, 0xFC, 0x87, 0x97, 0xCB, 0xEC, 0x88);

    public Guid Create()
    {
        return new Guid(0x45ACA620, 0xA438, 0x43C8, 0xBC, 0xD5, 0xFC, 0x87, 0x97, 0xCB, 0xEC, 0x88);
    }

    public Guid Create(string str)
    {
        return new Guid(str);
    }

    public Guid Create(byte[] buffer)
    {
        return new Guid(buffer);
    }
}";

            VerifyCSharpDiagnostic(source);
        }

        [Fact]
        public void DiagnoseAndFixMethod()
        {
            const string source = @"using System;
class Foo
{
    public Guid Bar()
    {
        return new Guid(""45ACA620-A438-43C8-BCD5-FC8797CBEC88"");
    }
}";

            const string fixedSource = @"using System;
class Foo
{
    public Guid Bar()
    {
        return new Guid(0x45ACA620, 0xA438, 0x43C8, 0xBC, 0xD5, 0xFC, 0x87, 0x97, 0xCB, 0xEC, 0x88);
    }
}";

            var expected = new DiagnosticResult {
                Id = "GuidFromString",
                Message = "Guid constructed from string",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 6, 25)
                }
            };

            VerifyCSharpDiagnostic(source, expected);
            VerifyCSharpFix(source, fixedSource);
        }

        [Fact]
        public void DiagnoseAndFixFields()
        {
            const string source = @"using System;
class Foo
{
    public static readonly Guid Bar = new Guid(""45ACA620-A438-43C8-BCD5-FC8797CBEC88"");
    public static readonly Guid Qux = new Guid(""54EC8794-49AA-48FB-953E-C6E59355DBFB"");

    public static readonly Key<string> NameKey =
        new Key<string>(new Guid(
            ""5636571C-2B61-442E-BD6B-199A7BFA1B3B""), 2);
}";

            const string fixedSource = @"using System;
class Foo
{
    public static readonly Guid Bar = new Guid(0x45ACA620, 0xA438, 0x43C8, 0xBC, 0xD5, 0xFC, 0x87, 0x97, 0xCB, 0xEC, 0x88);
    public static readonly Guid Qux = new Guid(0x54EC8794, 0x49AA, 0x48FB, 0x95, 0x3E, 0xC6, 0xE5, 0x93, 0x55, 0xDB, 0xFB);

    public static readonly Key<string> NameKey =
        new Key<string>(new Guid(
            0x5636571C, 0x2B61, 0x442E, 0xBD, 0x6B, 0x19, 0x9A, 0x7B, 0xFA, 0x1B, 0x3B), 2);
}";

            var expected1 = new DiagnosticResult {
                Id = "GuidFromString",
                Message = "Guid constructed from string",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 4, 48)
                }
            };

            var expected2 = new DiagnosticResult {
                Id = "GuidFromString",
                Message = "Guid constructed from string",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 5, 48)
                }
            };

            var expected3 = new DiagnosticResult {
                Id = "GuidFromString",
                Message = "Guid constructed from string",
                Severity = DiagnosticSeverity.Info,
                Locations = new[] {
                    new DiagnosticResultLocation("Test0.cs", 9, 13)
                }
            };

            VerifyCSharpDiagnostic(source, expected1, expected2, expected3);
            VerifyCSharpFix(source, fixedSource);
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new GuidFromStringAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new GuidFromStringCodeFixProvider();
        }
    }
}
