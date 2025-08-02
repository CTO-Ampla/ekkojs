using Microsoft.ClearScript;
using System.Dynamic;

namespace EkkoJS.Core.Modules.BuiltIn;

public class FileSystemModule : IModule
{
    public string Name => "fs";
    public string Protocol => "ekko";

    public object GetExports()
    {
        dynamic exports = new ExpandoObject();
        
        exports.readFile = new Func<string, string>(path =>
        {
            return File.ReadAllText(path);
        });

        exports.readFileSync = new Func<string, string>(path =>
        {
            return File.ReadAllText(path);
        });

        exports.writeFile = new Action<string, string>((path, content) =>
        {
            File.WriteAllText(path, content);
        });

        exports.writeFileSync = new Action<string, string>((path, content) =>
        {
            File.WriteAllText(path, content);
        });

        exports.exists = new Func<string, bool>(path =>
        {
            return File.Exists(path) || Directory.Exists(path);
        });

        exports.existsSync = new Func<string, bool>(path =>
        {
            return File.Exists(path) || Directory.Exists(path);
        });

        exports.mkdir = new Action<string>(path =>
        {
            Directory.CreateDirectory(path);
        });

        exports.mkdirSync = new Action<string>(path =>
        {
            Directory.CreateDirectory(path);
        });

        exports.readdir = new Func<string, string[]>(path =>
        {
            var files = Directory.GetFiles(path);
            var dirs = Directory.GetDirectories(path);
            return files.Concat(dirs).Select(Path.GetFileName).ToArray()!;
        });

        exports.readdirSync = new Func<string, string[]>(path =>
        {
            var files = Directory.GetFiles(path);
            var dirs = Directory.GetDirectories(path);
            return files.Concat(dirs).Select(Path.GetFileName).ToArray()!;
        });

        exports.rm = new Action<string>(path =>
        {
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, true);
        });

        exports.rmSync = new Action<string>(path =>
        {
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, true);
        });

        exports.stat = new Func<string, object>(path =>
        {
            dynamic stat = new ExpandoObject();
            
            if (File.Exists(path))
            {
                var info = new FileInfo(path);
                stat.isFile = true;
                stat.isDirectory = false;
                stat.size = info.Length;
                stat.mtime = info.LastWriteTime;
                stat.ctime = info.CreationTime;
                stat.atime = info.LastAccessTime;
            }
            else if (Directory.Exists(path))
            {
                var info = new DirectoryInfo(path);
                stat.isFile = false;
                stat.isDirectory = true;
                stat.size = 0;
                stat.mtime = info.LastWriteTime;
                stat.ctime = info.CreationTime;
                stat.atime = info.LastAccessTime;
            }
            else
            {
                throw new FileNotFoundException($"Path not found: {path}");
            }
            
            return stat;
        });

        exports.statSync = exports.stat;

        return exports;
    }
}