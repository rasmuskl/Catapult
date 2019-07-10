using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Catapult.Core.Indexes.Extensions;

namespace Catapult.Core.Indexes
{
    public class DirectoryTraverser
    {
        private readonly ExtensionContainer _extensionContainer;

        public DirectoryTraverser()
        {
            //_extensionContainer = new ExtensionReader().ReadRegistry();
            _extensionContainer = new ExtensionContainer(new ExtensionInfo[0]);
        }

        public ImmutableList<FileItem> GetFiles(DirectoryInfo directory)
        {
            var directoryQueue = new Queue<DirectoryInfo>();

            directoryQueue.Enqueue(directory);

            var fileItems = new List<FileItem>();

            while (directoryQueue.Any())
            {
                DirectoryInfo nextDirectory = directoryQueue.Dequeue();

                try
                {
                    FileSystemInfo[] fileSystemInfos = nextDirectory.GetFileSystemInfos();

                    foreach (var fileSystemInfo in fileSystemInfos)
                    {
                        if ((fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            directoryQueue.Enqueue(new DirectoryInfo(fileSystemInfo.FullName));
                        }
                        else
                        {
                            if (_extensionContainer.IsKnownExtension(fileSystemInfo.Extension))
                            {
                                fileItems.Add(new FileItem(fileSystemInfo.FullName));
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (PathTooLongException)
                {
                }
            }

            return fileItems.ToImmutableList();
        }
    }
}