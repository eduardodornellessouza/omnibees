using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Helper
{
    public class FTPManager
    {
        private int _port;
        private string _host;
        private string _username;
        private string _password;

        public FTPManager(string host, int port, string username, string password)
        {
            _port = port;
            _host = host;
            _username = username;
            _password = password;
        }

        public IAsyncResult UploadFileAsync(string fileName, FileStream fileStream)
        {
            IAsyncResult result;
            using (var client = new SftpClient(_host, _port, _username, _password))
            {
                try
                {
                    client.Connect();
                    result = client.BeginUploadFile(fileStream, fileName);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return result;
        }

        public bool UploadFile(string fileName, Stream fileStream)
        {
            using (var client = new SftpClient(_host, _port, _username, _password))
            {
                try
                {
                    client.Connect();
                    client.UploadFile(fileStream, fileName);
                    client.Disconnect();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return true;
        }

        public bool UploadFile(string fileName, string fileContent)
        {
            using (var client = new SftpClient(_host, _port, _username, _password))
            {
                try
                {
                    client.Connect();
                    client.CreateText(fileName);
                    client.AppendAllText(fileName, fileContent);
                    client.Disconnect();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return true;
        }
    }
}
