using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stu_Bussniss;
using Stu_DataAccess;
using System.Security.Claims;

namespace StudentAPIproject.Controllers
{
    [Authorize]
    [Route("api/stuAPI")]
    [ApiController]
    public class stuAPIController : ControllerBase
    {


        private readonly ILogger<AuthController> _logger;

        public stuAPIController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }



        [HttpGet("All", Name = "GetAllStudents")]
        [Authorize(Roles = "Admin")]

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]


        public ActionResult<IEnumerable<StudentDTO>> GetAllStudents()
        {

            List<StudentDTO> students = clsStudent.GetAllStudents();
            if (students.Count == 0)
            {
                return NotFound("No Student Found");
            }
            return Ok(students);
        }

        [AllowAnonymous]
        [HttpGet("Passed", Name = "GetPassedStudents")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<IEnumerable<StudentDTO>> GetPassedStudents()
        {
            var Passedstudents = clsStudent.GetPassedStudent();
            if (Passedstudents.Count == 0)
            {
                return NotFound("No Passed Student Found");
            }

            return Ok(Passedstudents);
        }

        [AllowAnonymous]
        [HttpGet("Avarage", Name = "GetAvgStudents")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<double> GetAvgStudents()
        {
            double avg = clsStudent.GetStudentAvarage();

            if (avg == 0)
            {
                return NotFound("No Student Found");
            }

            return Ok(avg);
        }





        // This endpoint retrieves a single student by ID.
        // It is protected by authentication at the controller level.
        // Authorization logic inside this method enforces ownership rules.
        [HttpGet("{id:int}", Name = "GetStudentById")]

        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StudentDTO>> GetStudentById(
    int id,
    [FromServices] IAuthorizationService authorizationService)
        {
            // Validate the incoming route parameter.
            // IDs less than 1 are not valid and indicate a bad request.
            if (id < 1)
                return BadRequest("Invalid student id.");


            var student = clsStudent.Find(id);


            // If no student exists with this ID, return 404 Not Found.
            // This prevents leaking information about valid IDs.
            if (student == null)
                return NotFound("Student not found.");


            var authResult = await authorizationService.AuthorizeAsync(User, id, "StudentOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid(); // 403

            return Ok(student.StudentDTO);

        }

        [HttpGet("ByEmail/{Email}", Name = "GetStudentByEmail")]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StudentDTO>> GetStudentByEmail(
        string Email,
        [FromServices] IAuthorizationService authorizationService)
        {
            if (string.IsNullOrEmpty(Email))
                return BadRequest("Invalid Email. Email must be a non-empty string.");

            var st = clsStudent.FindByEmail(Email);
            if (st == null)
                return NotFound($"Student with Email {Email} not found.");

            var authResult = await authorizationService.AuthorizeAsync(
                User, Email, "StudentOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            return Ok(st.StudentDTO);
        }

        [HttpPost("Uplode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UplodeImage(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            {
                return BadRequest("No File Uploded");
            }

            var UplodeDic = @"C:\Uploded";

            var FileName = Guid.NewGuid().ToString() + Path.GetExtension(formFile.FileName);
            var filePath = Path.Combine(UplodeDic, FileName);

            if (!Directory.Exists(UplodeDic))
            {

                Directory.CreateDirectory(UplodeDic);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(stream);
            }

            return Ok(new { filePath });


        }

        [HttpGet("GetImage/{filename}")]
        public IActionResult GetImage(string filename)
        {
            var uplodeFile = @"C:\Uploded";
            var filepath = Path.Combine(uplodeFile, filename);

            if (!System.IO.File.Exists(filepath))
            {
                return NotFound();
            }

            var image = System.IO.File.OpenRead(filepath);
            var mimeType = GetMimeType(filepath);

            return File(image, mimeType);



        }

        private string GetMimeType(string PathFile)
        {
            var ext = Path.GetExtension(PathFile).ToLowerInvariant();
            return ext switch
            {
                ".jbg" or ".jpeg" => "image/jpeg",
                ".png" => "image.png",
                ".gif" => "image.gif",
                _ => "application/octet-stream",
            };



        }

        [Authorize(Roles = "Admin")]

        [HttpPost(Name = "AddStudent")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]

        public ActionResult<int> AddStudent(StudentDTO stu)
        {
            if (stu == null || stu.Age < 0 || string.IsNullOrEmpty(stu.Name))
            {
                return BadRequest("Invalid student data.");
            }
            clsStudent student = new clsStudent(stu, clsStudent.eMode.New);
            if (!student.Save())
            {
                return BadRequest("Invalid student data.");

            }


            stu.Id = student.Id;

            return Ok(student.Id);


        }
        [Authorize(Roles = "Admin")]

        [HttpPut("{id}", Name = "UpdateStudent")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public ActionResult<StudentDTO> UpdateStudent(int id, StudentDTO newStudent)
        {



            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // ✅ Identify the admin who is performing the action
            // ClaimTypes.NameIdentifier is what you put in JWT during login.
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";


            if (id < 1 || newStudent == null || string.IsNullOrEmpty(newStudent.Name) || newStudent.Age < 0 || newStudent.grade < 0)
            {
                _logger.LogWarning(
          "Admin action blocked (invalid id). AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, IP={IP}",
          adminId,
          id,
          ip
      );

                return BadRequest("Invalid Data.");
            }

            clsStudent student = clsStudent.Find(id);

            if (student == null)
            {
                _logger.LogWarning(
           "Admin action failed (target not found). AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, IP={IP}",
           adminId,
           id,
           ip
       );
                return NotFound($"Student with {id} Not Found");

            }


            student.Name = newStudent.Name;
            student.Age = newStudent.Age;
            student.Grade = newStudent.grade;
            //student.Password = BCrypt.Net.BCrypt.HashPassword(newStudent.Password);
            student.Email = newStudent.Email;
            student.Role = newStudent.Role;

            if (student.Save())
            {

                return Ok(student.StudentDTO);
            }
            else
            {
                return StatusCode(500, new { message = "Error On Updating" });
            }

        }






        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}", Name = "DeleteStudent")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]

        public ActionResult DeleteStudent(int id)
        {
            // ✅ Capture IP once for tracing (helps investigations later)
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // ✅ Identify the admin who is performing the action
            // ClaimTypes.NameIdentifier is what you put in JWT during login.
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "unknown";


            if (id < 1)
            {

                _logger.LogWarning(
       "Admin action blocked (invalid id). AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, IP={IP}",
       adminId,
       id,
       ip
   );
                return BadRequest("Invalid ID. ID must be a non-negative integer.");
            }

            var student = clsStudent.Find(id);
            if (student == null)
            {
                _logger.LogWarning(
          "Admin action failed (target not found). AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, IP={IP}",
          adminId,
          id,
          ip
      );
                return NotFound("Student not found.");
            }

            if (clsStudent.DeleteStudent(student.Id))
            {

                _logger.LogInformation(
    "Admin action started. AdminId={AdminId}, Action=DeleteStudent, TargetId={TargetId}, TargetEmail={TargetEmail}, IP={IP}",
    adminId,
    student.Id,
    student.Email,
    ip
);

                return Ok($"Student with ID {id} has been deleted successfully.");
            }
            else
            {
                return StatusCode(500, new { message = "Error on Deleting" });
            }



        }
    }
}
