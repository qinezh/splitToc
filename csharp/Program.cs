using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.DocAsCode.DataContracts.Common;
using Microsoft.DocAsCode.Common;
using YamlDotNet.Core;

namespace SplitToc
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine($"Usage: {AppDomain.CurrentDomain.FriendlyName} [toc_path]");
                return 1;
            }

            var tocPath = args[0];
            SplitToc(tocPath);
            return 0;
        }

        private static void SplitToc(string originalTocPath)
        {
            if (!Path.IsPathRooted(originalTocPath))
            {
                originalTocPath = Path.GetFullPath(originalTocPath);
            }

            if (!File.Exists(originalTocPath))
            {
                throw new FileNotFoundException($"The path of toc file: {originalTocPath} can't be found");
            }

            var originalTocDir = Path.GetDirectoryName(originalTocPath);
            if (originalTocDir == null)
            {
                throw new Exception($"The directory of {originalTocPath} can't be null");
            }

            TocViewModel tocModel;
            using (var stream = File.OpenRead(originalTocPath))
            using (var reader = new StreamReader(stream))
            {
                try
                {
                    tocModel = YamlUtility.Deserialize<TocViewModel>(reader);
                }
                catch (YamlException ex)
                {
                    Console.WriteLine($"Error occurs while parsing toc {originalTocPath}, please check if the format is correct. {ex.Message}");
                    throw;
                }
            }

            if (tocModel == null)
            {
                Console.WriteLine($"No toc model parsed for {originalTocPath}.");
                return;
            }

            var mergedTocModel = new TocViewModel();
            foreach (var ns in tocModel)
            {
                var splittedTocPath = Path.Combine(originalTocDir, "_splitted", ns.Uid, "toc.yml");
                var splittedTocRelativePath = PathUtility.MakeRelativePath(originalTocDir, splittedTocPath);
                var reversedRelativePath = PathUtility.MakeRelativePath(splittedTocPath, originalTocDir);
                ProcessHref(ns, reversedRelativePath.Replace('\\', '/'));

                var splittedTocDir = Path.GetDirectoryName(splittedTocPath);
                if (splittedTocDir == null)
                {
                    throw new Exception($"The directory of {splittedTocPath} can't be null");
                }

                if (!Directory.Exists(splittedTocDir))
                {
                    Directory.CreateDirectory(splittedTocDir);
                }

                var splittedTocItem = new TocItemViewModel
                {
                    Uid = ns.Uid,
                    Name = ns.Name,
                    Items = ns.Items
                };
                if (ns.Metadata != null)
                {
                    splittedTocItem.Metadata = ns.Metadata;
                }

                var splittedTocModel = new TocViewModel(new List<TocItemViewModel> {splittedTocItem});

                YamlUtility.Serialize(splittedTocPath, splittedTocModel);
                Console.WriteLine($"Create new splitted toc ({splittedTocPath})");

                var mergedTocItem = new TocItemViewModel
                {
                    Uid = ns.Uid,
                    Name = ns.Name,
                    Href = Path.GetDirectoryName(splittedTocRelativePath).Replace('\\', '/') + "/"
                };

                mergedTocModel.Add(mergedTocItem);
            }

            YamlUtility.Serialize(originalTocPath, mergedTocModel);
            Console.WriteLine($"Rewrite original toc file ({originalTocPath})");
        }

        private static void ProcessHref(TocItemViewModel item, string relativePath)
        {
            if (item.Items == null || item.Items.Count == 0)
            {
                return;
            }
            foreach (var i in item.Items)
            {
                i.Href = PathUtility.IsRelativePath(i.Href) ? Path.Combine(relativePath, i.Href) : i.Href;
                ProcessHref(i, relativePath);
            }
        }
    }
}
