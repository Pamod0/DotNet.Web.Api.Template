using ASP.NET_Core_Identity.DTOs.Department;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Auth;
using ASP.NET_Core_Identity.Models.Decisions;
using ASP.NET_Core_Identity.Models.FileUploads;
using ASP.NET_Core_Identity.Repositories.Interfaces;
using ASP.NET_Core_Identity.Services.Interfaces;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ASP.NET_Core_Identity.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly IMeetingRepository _meetingRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly IEmailService _emailService;
        //private readonly IUserService _userService;

        public MeetingService(
            IMeetingRepository meetingRepository,
            IDepartmentRepository departmentRepository,
            IFileStorageService fileStorageService,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            IEmailService emailService
            //IUserService userService
            )
        {
            _meetingRepository = meetingRepository;
            _departmentRepository = departmentRepository;
            _fileStorageService = fileStorageService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _emailService = emailService;
            //_userService = userService;
        }

        public async Task<MeetingDto?> GetMeetingByIdAsync(Guid id)
        {
            var meeting = await _meetingRepository.GetMeetingByIdAsync(id, true);
            if (meeting == null)
            {
                return null;
            }

            // Map to DTO using AutoMapper
            var meetingDto = _mapper.Map<MeetingDto>(meeting);

            meetingDto.Participants = meeting.MeetingDepartments
                .Select(md => new DepartmentDto
                {
                    Id = md.Department.Id,
                    Name = md.Department.Name
                }).ToList();
            meetingDto.Decision = meeting.Decisions
                .Select(d => new MeetingDecisionDto
                {
                    Id = d.Id,
                    ReferenceId = d.ReferenceId,
                    Description = d.Description,
                    Status = d.Status

                }).ToList();


            return meetingDto;
        }

        public async Task<PagedResponse<IEnumerable<MeetingDto>>> GetAllMeetingsAsync(PagedRequest request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 10;

            var query = _meetingRepository.GetAllMeetingsQueryable(); // Get IQueryable from repository  

            // Get current user's ID and department
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
            {
                var currentUser = await _userRepository.GetUserByIdAsync(userId);

                // Check if user is admin (assuming role-based check)
                var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
                var isExcoMember = _httpContextAccessor.HttpContext?.User.IsInRole("ExcoMember") ?? false;
                var isUser = _httpContextAccessor.HttpContext?.User.IsInRole("User") ?? false;

                // If not admin, filter by user's department
                if ((!isAdmin && !isExcoMember && !isUser) && currentUser != null)
                {
                    query = query.Where(m => m.MeetingDepartments.Any(md => md.DepartmentId == currentUser.Department.Id));
                }
            }

            // Apply search filter  
            if (!string.IsNullOrEmpty(request.SearchText))
            {
                if (request.ExactMatch)
                {
                    query = query.Where(m => m.Description == request.SearchText);
                }
                else
                {
                    query = query.Where(m => m.Description.Contains(request.SearchText) ||
                                             m.Decisions.Any(d => d.Description.Contains(request.SearchText)));
                }
            }

            // Order by a consistent field, e.g., MeetingDate  
            query = query.OrderByDescending(m => m.MeetingDate);

            var totalRecords = await query.CountAsync(); // Get total meeting count after filtering  

            var meetings = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(m => m.Decisions) // Include Decisions to calculate count  
                .ToListAsync(); // Fetch paginated results  

            var meetingDtos = meetings.Select(meeting =>
            {
                var meetingDto = _mapper.Map<MeetingDto>(meeting);

                meetingDto.Participants = meeting.MeetingDepartments
                    .Select(md => new DepartmentDto
                    {
                        Id = md.Department.Id,
                        Name = md.Department.Name,
                        ShortName = md.Department.ShortName
                    }).ToList();

                meetingDto.DecisionsCount = meeting.Decisions.Count(d => !d.IsDeleted);

                return meetingDto;
            }).ToList();

            return new PagedResponse<IEnumerable<MeetingDto>>(request.Page, request.PageSize, totalRecords, meetingDtos);
        }

        //public async Task<PagedResponse<IEnumerable<MeetingDto>>> GetAllMeetingsAsync(PagedRequest request)
        //{
        //    if (request.Page < 1) request.Page = 1;
        //    if (request.PageSize < 1) request.PageSize = 10;

        //    var query = _meetingRepository.GetAllMeetingsQueryable(); // Get IQueryable from repository  

        //    // Apply search filter  
        //    if (!string.IsNullOrEmpty(request.SearchText))
        //    {
        //        if (request.ExactMatch)
        //        {
        //            query = query.Where(m => m.Description == request.SearchText);
        //        }
        //        else
        //        {
        //            query = query.Where(m => m.Description.Contains(request.SearchText) ||
        //                                     m.Decisions.Any(d => d.Description.Contains(request.SearchText)));
        //        }
        //    }

        //    // Order by a consistent field, e.g., MeetingDate  
        //    query = query.OrderByDescending(m => m.MeetingDate);

        //    var totalRecords = await query.CountAsync(); // Get total meeting count after filtering  

        //    var meetings = await query
        //        .Skip((request.Page - 1) * request.PageSize)
        //        .Take(request.PageSize)
        //        .Include(m => m.Decisions) // Include Decisions to calculate count  
        //        .ToListAsync(); // Fetch paginated results  

        //    var meetingDtos = meetings.Select(meeting =>
        //    {
        //        var meetingDto = _mapper.Map<MeetingDto>(meeting);

        //        meetingDto.Participants = meeting.MeetingDepartments
        //            .Select(md => new DepartmentDto
        //            {
        //                Id = md.Department.Id,
        //                Name = md.Department.Name,
        //                ShortName = md.Department.ShortName
        //            }).ToList();

        //        meetingDto.DecisionsCount = meeting.Decisions.Count(d => !d.IsDeleted);

        //        return meetingDto;
        //    }).ToList();

        //    return new PagedResponse<IEnumerable<MeetingDto>>(request.Page, request.PageSize, totalRecords, meetingDtos);
        //}

        public async Task<IEnumerable<MeetingDropdownDto>> GetMeetingDropdownAsync()
        {
            var meetingDropdown = await _meetingRepository.GetMeetingDropwdownAsync();

            if (meetingDropdown == null || !meetingDropdown.Any())
            {
                return new List<MeetingDropdownDto>();
            }

            return meetingDropdown;
        }

        public async Task<MeetingDto> CreateMeetingAsync(AddMeetingDto addMeetingDto)
        {
            var meeting = _mapper.Map<Meeting>(addMeetingDto);

            // Get current user's ID
            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                throw new InvalidOperationException("User ID is required to create a meeting.");
            }

            // Convert userIdString to Guid
            if (!Guid.TryParse(userIdString, out var userId))
            {
                throw new InvalidOperationException("Invalid User ID format.");
            }

            // BaseEntity properties (CreatedUserId, CreatedAt) will be handled by ApplicationDbContext's SaveChangesAsync override
            // Set audit fields
            //meeting.CreatedUserId = userId;
            //meeting.UpdatedUserId = userId; // Initially, CreatedBy and UpdatedBy are the same
            //meeting.CreatedAt = DateTime.UtcNow; // Set creation timestamp
            meeting.UpdatedAt = DateTime.UtcNow; // Set initial update timestamp

            // Handle Meeting Minutes file upload (SupportDocument)
            if (addMeetingDto.MeetingMinutesFile != null)
            {
                var (fileName, filePath) = await _fileStorageService.SaveFileAsync(addMeetingDto.MeetingMinutesFile, "Meeting");
                meeting.SupportDocuments = new List<SupportDocument>()
                {
                    new SupportDocument
                    {
                        FileName = fileName,
                        FilePath = filePath,
                        ContentType = addMeetingDto.MeetingMinutesFile.ContentType,
                        FileSize = addMeetingDto.MeetingMinutesFile.Length,
                        Description = $"Meeting minutes for meeting on {addMeetingDto.MeetingDate}",
                        // Explicitly set DecisionId and TaskId to null for meeting minutes
                        DecisionId = null, // <-- Ensure this is null
                        TaskId = null      // <-- Ensure this is null
                    }
                };
            }

            // Handle Participants (MeetingDepartments)
            if (addMeetingDto.DepartmentIds != null && addMeetingDto.DepartmentIds.Any())
            {
                var departments = await _departmentRepository.GetDepartmentsByIdsAsync(addMeetingDto.DepartmentIds);
                foreach (var department in departments)
                {
                    meeting.MeetingDepartments.Add(new MeetingDepartment
                    {
                        MeetingId = meeting.Id,
                        DepartmentId = department.Id,
                        Department = department
                    });
                }
            }

            var createdMeeting = await _meetingRepository.AddMeetingAsync(meeting);
            var meetingDto = _mapper.Map<MeetingDto>(createdMeeting);

            if (addMeetingDto.DepartmentIds != null)
            {
                foreach (var department in addMeetingDto.DepartmentIds)
                {
                    var departmentUsers = (await _userRepository.GetAllUsersInDepartmentAsync(department)).ToList();

                    var subject = $"New Meeting Scheduled on {addMeetingDto.MeetingDate}";

                    var body = $"""
                        <div style="font-family:Segoe UI,Arial,sans-serif;font-size:15px;color:#222;">
                            <h2 style="color:#2a5d9f;">New Meeting Scheduled</h2>
                            <p>
                                <strong>Date:</strong> {addMeetingDto.MeetingDate}<br/>
                                <strong>Start Time:</strong> {addMeetingDto.StartTime}<br/>
                                {(addMeetingDto.EndTime.HasValue ? $"<strong>End Time:</strong> {addMeetingDto.EndTime}<br/>" : "")}
                                <strong>Description:</strong> {addMeetingDto.Description}
                            </p>
                            <p style="margin-top:20px;">
                                Please check the system for more details.
                            </p>
                        </div>
                        """;

                    foreach (var user in departmentUsers)
                    {
                        _ = _emailService.SendEmailAsync(user.Email, subject, body);
                    }
                }
            }

            return meetingDto;
        }

        public async Task<bool> UpdateMeetingAsync(UpdateMeetingDto updateMeetingDto)
        {
            var existingMeeting = await _meetingRepository.GetMeetingByIdAsync(updateMeetingDto.Id, includeRelated: true);

            if (existingMeeting == null)
            {
                return false;
            }

            if (updateMeetingDto.MeetingDate.HasValue)
            {
                existingMeeting.MeetingDate = updateMeetingDto.MeetingDate.Value;
            }
            if (updateMeetingDto.StartTime.HasValue)
            {
                existingMeeting.StartTime = updateMeetingDto.StartTime.Value;
            }
            if (updateMeetingDto.EndTime.HasValue)
            {
                existingMeeting.EndTime = updateMeetingDto.EndTime.Value;
            }
            if (updateMeetingDto.Description != null) // strings are reference types, so check for null
            {
                existingMeeting.Description = updateMeetingDto.Description;
            }
            if (updateMeetingDto.SendNotificationToParticipants.HasValue)
            {
                existingMeeting.SendNotificationToParticipants = updateMeetingDto.SendNotificationToParticipants.Value;
            }

            if (updateMeetingDto.MeetingMinutesFile != null)
            {
                var (newFileName, newFilePath) = await _fileStorageService.SaveFileAsync(updateMeetingDto.MeetingMinutesFile, "Meeting");

                var existingSupportDocuments = await _meetingRepository.GetSupportDocumentsAsync(updateMeetingDto.Id);

                var newSupportDocument = new SupportDocument
                {
                    FileName = newFileName,
                    FilePath = newFilePath,
                    ContentType = updateMeetingDto.MeetingMinutesFile.ContentType,
                    FileSize = updateMeetingDto.MeetingMinutesFile.Length,
                    Description = $"Meeting minutes for meeting on {updateMeetingDto.MeetingDate}",
                    MeetingId = existingMeeting.Id,
                    DecisionId = null,
                    TaskId = null
                };

                existingMeeting.SupportDocuments = existingSupportDocuments.Concat(new List<SupportDocument> { newSupportDocument }).ToList();
            }

            // This condition is for explicitly removing the file if the client sends `MeetingMinutesFile: null`
            // and an existing file is present. If they simply omit the field, this won't trigger.
            // For file uploads in multipart/form-data, if a file input is left empty, it's often null.
            // If you want to explicitly signal removal, you might need a separate boolean flag in the DTO
            // e.g., `bool RemoveMeetingMinutesFile { get; set; }`
            //else if (updateMeetingDto.MeetingMinutesFile == null && existingMeeting.MeetingSupportDocument != null)
            //{
            //    // This branch assumes null means "remove existing file".
            //    // Be explicit if this interpretation isn't clear from client side.
            //    _fileStorageService.DeleteFile(existingMeeting.MeetingSupportDocument.FilePath);
            //    _meetingRepository.RemoveSupportDocument(existingMeeting.MeetingSupportDocument);
            //    existingMeeting.MeetingSupportDocument = null;
            //}


            // Handle Department relationship updates (Many-to-Many)
            // This logic assumes that if DepartmentIds is provided (not null),
            // it means the client wants to REPLACE all existing departments with the new list.
            // If DepartmentIds is null, it means no change to departments.
            if (updateMeetingDto.DepartmentIds != null)
            {
                _meetingRepository.RemoveMeetingDepartments(existingMeeting.MeetingDepartments);
                existingMeeting.MeetingDepartments.Clear(); // Clear the collection in the entity

                foreach (var departmentId in updateMeetingDto.DepartmentIds)
                {
                    existingMeeting.MeetingDepartments.Add(new MeetingDepartment
                    {
                        MeetingId = existingMeeting.Id,
                        DepartmentId = departmentId
                    });
                }
            }

            try
            {
                // The repository's UpdateMeetingAsync will now track changes on the existingMeeting
                await _meetingRepository.UpdateMeetingAsync(existingMeeting);
                await _meetingRepository.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error updating meeting: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        public async Task<bool> DeleteMeetingAsync(Guid id)
        {
            var meetingToDelete = await _meetingRepository.GetMeetingByIdAsync(id, includeRelated: true); // Include related for file deletion  

            if (meetingToDelete == null)
            {
                return false;
            }

            await _meetingRepository.DeleteMeetingAsync(meetingToDelete.Id);
            await _meetingRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> MeetingExistsAsync(Guid id)
        {
            return await _meetingRepository.MeetingExists(id);
        }

        public async Task<MeetingDto?> GetLatestMeetingAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var meeting = await _meetingRepository
                .GetAllMeetingsQueryable()
                .Where(m => m.MeetingDate >= today)
                .OrderBy(m => m.MeetingDate)
                .FirstOrDefaultAsync();

            if (meeting == null)
            {
                return null;
            }

            var meetingDto = _mapper.Map<MeetingDto>(meeting);
            meetingDto.Participants = meeting.MeetingDepartments
                .Select(md => new DepartmentDto
                {
                    Id = md.Department.Id,
                    Name = md.Department.Name
                }).ToList();

            return meetingDto;
        }

        public async System.Threading.Tasks.Task DeleteFileAsync(Guid fileId)
        {
            var file = await _meetingRepository.GetSupportDocumentByIdAsync(fileId);
            if (file != null)
            {
                _fileStorageService.DeleteFile(file.FilePath);
                _meetingRepository.RemoveSupportDocument(file);
                await _meetingRepository.SaveChangesAsync();
            }
        }

    }
}
