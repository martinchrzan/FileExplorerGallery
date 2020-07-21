using FileExplorerGallery.Extensions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;
using static FileExplorerGallery.Helpers.Win32Apis;

namespace FileExplorerGallery.Helpers
{
    // This class is responsible for sorting files as seen in file explorer - if files are sorted by a date in file explorer, it should return them in the same order.
    public static class FileExplorerSortingProvider
    {
        private const string ComputerNodeName = "20D04FE0-3AEA-1069-A2D8-08002B30309D";
        private const string PicturesShellViewId = "{b3690e58-e961-423b-b687-386ebfd83239}";

        private static Dictionary<int, string> sortColumnMap = new Dictionary<int, string>()
        {
            { 0x0a, "name" },
            { 0x04, "type" },
            { 0x0c, "size" },
            { 0x64, "date" },
            { 0x0e, "modifieddate" },
            { 0x10, "accesseddate" }
        };

        public static IOrderedEnumerable<FileInfo> Sort(FileInfo[] files, string directory)
        {
            var pathToFindList = directory.Split('\\').ToList();

            var nodeSlot = FindNodeSlot(pathToFindList);

            if (string.IsNullOrEmpty(nodeSlot))
            {
                // we didn't find node slote, return unordered
                return files.OrderBy(it => 1);
            }
            var sortColumns = GetSortColumns(nodeSlot, "Shell");
            if (sortColumns.Count == 0)
            {
                sortColumns = GetSortColumns(nodeSlot, "ComDlg");
            }

            if(sortColumns.Count == 0)
            {
                // we didn't find sorting columns, return unordered
                return files.OrderBy(it => 1);
            }
            return SortByColumns(sortColumns, files);
        }

        private static IOrderedEnumerable<FileInfo> SortByColumns(List<Tuple<string, bool>> sortColumns, FileInfo[] files)
        {
            if(sortColumns.Count == 0)
            {
                Logger.LogWarning("No sorting setting found in registry");
                return files.OrderBy(it => 1);
            }

            // TODO add support for sorting on multiple columns
            var columnName = sortColumns[0].Item1;
            var isDescending = sortColumns[0].Item2;

            switch (columnName)
            {
                case "name":
                    if (isDescending)
                    {
                        return files.OrderByNaturalDescending(it => it.FullName);
                    }
                    return files.OrderByNaturalAscending(it => it.FullName);
                case "date":
                    return OrderByDate(files, isDescending);
                case "size":
                    if (isDescending)
                    {
                        return files.OrderByDescending(it => it.Length);
                    }
                    return files.OrderBy(it => it.Length);
                case "type":
                    if (isDescending)
                    {
                        return files.OrderByDescending(it => it.Extension);
                    }
                    return files.OrderBy(it => it.Extension);
                case "modifieddate":
                    if (isDescending)
                    {
                        return files.OrderByDescending(it => it.LastWriteTime);
                    }
                    return files.OrderBy(it => it.LastWriteTime);
                case "accesseddate":
                    if (isDescending)
                    {
                        return files.OrderByDescending(it => it.LastAccessTime);
                    }
                    return files.OrderBy(it => it.LastAccessTime);
                default:
                    // we don't know how to sort it yet, order by full name (not using natural ordering)
                    return files.OrderBy(it => it.FullName);
            }
        }

        private static IOrderedEnumerable<FileInfo> OrderByDate(FileInfo[] files, bool isDescending)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var filesWithCorrectDate = new Dictionary<FileInfo, DateTime>();
            foreach(var file in files)
            {
                filesWithCorrectDate[file] = GetRealCreationTime(file);
            }

            sw.Stop();
            var el = sw.ElapsedMilliseconds;
            if (isDescending)
            {
                filesWithCorrectDate.OrderByDescending(it => it.Value).Select(it => it.Key).OrderBy(a => 1);
            }
            return filesWithCorrectDate.OrderBy(it => it.Value).Select(it => it.Key).OrderBy(a => 1);
        }

        // File explorer uses date taken if a picture contains EXIF data for this property
        private static DateTime GetRealCreationTime(FileInfo image)
        {
            try
            {
                using (var picStream = new FileStream(image.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    BitmapSource bitSource = BitmapFrame.Create(picStream,BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
                    BitmapMetadata metaData = (BitmapMetadata)bitSource.Metadata;
                    return DateTime.Parse(metaData.DateTaken);
                }
            }
            catch
            {
                //do nothing  
            }
            return image.CreationTime;
        }

        private static List<Tuple<string, bool>> GetSortColumns(string nodeSlot, string subFolder)
        {
            var sortColumns = new List<Tuple<string, bool>>();

            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\Bags\" + nodeSlot + "\\" + subFolder, false))
            {
                if(key == null)
                {
                    return sortColumns;
                }

                var subkeys = key.GetSubKeyNames();

                var picturesSubkey = subkeys.FirstOrDefault(it => CultureInfo.CurrentCulture.CompareInfo.IndexOf(it, PicturesShellViewId, CompareOptions.IgnoreCase) >= 0);

                if (picturesSubkey != null)
                {

                    var sub = key.OpenSubKey(picturesSubkey);
                    var subkeyValues = sub.GetValueNames();
                    foreach (var subkeyvalue in subkeyValues)
                    {
                        if (subkeyvalue == "Sort")
                        {
                            sortColumns = DecodeSortColumns(sub, subkeyvalue);
                        }
                    }
                }
            }
            return sortColumns;
        }

        private static List<Tuple<string, bool>> DecodeSortColumns(RegistryKey sub, string subkeyvalue)
        {
            var sortColumns = new List<Tuple<string, bool>>();
            var sort = (byte[])sub.GetValue(subkeyvalue);

            // remove header
            sort = sort.Skip(16).ToArray();

            var numberOfSortingColumnsPart = sort.Take(4).ToArray();
            // remove number of columns part as we read it already
            sort = sort.Skip(4).ToArray();

            int numberOfSortingColumns = BitConverter.ToInt32(numberOfSortingColumnsPart, 0);

            for (int i = 0; i < numberOfSortingColumns; i++)
            {
                // skip fmtid (guid)
                sort = sort.Skip(16).ToArray();
                var sortingColumnNamePart = BitConverter.ToUInt16(sort.Take(4).ToArray(), 0);
                // remove sorting column name
                sort = sort.Skip(4).ToArray();

                var directionDescending = BitConverter.ToUInt16(sort.Take(4).ToArray(), 0) == 0xFFFF;

                // remove direction part
                sort = sort.Skip(4).ToArray();

                var sortingColumnName = "Unknown";
                if (sortColumnMap.ContainsKey(sortingColumnNamePart))
                {
                    sortingColumnName = sortColumnMap[sortingColumnNamePart];
                }

                sortColumns.Add(new Tuple<string, bool>(sortingColumnName, directionDescending));
            }
            return sortColumns;
        }

        private static string FindNodeSlot(List<string> pathToFindList)
        {
            string nodeSlot = "";
            using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Classes\Local Settings\Software\Microsoft\Windows\Shell\BagMRU", false))
            {
                var valueNames = key.GetValueNames();

                foreach (var valueName in valueNames)
                {
                    if (char.IsDigit(valueName[0]))
                    {
                        var val = (byte[])key.GetValue(valueName);
                        var folderName = GetFolderName(val);

                        if (folderName?.Contains(ComputerNodeName) == true)
                        {
                            nodeSlot = RecursiveFindNodeSlotForPath(key.OpenSubKey(valueName), pathToFindList);
                            break;
                        }
                    }
                }
            }

            return nodeSlot;
        }

        private static string RecursiveFindNodeSlotForPath(RegistryKey key, List<string> pathToFind)
        {
            var valueNames = key.GetValueNames();

            foreach (var valueName in valueNames)
            {
                if (pathToFind.Count == 0)
                {
                    if (valueName == "NodeSlot")
                    {
                        return key.GetValue(valueName).ToString();
                    }
                }
                else if (char.IsDigit(valueName[0]))
                {
                    var val = (byte[])key.GetValue(valueName);
                    var folderName = GetFolderName(val);

                    if (folderName == pathToFind.First())
                    {
                        // we have a match, search for the remaining path
                        pathToFind.RemoveAt(0);
                        return RecursiveFindNodeSlotForPath(key.OpenSubKey(valueName), pathToFind);
                    }
                }
            }
            return null;
        }

        private static string GetFolderName(byte[] IDL)
        {
            GCHandle pinnedArray = GCHandle.Alloc(IDL, GCHandleType.Pinned);
            IntPtr PIDL = pinnedArray.AddrOfPinnedObject();
            StringBuilder name = new StringBuilder(2048);
            int result = SHGetNameFromIDList(PIDL, 0x80018001, out name);
            pinnedArray.Free();
            if (name == null)
            {
                var encoded = Encoding.UTF8.GetString(IDL);
                encoded = new string(encoded.Where(c => !char.IsControl(c)).ToArray());
                encoded = encoded.Replace("/", "");
                encoded = encoded.Replace(@"\\", @"\");
                encoded = encoded.Replace(@"\", "");

                return encoded;
            }
            return name.ToString();
        }
    }
}
