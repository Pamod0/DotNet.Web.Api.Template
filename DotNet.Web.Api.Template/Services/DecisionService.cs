using ASP.NET_Core_Identity.Data;
using ASP.NET_Core_Identity.DTOs.Decision;
using ASP.NET_Core_Identity.DTOs.Meeeting;
using ASP.NET_Core_Identity.Models;
using ASP.NET_Core_Identity.Models.Decisions;
using ASP.NET_Core_Identity.Repositories;
using ASP.NET_Core_Identity.Repositories.Interfaces;
using ASP.NET_Core_Identity.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace ASP.NET_Core_Identity.Services
{
    public class DecisionService : IDecisionService
    {
        private readonly IDecisionRepository _decisionRepository;
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;

        public DecisionService(
            IDecisionRepository decisionRepository,
            IMapper mapper, ApplicationDbContext context,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            IUserRepository userRepository,
            IFileStorageService fileStorageService
            )
        {
            _decisionRepository = decisionRepository;
            _mapper = mapper;
            _context = context;
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<DecisionDto?> GetDecisionByIdAsync(Guid id)
        {
            var decision = await _decisionRepository.GetDecisionByIdAsync(id);

            var decisionDto = _mapper.Map<DecisionDto>(decision);

            var taskCompletion = await GetTaskCompletionForDecisionAsync(id);
            if (taskCompletion != null)
            {
                decisionDto.TaskCompletion = taskCompletion;
            }

            return decisionDto;
        }

        //public async Task<IEnumerable<DecisionDto>> GetAllDecisionsAsync()
        //{
        //    var decisions = await _decisionRepository.GetAllDecisionsAsync();
        //    return _mapper.Map<IEnumerable<DecisionDto>>(decisions);
        //}

        public async Task<PagedResponse<IEnumerable<DecisionDto>>> GetAllDecisionsAsync(PagedRequest request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 10;

            var query = _decisionRepository.GetAllDecisionsQueryable();

            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
            {
                var currentUser = await _userRepository.GetUserByIdAsync(userId);

                // Check if user is admin (assuming role-based check)
                var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;
                var isExcoMember = _httpContextAccessor.HttpContext?.User.IsInRole("ExcoMember") ?? false;

                // If not admin, filter by user's department
                try
                {
                    if ((!isAdmin && !isExcoMember) && currentUser != null)
                    {
                        query = query.Where(m => m.DecisionDepartments.Any(dc => dc.DepartmentId == currentUser.DepartmentId));
                    }
                }
                catch (Exception ex)
                {
                    // Handle potential null reference exceptions or other issues
                    Console.WriteLine($"Error filtering decisions by user department: {ex.Message}");
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
                                             m.ReferenceId.Contains(request.SearchText));
                }
            }

            // Order by a consistent field, e.g., MeetingDate
            query = query.OrderByDescending(m => m.CreatedAt);

            var totalRecords = await query.CountAsync(); // Get total meeting count after filtering

            var meetings = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(m => m.Meeting)
                .ToListAsync();

            var meetingReadDtos = _mapper.Map<IEnumerable<DecisionDto>>(meetings);

            foreach (var decision in meetingReadDtos)
            {
                var meeting = meetings.FirstOrDefault(m => m.Id == decision.Id);
                decision.MeetingDescription = meeting?.Meeting?.Description;
            }

            return new PagedResponse<IEnumerable<DecisionDto>>(request.Page, request.PageSize, totalRecords, meetingReadDtos);
        }

        public async Task<PagedResponse<IEnumerable<DecisionDto>>> GetAllCompletedDecisionsAsync(PagedRequest request)
        {
            if (request.Page < 1) request.Page = 1;
            if (request.PageSize < 1) request.PageSize = 10;

            var query = _decisionRepository.GetAllCompletedDecisionsQueryable();

            var userIdString = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdString) && Guid.TryParse(userIdString, out var userId))
            {
                var currentUser = await _userRepository.GetUserByIdAsync(userId);

                // Check if user is admin (assuming role-based check)
                var isAdmin = _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

                // If not admin, filter by user's department
                try
                {
                    if (!isAdmin && currentUser != null)
                    {
                        query = query.Where(m => m.DecisionDepartments.Any(dc => dc.DepartmentId == currentUser.DepartmentId));
                    }
                }
                catch (Exception ex)
                {
                    // Handle potential null reference exceptions or other issues
                    Console.WriteLine($"Error filtering decisions by user department: {ex.Message}");
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
                                             m.ReferenceId.Contains(request.SearchText));
                }
            }

            // Order by a consistent field, e.g., MeetingDate
            query = query.OrderByDescending(m => m.CreatedAt);

            var totalRecords = await query.CountAsync(); // Get total meeting count after filtering

            var meetings = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Include(m => m.Meeting)
                .ToListAsync();

            var meetingReadDtos = _mapper.Map<IEnumerable<DecisionDto>>(meetings);

            foreach (var decision in meetingReadDtos)
            {
                var meeting = meetings.FirstOrDefault(m => m.Id == decision.Id);
                decision.MeetingDescription = meeting?.Meeting?.Description;
            }

            return new PagedResponse<IEnumerable<DecisionDto>>(request.Page, request.PageSize, totalRecords, meetingReadDtos);
        }

        public async Task<DecisionDto> CreateDecisionAsync(AddDecisionDto addDecisionDto)
        {
            var meeting = await _context.Meetings
                .FirstOrDefaultAsync(m => m.Id == addDecisionDto.MeetingId);

            if (meeting == null)
            {
                throw new ArgumentException("Meeting not found.");
            }

            var decision = _mapper.Map<Decision>(addDecisionDto);

            decision.UpdatedAt = DateTime.UtcNow;

            await _decisionRepository.AddDecisionAsync(decision);

            return _mapper.Map<DecisionDto>(decision);
        }

        public async Task<bool> UpdateDecisionAsync(Guid id, DecisionUpdateDto decisionUpdateDto)
        {
            if (id != decisionUpdateDto.Id)
            {
                return false;
            }

            var existingDecision = await _decisionRepository.GetDecisionByIdAsync(id);
            if (existingDecision == null)
            {
                return false;
            }

            _mapper.Map(decisionUpdateDto, existingDecision);

            if (decisionUpdateDto.DepartmentsIds != null)
            {
                existingDecision.DecisionDepartments.Clear();

                foreach (var departmentId in decisionUpdateDto.DepartmentsIds)
                {
                    var decisionDepartment = new DecisionDepartment
                    {
                        DepartmentId = departmentId,
                        DecisionId = existingDecision.Id
                    };
                    existingDecision.DecisionDepartments.Add(decisionDepartment);
                }
            }

            if (decisionUpdateDto.Tasks != null)
            {
                existingDecision.Tasks.Clear();
                foreach (var taskDto in decisionUpdateDto.Tasks)
                {
                    var task = new Models.Decisions.Task
                    {
                        Name = taskDto.Name,
                        Comment = taskDto.Comment,
                        DecisionId = existingDecision.Id,
                        isCompleted = taskDto.isCompleted
                    };
                    if (taskDto.AssignedDepartmentIds != null)
                    {
                        foreach (var departmentId in taskDto.AssignedDepartmentIds)
                        {
                            var taskDepartment = new TaskDepartment
                            {
                                DepartmentId = departmentId,
                                Task = task
                            };
                            task.TaskDepartments.Add(taskDepartment);
                        }
                    }
                    existingDecision.Tasks.Add(task);
                    existingDecision.Status = DecisionStatus.InProgress;
                }

                var taskDepartments = decisionUpdateDto.Tasks.SelectMany(t => t.AssignedDepartmentIds).Distinct();
                await _notificationService.CreateAndSendTaskUpdateNotification(decisionUpdateDto, taskDepartments);
            }

            existingDecision.UpdatedAt = DateTime.UtcNow;

            await _decisionRepository.UpdateDecisionAsync(existingDecision);

            return true;
        }

        public async Task<bool> UpdateDecisionPartialAsync(Guid id, JsonPatchDocument<UpdateDecisionDto> patchDoc)
        {
            // Retrieve the Decision with its related Tasks and TaskDepartments
            // This is crucial for EF to track the changes correctly.
            var existingDecision = await _context.Decisions
                .Include(d => d.Tasks)
                    .ThenInclude(t => t.TaskDepartments)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existingDecision == null)
            {
                return false;
            }

            // Apply the patch to a new DTO instance, without passing ModelState.
            var decisionToPatch = _mapper.Map<UpdateDecisionDto>(existingDecision);
            patchDoc.ApplyTo(decisionToPatch);

            // Now, manually update the properties on the EF tracked entity.
            // This is the key to preserving the nested collection relationships.
            foreach (var op in patchDoc.Operations)
            {
                if (op.path.StartsWith("/tasks/") && op.op.ToLower() == "replace")
                {
                    var pathSegments = op.path.Split('/');
                    if (pathSegments.Length == 4)
                    {
                        var taskIndex = int.Parse(pathSegments[2]);
                        var propertyToUpdate = pathSegments[3].ToLower();

                        if (taskIndex >= 0 && taskIndex < existingDecision.Tasks.Count)
                        {
                            var taskToUpdate = existingDecision.Tasks.ElementAt(taskIndex);

                            switch (propertyToUpdate)
                            {
                                case "comment":
                                    taskToUpdate.Comment = op.value?.ToString();
                                    break;
                                case "iscompleted":
                                    if (bool.TryParse(op.value?.ToString(), out var isCompleted))
                                    {
                                        taskToUpdate.isCompleted = isCompleted;
                                        existingDecision.Status = DecisionStatus.InProgress;
                                    }
                                    break;
                                    // Add other updatable properties of Task here
                            }
                        }
                    }
                }
                // Handle patches to other top-level properties of Decision, if any
                else
                {
                    switch (op.path.ToLower())
                    {
                        case "/description":
                            existingDecision.Description = op.value?.ToString();
                            break;
                            // Add other updatable top-level properties here
                    }
                }
            }

            // check each task for completion status
            foreach (var task in existingDecision.Tasks)
            {
                if (task.isCompleted)
                {
                    existingDecision.Status = DecisionStatus.Completed;
                }
                else
                {
                    existingDecision.Status = DecisionStatus.InProgress;
                }
            }

            existingDecision.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<bool> DeleteDecisionAsync(Guid id)
        {
            var exists = await _decisionRepository.DecisionExists(id);
            if (!exists)
            {
                return false;
            }
            await _decisionRepository.DeleteDecisionAsync(id);
            return true;
        }

        public async Task<TaskCompletionDto?> GetTaskCompletionForDecisionAsync(Guid decisionId)
        {
            var decision = await _decisionRepository.GetDecisionWithTasksAsync(decisionId);

            if (decision == null)
            {
                return null;
            }

            var completedTasksCount = decision.Tasks.Count(t => t.isCompleted);
            var totalTasksCount = decision.Tasks.Count;

            var taskCompletionText = $"{completedTasksCount}/{totalTasksCount}";

            if (totalTasksCount == 0)
            {
                return new TaskCompletionDto
                {
                    Progress = "0%",
                    TaskCompletionText = "No tasks available."
                };
            }

            var completionPercentage = (int)((completedTasksCount / (double)totalTasksCount) * 100);
            var progress = $"{completionPercentage}%";

            return new TaskCompletionDto
            {
                Progress = progress,
                TaskCompletionText = taskCompletionText
            };
        }

        public async System.Threading.Tasks.Task DeleteFileAsync(Guid fileId)
        {
            var file = await _decisionRepository.GetSupportDocumentByIdAsync(fileId);
            if (file != null)
            {
                _fileStorageService.DeleteFile(file.FilePath);
                _decisionRepository.RemoveSupportDocument(file);
                await _decisionRepository.SaveChangesAsync();
            }
        }


    }
}