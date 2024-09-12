using CLOUD_FINAL_PROJECT.Data;
using CLOUD_FINAL_PROJECT.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace CLOUD_FINAL_PROJECT.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private ApplicationDbContext _db {  get; set; }

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<S3FileDetails> files = new List<S3FileDetails>();

            files= _db.S3FileDetails.ToList();

            return View(files);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        private string Access_key = "a";
        private string Secret_access_key = "b";



        //upload file to AWS S3 and also make entry in database table
        [HttpPost]
        public async Task<IActionResult> UploadFileToS3(IFormFile file)
        {
            if (file != null)
            {
				var region = Amazon.RegionEndpoint.USEast1;

				using (var amazonS3client = new AmazonS3Client(Access_key,
                    Secret_access_key, new AmazonS3Config { RegionEndpoint = region }))
				{
					using (var memorystream = new MemoryStream())
					{
						file.CopyTo(memorystream);
						var request = new TransferUtilityUploadRequest
						{
							InputStream = memorystream,
							Key = file.FileName,
							BucketName = "bucket-kms-iam",
							ContentType = file.ContentType,
						};

						var transferUtility = new TransferUtility(amazonS3client);
						await transferUtility.UploadAsync(request);
					}
				}
				S3FileDetails fileDetails = new S3FileDetails();
				fileDetails.FileName = file.FileName;
				fileDetails.FileDate = DateTime.Today.Date;

				_db.S3FileDetails.Add(fileDetails);
				_db.SaveChanges();

				ViewBag.Success = "File Uploaded Successfully on S3 Bucket";

				return RedirectToAction(nameof(Index));
			}    
            else {
				string script = "<script>alert('Vui long chon file upload');</script>";
				HttpContext.Response.WriteAsync(script);
				return BadRequest();
			}
            
        }


        public IActionResult DeleteFile(Int32 ID)
        {
            S3FileDetails details = new S3FileDetails();
            details = _db.S3FileDetails.FirstOrDefault(x => x.ID == ID);
            return View(details);
        }



        //Code delete

        [HttpPost]
        public async Task<IActionResult> DeleteFileToS3(string Filename)
        {
            var region = Amazon.RegionEndpoint.USEast1;
            using (var amazonS3client = new AmazonS3Client(Access_key,
                Secret_access_key, new AmazonS3Config { RegionEndpoint = region }))
            {
                var transferUtility = new TransferUtility(amazonS3client);
                await transferUtility.S3Client.DeleteObjectAsync(new DeleteObjectRequest()
                {
                    BucketName = "bucket-kms-iam",
                    Key = Filename
                });


                S3FileDetails filedetails = new S3FileDetails();
                filedetails = _db.S3FileDetails.FirstOrDefault(x => x.FileName.ToLower() == Filename.ToLower());
                _db.S3FileDetails.Remove(filedetails);

                _db.SaveChanges();

                ViewBag.Success = "File Deleted Successfully on S3";
                return RedirectToAction(nameof(Index));

            }

        }

        public IActionResult ViewFileForDownload(Int32 ID)
        {
            S3FileDetails details = new S3FileDetails();
            details = _db.S3FileDetails.FirstOrDefault(x => x.ID == ID);
            return View(details);
        }

        public async Task<IActionResult> DownloadFile(string Filename)
        {
            var region = Amazon.RegionEndpoint.USEast1;
            using (var amazonS3client = new AmazonS3Client(Access_key,
                Secret_access_key, new AmazonS3Config { RegionEndpoint = region }))
            {
                var transferUtility = new TransferUtility(amazonS3client);
                // Ensure that Filename is set as the Key
                if (string.IsNullOrEmpty(Filename))
                {
                    return BadRequest("Filename cannot be null or empty.");
                }

                var response = await transferUtility.S3Client.GetObjectAsync(new
                    GetObjectRequest()
                {
                    BucketName = "bucket-kms-iam",
                    Key = Filename
                });

                if (response.ResponseStream == null)
                {
                    return NotFound();
                }
                return File(response.ResponseStream, response.Headers.ContentType, Filename);
            }
        }

        public IActionResult ShareFile(Int32 ID)
        {
            S3FileDetails details = new S3FileDetails();
            details = _db.S3FileDetails.FirstOrDefault(x => x.ID == ID);
            return View(details);
        }

        //string Url;

        //Url = "https://final-bucket-cloud.s3.amazonaws.com/" + Filename;

        //return File(Url)


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
