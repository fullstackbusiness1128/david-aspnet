using Amazon.Runtime;
using Amazon.S3.Model;
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;
using System.Threading.Tasks;
using FunTokenz.Models.Data;
using FunTokenz.Models.Business;

namespace FunTokenz.Services
{
    public class S3 : IS3
    {

        private readonly IConfiguration _configuration;
        private readonly FTConfig _ftConfig;


        public S3(IConfiguration configuration, FTConfig ftConfig)
        {
            _configuration = configuration;
            _ftConfig = ftConfig;
        }

        public async Task<bool> sendToS3(string data, string path, string ext)
        {
            try
            {

                string contentType = "image/jpeg";
                if (ext.ToLower() == ".gif")
                {
                    contentType = "image/gif";
                }
                else if (ext.ToLower() == ".png")
                {
                    contentType = "image/png";
                }
                else if (ext.ToLower() == ".pdf")
                {
                    contentType = "application/pdf";
                }
                else if (ext.ToLower() == ".doc")
                {
                    contentType = "application/msword";
                }
                else if (ext.ToLower() == ".xls")
                {
                    contentType = "application/vnd.ms-excel";
                }
                else if (ext.ToLower() == ".docx")
                {
                    contentType = "	application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }
                else if (ext.ToLower() == ".xlsx")
                {
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                }
                else if (ext.ToLower() == ".bmp")
                {
                    contentType = "image/bmp";
                }
                else if (ext.ToLower() == ".tiff")
                {
                    contentType = "image/tiff";
                }

                IAmazonS3 S3Client;
                using (S3Client = new AmazonS3Client())
                {
                    try
                    {

                        PutObjectRequest putRequest = new PutObjectRequest
                        {
                            BucketName = _ftConfig.S3Bucket,
                            ContentType = contentType,
                            Key = "files/" + path + ext,
                            AutoCloseStream = true
                        };

                        byte[] bytes = Convert.FromBase64String(data);

                        using (var ms = new MemoryStream(bytes))
                        {
                            try
                            {
                                putRequest.InputStream = ms;
                                var sendfile = await S3Client.PutObjectAsync(putRequest);
                                return (sendfile.HttpStatusCode == System.Net.HttpStatusCode.OK) ? true : false;
                            }
                            catch (AmazonServiceException ase)
                            {
                                return false;
                            }
                            catch (AmazonClientException ace)
                            {
                                return false;
                            }
                        }
                    }
                    catch (AmazonS3Exception exc)
                    {
                        Console.WriteLine("File Upload Error: " + exc);
                        return false;
                    }
                }
            }
            catch (AmazonS3Exception exc)
            {
                return false;
            }
        }
    }
}
