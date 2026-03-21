// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Win32.SafeHandles;
using Xunit;

namespace System.IO.Tests
{
    public class File_TryOpen : FileCleanupTestBase
    {
        [Fact]
        public void TryOpen_ExistingFile_ReturnsTrue()
        {
            string path = GetTestFilePath();
            System.IO.File.WriteAllText(path, "hello");

            Assert.True(System.IO.File.TryOpen(path, FileMode.Open, out FileStream? stream));
            Assert.NotNull(stream);
            using (stream)
            {
                Assert.True(stream.CanRead);
            }
        }

        [Fact]
        public void TryOpen_NonExistentFile_ReturnsFalse()
        {
            string path = GetTestFilePath();

            Assert.False(System.IO.File.TryOpen(path, FileMode.Open, out FileStream? stream));
            Assert.Null(stream);
        }

        [Fact]
        public void TryOpen_CreateNew_ExistingFile_ReturnsFalse()
        {
            string path = GetTestFilePath();
            System.IO.File.WriteAllText(path, "hello");

            Assert.False(System.IO.File.TryOpen(path, FileMode.CreateNew, out FileStream? stream));
            Assert.Null(stream);
        }

        [Fact]
        public void TryOpen_Create_CreatesFile()
        {
            string path = GetTestFilePath();

            Assert.True(System.IO.File.TryOpen(path, FileMode.Create, out FileStream? stream));
            Assert.NotNull(stream);
            using (stream)
            {
                Assert.True(stream.CanWrite);
            }

            Assert.True(System.IO.File.Exists(path));
        }

        [Fact]
        public void TryOpen_WithAccessAndShare_ReturnsTrue()
        {
            string path = GetTestFilePath();
            System.IO.File.WriteAllText(path, "hello");

            Assert.True(System.IO.File.TryOpen(path, FileMode.Open, FileAccess.Read, FileShare.Read, out FileStream? stream));
            Assert.NotNull(stream);
            using (stream)
            {
                Assert.True(stream.CanRead);
                Assert.False(stream.CanWrite);
            }
        }

        [Fact]
        public void TryOpen_WithModeAndAccess_ReturnsTrue()
        {
            string path = GetTestFilePath();
            System.IO.File.WriteAllText(path, "hello");

            Assert.True(System.IO.File.TryOpen(path, FileMode.Open, FileAccess.Read, out FileStream? stream));
            Assert.NotNull(stream);
            using (stream)
            {
                Assert.True(stream.CanRead);
            }
        }

        [Fact]
        public void TryOpen_WithOptions_ReturnsTrue()
        {
            string path = GetTestFilePath();
            System.IO.File.WriteAllText(path, "hello");

            var options = new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read };
            Assert.True(System.IO.File.TryOpen(path, options, out FileStream? stream));
            Assert.NotNull(stream);
            using (stream)
            {
                Assert.True(stream.CanRead);
            }
        }

        [Fact]
        public void TryOpen_WithOptions_NonExistent_ReturnsFalse()
        {
            string path = GetTestFilePath();

            var options = new FileStreamOptions { Mode = FileMode.Open, Access = FileAccess.Read };
            Assert.False(System.IO.File.TryOpen(path, options, out FileStream? stream));
            Assert.Null(stream);
        }

        [Fact]
        public void TryOpen_NullPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => System.IO.File.TryOpen(null!, FileMode.Open, out _));
        }

        [Fact]
        public void TryOpen_EmptyPath_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => System.IO.File.TryOpen("", FileMode.Open, out _));
        }

        [Fact]
        public void TryOpen_InvalidMode_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => System.IO.File.TryOpen(GetTestFilePath(), (FileMode)(-1), out _));
        }

        [Fact]
        public void TryOpen_SharingViolation_ReturnsFalse()
        {
            string path = GetTestFilePath();
            using (var lockedStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Assert.False(System.IO.File.TryOpen(path, FileMode.Open, FileAccess.Read, FileShare.None, out FileStream? stream));
                Assert.Null(stream);
            }
        }
    }

    public class File_TryOpenHandle : FileCleanupTestBase
    {
        [Fact]
        public void TryOpenHandle_ExistingFile_ReturnsTrue()
        {
            string path = GetTestFilePath();
            System.IO.File.WriteAllText(path, "hello");

            Assert.True(System.IO.File.TryOpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read, out SafeFileHandle? handle));
            Assert.NotNull(handle);
            Assert.False(handle.IsInvalid);
            handle.Dispose();
        }

        [Fact]
        public void TryOpenHandle_NonExistent_ReturnsFalse()
        {
            string path = GetTestFilePath();

            Assert.False(System.IO.File.TryOpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read, out SafeFileHandle? handle));
            Assert.Null(handle);
        }

        [Fact]
        public void TryOpenHandle_CreateNew_ExistingFile_ReturnsFalse()
        {
            string path = GetTestFilePath();
            System.IO.File.WriteAllText(path, "hello");

            Assert.False(System.IO.File.TryOpenHandle(path, FileMode.CreateNew, FileAccess.Write, FileShare.None, out SafeFileHandle? handle));
            Assert.Null(handle);
        }

        [Fact]
        public void TryOpenHandle_Create_CreatesFile()
        {
            string path = GetTestFilePath();

            Assert.True(System.IO.File.TryOpenHandle(path, FileMode.Create, FileAccess.Write, FileShare.None, out SafeFileHandle? handle));
            Assert.NotNull(handle);
            Assert.False(handle.IsInvalid);
            handle.Dispose();
            Assert.True(System.IO.File.Exists(path));
        }

        [Fact]
        public void TryOpenHandle_FullOverload_ReturnsTrue()
        {
            string path = GetTestFilePath();
            System.IO.File.WriteAllText(path, "hello");

            Assert.True(System.IO.File.TryOpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.Read, FileOptions.None, 0, out SafeFileHandle? handle));
            Assert.NotNull(handle);
            Assert.False(handle.IsInvalid);
            handle.Dispose();
        }

        [Fact]
        public void TryOpenHandle_NullPath_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => System.IO.File.TryOpenHandle(null!, FileMode.Open, FileAccess.Read, FileShare.Read, out _));
        }

        [Fact]
        public void TryOpenHandle_SharingViolation_ReturnsFalse()
        {
            string path = GetTestFilePath();
            using (var lockedStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Assert.False(System.IO.File.TryOpenHandle(path, FileMode.Open, FileAccess.Read, FileShare.None, out SafeFileHandle? handle));
                Assert.Null(handle);
            }
        }
    }
}
