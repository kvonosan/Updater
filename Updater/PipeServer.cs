using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;

namespace Updater
{
    class fileinjson
    {
        public string filename;
        public string md5;

        public fileinjson()
        {
            filename = "";
            md5 = "";
        }
    }

    static class bytetohex
    {
        public static string ToHex(this byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);

            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));

            return result.ToString();
        }
    }

    class PipeServer
    {
        private NamedPipeServerStream server;
        private StreamReader sr;
        private StreamWriter sw;
        private WebClient client = new WebClient();
        private List<fileinjson> files = new List<fileinjson>();
        private bool log = true;

        private void SendMessage(string mess)
        {
            sw.WriteLine(mess);
            sw.Flush();
        }

        private bool UpdateNow(string programm_name, double version)
        {
            if (programm_name.StartsWith("catpost_vk_content"))
            {
                string str = client.DownloadString("http://tomnolane.ru/catpost_vk_content/catpost_vk_content.version").ToString();
                double net_version  = double.Parse(str, CultureInfo.InvariantCulture);
                if (net_version > version)
                {
                    return true;
                }
            }
            else if (programm_name.StartsWith("catpost_scanner"))
            {
                string str = client.DownloadString("http://tomnolane.ru/catpost_scanner/catpost_scanner.version").ToString();
                double net_version = double.Parse(str, CultureInfo.InvariantCulture);
                if (net_version > version)
                {
                    return true;
                }
            }
            return false;
        }

        private void Update(string programm_name, string path)
        {
            if (programm_name.StartsWith("catpost_vk_content"))
            {
                string update_file = client.DownloadString("http://tomnolane.ru/update.php?catpost_vk_content=1").ToString();
                var obj = JObject.Parse(update_file);
                List<string> str = new List<string>();
                foreach (var obj1 in obj.Properties().Select(p => p.Name))
                {
                    str.Add(obj1.ToString());
                }
                int index = 0;
                foreach (var obj2 in obj)
                {
                    fileinjson file = new fileinjson();
                    file.filename = str[index];
                    file.md5 = obj2.Value.ToString();
                    index++;
                    files.Add(file);
                }
                foreach (var file in files)
                {
                    var md5 = MD5.Create();
                    if (!File.Exists(path + "/" + file.filename))
                    {
                        if (log)
                        {
                            File.AppendAllText("update_debug.log", "обновляю файл " + file.filename + Environment.NewLine);
                        }
                        string str_download = "http://tomnolane.ru/catpost_vk_content/" + file.filename;
                        client.DownloadFile(str_download, path + "/" + file.filename);
                    }
                    var stream = File.OpenRead(path + "/" + file.filename);
                    var md5_file = bytetohex.ToHex(md5.ComputeHash(stream), false);
                    stream.Close();
                    if (md5_file != file.md5)
                    {
                        if (log)
                        {
                            File.AppendAllText("update_debug.log", "обновляю файл " + file.filename + Environment.NewLine);
                        }
                        string str_download = "http://tomnolane.ru/catpost_vk_content/" + file.filename;
                        client.DownloadFile(str_download, path + "/" + file.filename);
                    }
                }
            } else if (programm_name.StartsWith("catpost_scanner"))
            {
                string update_file = client.DownloadString("http://tomnolane.ru/update.php?catpost_scanner=1").ToString();
                var obj = JObject.Parse(update_file);
                List<string> str = new List<string>();
                foreach (var obj1 in obj.Properties().Select(p => p.Name))
                {
                    str.Add(obj1.ToString());
                }
                int index = 0;
                foreach (var obj2 in obj)
                {
                    fileinjson file = new fileinjson();
                    file.filename = str[index];
                    file.md5 = obj2.Value.ToString();
                    index++;
                    files.Add(file);
                }
                foreach (var file in files)
                {
                    var md5 = MD5.Create();
                    if (!File.Exists(path + "/" + file.filename))
                    {
                        if (log)
                        {
                            File.AppendAllText("update_debug.log", "обновляю файл " + file.filename + Environment.NewLine);
                        }
                        string str_download = "http://tomnolane.ru/catpost_scanner/" + file.filename;
                        client.DownloadFile(str_download, path + "/" + file.filename);
                    }
                    var stream = File.OpenRead(path + "/" + file.filename);
                    var md5_file = bytetohex.ToHex(md5.ComputeHash(stream), false);
                    stream.Close();
                    if (md5_file != file.md5)
                    {
                        if (log)
                        {
                            File.AppendAllText("update_debug.log", "обновляю файл " + file.filename + Environment.NewLine);
                        }
                        string str_download = "http://tomnolane.ru/catpost_scanner/" + file.filename;
                        client.DownloadFile(str_download, path + "/" + file.filename);
                    }
                }
            }
        }

        public void Work()
        {
            try
            {
                if (log)
                {
                    File.AppendAllText("update_debug.log", "Ждем соединений.." + Environment.NewLine);
                }
                server.WaitForConnection();
                SendMessage("ready");

                string mess = sr.ReadLine();
                if (mess == "update")
                {
                    if (log)
                    {
                        File.AppendAllText("update_debug.log", "прислано update.." + Environment.NewLine);
                    }

                    SendMessage("getpath");

                    string path = sr.ReadLine();
                    if (log)
                    {
                        File.AppendAllText("update_debug.log", path + Environment.NewLine);
                    }

                    SendMessage("getversion");

                    string str = sr.ReadLine();
                    double version = double.Parse(str, CultureInfo.InvariantCulture);
                    if (log)
                    {
                        File.AppendAllText("update_debug.log", version.ToString() + Environment.NewLine);
                    }
                    SendMessage("getexename");

                    string exe_name = sr.ReadLine();
                    if (log)
                    {
                        File.AppendAllText("update_debug.log", exe_name + Environment.NewLine);
                    }

                    SendMessage("getid");

                    string exe_id = sr.ReadLine();
                    if (log)
                    {
                        File.AppendAllText("update_debug.log", exe_id + Environment.NewLine);
                    }

                    if (UpdateNow(exe_name, version))
                    {
                        Process.GetProcessById(int.Parse(exe_id)).Kill();
                        if (log)
                        {
                            File.AppendAllText("update_debug.log", "процесс убит" + Environment.NewLine);
                        }
                        Thread.Sleep(1000);
                        Update(exe_name, path);
                        Process proc = new Process();
                        proc.StartInfo.FileName = path + "/" + exe_name;
                        proc.StartInfo.WorkingDirectory = Path.GetDirectoryName(path + "/");
                        proc.Start();
                        if (log)
                        {
                            File.AppendAllText("update_debug.log", "процесс запущен" + Environment.NewLine);
                        }
                    }
                    Environment.Exit(0);
                }
            } catch (Exception ex)
            {
                File.AppendAllText("update_debug.log", "ошибка " + ex.Message);
            }            
        }
		
		public PipeServer()
        {
            server = new NamedPipeServerStream("catpost_update");
            sr = new StreamReader(server);
            sw = new StreamWriter(server);
        }
    }
}
