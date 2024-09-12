using System.ComponentModel.DataAnnotations;

namespace CLOUD_FINAL_PROJECT.Models
{
    public class S3FileDetails
    {
        [Key]
        public int ID {  get; set; }
        public DateTime FileDate { get; set; }
        public string FileName { get; set; }
    }
}
