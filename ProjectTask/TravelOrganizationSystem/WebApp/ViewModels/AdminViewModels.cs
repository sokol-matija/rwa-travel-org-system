using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApp.Models;

namespace WebApp.ViewModels
{
    #region AdminGuides ViewModels

    public class AdminGuidesIndexViewModel
    {
        public IEnumerable<GuideModel> Guides { get; set; } = new List<GuideModel>();
        public string? ErrorMessage { get; set; }
        public string? SearchFilter { get; set; }
    }

    public class AdminGuideCreateViewModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Biography cannot be longer than 500 characters")]
        [Display(Name = "Biography")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        [Display(Name = "Years of Experience")]
        public int? YearsExperience { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class AdminGuideEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot be longer than 50 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot be longer than 50 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Biography cannot be longer than 500 characters")]
        [Display(Name = "Biography")]
        public string? Bio { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot be longer than 20 characters")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        [Display(Name = "Years of Experience")]
        public int? YearsExperience { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public class AdminGuideDetailsViewModel
    {
        public GuideModel Guide { get; set; } = default!;
        public int TripsCount { get; set; }
        public string? ErrorMessage { get; set; }
    }

    #endregion

    #region AdminGuideAssignments ViewModels

    public class AdminGuideAssignmentsIndexViewModel
    {
        public List<TripModel> Trips { get; set; } = new List<TripModel>();
        public List<GuideModel> Guides { get; set; } = new List<GuideModel>();
        public SelectList TripSelectList { get; set; } = default!;
        public SelectList GuideSelectList { get; set; } = default!;
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
    }

    #endregion

    #region AdminLogs ViewModels

    public class AdminLogsIndexViewModel
    {
        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 100;

        public List<LogModel> Logs { get; set; } = new List<LogModel>();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = DefaultPageSize;
        public string? ErrorMessage { get; set; }
        public bool IsLoading { get; set; }

        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < TotalPages;
        public int StartLogNumber => (Page - 1) * PageSize + 1;
        public int EndLogNumber => Math.Min(Page * PageSize, TotalCount);

        public List<int> GetPaginationNumbers()
        {
            var pageNumbers = new List<int>();

            if (TotalPages <= 7)
            {
                for (int i = 1; i <= TotalPages; i++)
                {
                    pageNumbers.Add(i);
                }
            }
            else
            {
                if (Page <= 4)
                {
                    for (int i = 1; i <= 5; i++)
                    {
                        pageNumbers.Add(i);
                    }
                    if (TotalPages > 6)
                    {
                        pageNumbers.Add(-1);
                        pageNumbers.Add(TotalPages);
                    }
                }
                else if (Page >= TotalPages - 3)
                {
                    pageNumbers.Add(1);
                    if (TotalPages > 6)
                    {
                        pageNumbers.Add(-1);
                    }
                    for (int i = TotalPages - 4; i <= TotalPages; i++)
                    {
                        pageNumbers.Add(i);
                    }
                }
                else
                {
                    pageNumbers.Add(1);
                    pageNumbers.Add(-1);
                    for (int i = Page - 1; i <= Page + 1; i++)
                    {
                        pageNumbers.Add(i);
                    }
                    pageNumbers.Add(-1);
                    pageNumbers.Add(TotalPages);
                }
            }

            return pageNumbers;
        }

        public string GetPageUrl(int pageNumber)
        {
            return $"?page={pageNumber}&pageSize={PageSize}";
        }
    }

    #endregion
}
