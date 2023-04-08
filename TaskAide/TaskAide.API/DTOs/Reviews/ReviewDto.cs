using System.ComponentModel.DataAnnotations;

namespace TaskAide.API.DTOs.Reviews
{
    public class ReviewDto
    {
        [Range(1,5)]
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
