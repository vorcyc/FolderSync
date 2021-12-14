using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vorcyc.FolderSync
{
    internal sealed class Engine
    {

        private List<PathItem> _finnalItems = new List<PathItem>();

        private List<PathItem> _tempItems = new List<PathItem>();

        private string _sourceFolder, _targetFolder;


        private string GetRelativePath(string dir, string fn) =>
             fn.Substring(dir.Length + 1);


        //从源目录对比虚拟目标目录，以确定哪些创建、哪些该保持
        private void BuildFromSourceFolder()
        {

            foreach (var dirPath in Directory.GetDirectories(_sourceFolder, "*.*", SearchOption.AllDirectories))
            {

                var srcFolderRelativePath = GetRelativePath(_sourceFolder, dirPath);

                var targetFolder = Path.Combine(_targetFolder, srcFolderRelativePath);

                var item = new PathItem();

                if (!Directory.Exists(targetFolder))
                    item.Behaviour = Behaviour.Create; //源有，目标无，则创建
                else
                    item.Behaviour = Behaviour.Keep; //都有，则保持

                item.SourcePath = dirPath;
                item.TargetPath = targetFolder;
                item.PathType = PathType.Folder;

                _tempItems.Add(item);
            }


            foreach (var filePath in Directory.GetFiles(_sourceFolder, "*.*", SearchOption.AllDirectories))
            {

                var srcFileRelativePath = GetRelativePath(_sourceFolder, filePath);

                var targetFilename = Path.Combine(_targetFolder, srcFileRelativePath);

                var item = new PathItem();

                if (!File.Exists(targetFilename))
                    item.Behaviour = Behaviour.Create; //源有，目标无，则创建
                else
                    item.Behaviour = Behaviour.Keep; //都有，则保持

                item.SourcePath = filePath;
                item.TargetPath = targetFilename;
                item.PathType = PathType.File;


                _tempItems.Add(item);
            }
        }


        private void BuildFromTargetFolder()
        {

            foreach (var dirPath in Directory.GetDirectories(_targetFolder, "*.*", SearchOption.AllDirectories))
            {

                var targetFolderRelativePath = GetRelativePath(_targetFolder, dirPath);

                var srcFolder = Path.Combine(_sourceFolder, targetFolderRelativePath);

                var item = new PathItem();

                if (!Directory.Exists(srcFolder))
                    item.Behaviour = Behaviour.Delete; //源无，目标有，则删除
                else
                    item.Behaviour = Behaviour.Keep;

                item.SourcePath = srcFolder;
                item.TargetPath = dirPath;
                item.PathType = PathType.Folder;

                _tempItems.Add(item);
            }


            foreach (var filePath in Directory.GetFiles(_targetFolder, "*.*", SearchOption.AllDirectories))
            {
                var srcFileRelativePath = GetRelativePath(_targetFolder, filePath);

                var srcFilename = Path.Combine(_sourceFolder, srcFileRelativePath);

                var item = new PathItem();

                if (!File.Exists(srcFilename))
                    item.Behaviour = Behaviour.Delete; //源无，目标有，则删除
                else
                    item.Behaviour = Behaviour.Keep;

                item.SourcePath = srcFilename;
                item.TargetPath = filePath;
                item.PathType = PathType.File;


                _tempItems.Add(item);
            }
        }


        /// <summary>
        /// KEEP类检查：大小比较，尺寸不同则需要覆盖
        /// </summary>
        private void UpdateToOverride()
        {
            foreach (var pi in _tempItems)
            {
                if (pi.PathType == PathType.File && pi.Behaviour == Behaviour.Keep)
                {
                    var sfi = new FileInfo(pi.SourcePath);
                    var tfi = new FileInfo(pi.TargetPath);

                    if (sfi.Length != tfi.Length)
                    {
                        pi.Behaviour = Behaviour.Override;
                    }
                }
            }
        }



        private void BuildProperties()
        {
            this._filesToDelete = from p in _finnalItems
                                  where p.PathType == PathType.File
                                  && p.Behaviour == Behaviour.Delete
                                  select p;

            //-------------------

            this._foldersToDelete = from p in _finnalItems
                                    where p.PathType == PathType.Folder
                                    && p.Behaviour == Behaviour.Delete
                                    select p;

            //-------------------


            this._foldersToCreate = from p in _finnalItems
                                    where p.PathType == PathType.Folder
                                    && p.Behaviour == Behaviour.Create
                                    select p;

            //-------------------
            this._filesToCreate = from p in _finnalItems
                                  where p.PathType == PathType.File
                                  && p.Behaviour == Behaviour.Create
                                  select p;


            //------------------------
            this._filesToOverride = from p in _finnalItems
                                    where p.PathType == PathType.File
                                    && p.Behaviour == Behaviour.Override
                                    select p;
        }


        public Report Scan(string sourceFolder, string targetFolder)
        {
            _sourceFolder = sourceFolder;
            _targetFolder = targetFolder;
            _tempItems.Clear();
            _finnalItems.Clear();

            BuildFromSourceFolder();
            BuildFromTargetFolder();
            UpdateToOverride();

            var comparer = new PathItemComparer();
            var distItems = _tempItems.Distinct(comparer);

            int order = 1;
            foreach (var item in distItems)
            {
                item.Order = order;
                _finnalItems.Add(item);
                order++;
            }

            BuildProperties();
            _tempItems.Clear();
            return new Report(this);
        }

        public async Task<Report> ScanAsync(string sourceFolder, string targetFolder)
        {
            return await Task.Run(
             () =>
             {
                 return Scan(sourceFolder, targetFolder);
             });
        }

        public void Run()
        {
            foreach (var ftd in this.FilesToDelete)
                File.Delete(ftd.TargetPath);

            //-------------------

            //foreach (var ftd in this.FoldersToDelete)
            //    Directory.Delete(ftd.TargetPath);

            for (int i = this.FoldersToDelete.Count() -1; i >= 0; i--)
            {
                Directory.Delete(this.FoldersToDelete.ElementAt(i).TargetPath);
            }

            //-------------------

            foreach (var ftc in this.FoldersToCreate)
                Directory.CreateDirectory(ftc.TargetPath);

            //-------------------
            foreach (var ftc in this.FilesToCreate)
                File.Copy(ftc.SourcePath, ftc.TargetPath);

            //------------------------

            foreach (var fto in this.FilesToOverride)
            {
                File.Delete(fto.TargetPath);
                File.Copy(fto.SourcePath, fto.TargetPath);
            }
        }

        public async Task RunAsync()
        {
            await Task.Run(() =>
            {
                Run();
            });
        }



        public IEnumerable<PathItem> Items => _finnalItems;


        private IEnumerable<PathItem> _filesToDelete;
        public IEnumerable<PathItem> FilesToDelete => _filesToDelete;


        private IEnumerable<PathItem> _foldersToDelete;
        public IEnumerable<PathItem> FoldersToDelete => _foldersToDelete;


        private IEnumerable<PathItem> _foldersToCreate;
        public IEnumerable<PathItem> FoldersToCreate => _foldersToCreate;


        private IEnumerable<PathItem> _filesToCreate;
        public IEnumerable<PathItem> FilesToCreate => _filesToCreate;


        private IEnumerable<PathItem> _filesToOverride;
        public IEnumerable<PathItem> FilesToOverride => _filesToOverride;




    }










}
