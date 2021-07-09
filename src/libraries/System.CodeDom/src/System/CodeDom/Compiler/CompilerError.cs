// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace System.CodeDom.Compiler
{
    public class CompilerError
    {
        public CompilerError() : this(string.Empty, 0, 0, string.Empty, string.Empty) { }

        public CompilerError(string fileName, int line, int column, string errorNumber, string errorText)
        {
            Line = line;
            Column = column;
            ErrorNumber = errorNumber;
            ErrorText = errorText;
            FileName = fileName;
        }

        public int Line { get; set; }

        public int Column { get; set; }

        public string ErrorNumber { get; set; }

        public string ErrorText { get; set; }

        public bool IsWarning { get; set; }

        public string FileName { get; set; }

        public override string ToString() => FileName.Length > 0 ?
            string.Create(CultureInfo.InvariantCulture, $"{FileName}({Line},{Column}) : {WarningString} {ErrorNumber}: {ErrorText}") :
            string.Create(CultureInfo.InvariantCulture, $"{WarningString} {ErrorNumber}: {ErrorText}");

        private string WarningString => IsWarning ? "warning" : "error";
    }
}
