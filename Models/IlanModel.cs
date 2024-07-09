using System.ComponentModel.DataAnnotations;

namespace IlanSitesi.Models
{
    public class Ilan
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public int Price { get; set; }
        public string Detail { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public IFormFile? Image { get; set; }
        public string? ImgUrl { get; set; }

    }
    public class FormMailModel
    {
        public Ilan Ilan { get; set; }
        public string Subject { get; set; } 
        public string Message { get; set; }
    }
}
