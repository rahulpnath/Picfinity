using Picfinity.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Picfinity.Common
{
    class HtmlHelper
    {


        public async static Task<byte[]> GetMultiFormPostData(StorageFile file)
        {
            var read = await file.OpenReadAsync();
            var data = new byte[read.Size];
            BinaryReader readerB = new BinaryReader(read.AsStreamForRead());
            readerB.Read(data, 0, (int)read.Size);

            //DataReader reader = new DataReader(read);
            //await reader.LoadAsync((uint)read.Size);
            //reader.ReadBytes(data);
            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("file", new FileParameter(data, file.Name, "image/jpeg"));
            return GetMultipartFormData(param, AppSettings.FormDataBoundary);
        }

        private static byte[] GetMultipartFormData(Dictionary<string, object> postParameters, string boundary)
        {
            Stream formDataStream = new System.IO.MemoryStream();
            bool needsCLRF = false;
            Encoding encoding = Encoding.UTF8 ;

            foreach (var param in postParameters)
            {
                // Thanks to feedback from commenters, add a CRLF to allow multiple parameters to be added.
                // Skip it on the first parameter, add it to subsequent parameters.
                if (needsCLRF)
                    formDataStream.Write(encoding.GetBytes("\r\n"), 0, encoding.GetByteCount("\r\n"));

                needsCLRF = true;

                if (param.Value is FileParameter)
                {
                    FileParameter fileToUpload = (FileParameter)param.Value;

                    // Add just the first part of this param, since we will write the file data directly to the Stream
                    string header = string.Format("--{0}\r\nContent-Type: {3}; name=\"{1}\"\r\nContent-Transfer-Encoding: binary\r\nContent-ID: <file>\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Location: file\r\n\r\n",
                        boundary,
                        param.Key,
                        fileToUpload.FileName ?? param.Key,
                        fileToUpload.ContentType ?? "application/octet-stream");

                    formDataStream.Write(encoding.GetBytes(header), 0, encoding.GetByteCount(header));

                    // Write the file data directly to the Stream, rather than serializing it to a string.
                    formDataStream.Write(fileToUpload.File, 0, fileToUpload.File.Length);
                }
                else
                {
                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}",
                        boundary,
                        param.Key,
                        param.Value);
                    formDataStream.Write(encoding.GetBytes(postData), 0, encoding.GetByteCount(postData));
                }
            }

            // Add the end of the request.  Start with a newline
            string footer = "\r\n--" + boundary + "--\r\n";
            formDataStream.Write(encoding.GetBytes(footer), 0, encoding.GetByteCount(footer));

            // Dump the Stream into a byte[]
            formDataStream.Position = 0;
            byte[] formData = new byte[formDataStream.Length];
            formDataStream.Read(formData, 0, formData.Length);
            formDataStream.Flush();
            formDataStream.Dispose();

            return formData;
        }

    }

    public class FileParameter
    {
        public byte[] File { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public FileParameter(byte[] file) : this(file, null) { }
        public FileParameter(byte[] file, string filename) : this(file, filename, null) { }
        public FileParameter(byte[] file, string filename, string contenttype)
        {
            File = file;
            FileName = filename;
            ContentType = contenttype;
        }
    }
}
