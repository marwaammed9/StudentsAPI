using Stu_DataAccess;
using System.ComponentModel.Design;
using System.Security.Cryptography;

namespace Stu_Bussniss
{
    public class clsStudent
    {

        public string Name { get; set; }
        public int Id { get; set; }
        public int Grade { get; set; }
        public int Age { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }



        public string RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }



        public clsStudent(string name, int id, int age, int grade, string Email, string Role, string Password)
        {
            this.Name = name;
            this.Grade = grade;
            this.Id = id;
            this.Age = age;
            this.Email = Email;
            this.Role = Role;
            this.Password = BCrypt.Net.BCrypt.HashPassword(Password);

        }
        public StudentDTO StudentDTO
        {
            get
            {
                return (new StudentDTO(Name, Id, Age, Grade, Email, Role, Password));
            }
        }

      
        public enum eMode { New = 1, update = 2 };
        eMode Mode = eMode.update;


        public clsStudent(StudentDTO stu, eMode Mode = eMode.New)
        {
            this.Name = stu.Name;
            this.Grade = stu.grade;
            this.Id = stu.Id;
            this.Age = stu.Age;
            this.Email = stu.Email;
            this.Role = stu.Role;


            this.Mode = Mode;

            if (Mode == eMode.update)
            {
                this.Password = stu.Password;
            }
            else
            {
                this.Password = BCrypt.Net.BCrypt.HashPassword(stu.Password);
            }

        }

        private bool _AddNewStudent()
        {
            this.Id = StuDataAccess.AddStudent(StudentDTO);
            return (this.Id != -1);

        }

        private bool _Update()
        {
            return StuDataAccess.UpdateStudent(StudentDTO);

        }
        public static bool DeleteStudent(int id)
        {
            return StuDataAccess.DeleteStudent(id);
        }
        public bool Save()
        {
            switch (Mode)
            {
                case eMode.New:
                    if (_AddNewStudent())
                    {
                        this.Mode = eMode.update;
                        return true;
                    }
                    else
                    {
                        return false;
                    }


                case eMode.update:
                    if (_Update())
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

            }

            return false;

        }


        public static List<StudentDTO> GetAllStudents()
        {
            return StuDataAccess.GetAllStudents();
        }

        public static List<StudentDTO> GetPassedStudent()
        {
            return StuDataAccess.GetPassedStudents();
        }

        public static double GetStudentAvarage()
        {
            return StuDataAccess.GetAvarage();
        }

        public static string GenerateRefreshToken(string Email)
        {

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

            bool saved = StuDataAccess.SaveRefreshToken(
                Email, BCrypt.Net.BCrypt.HashPassword(token), DateTime.UtcNow.AddDays(7));

            return saved ? token : null;


        }

        public bool TokenRevoked()
        {
            return StuDataAccess.RevokeRefreshToken(this.Email);
        }

        public static StudentRefreshTokenDTO GetRefreshToken(string Email)
        {
            return StuDataAccess.GetRefreshTokenByEmail(Email);
        }


        public static clsStudent Find(int id)
        {
            StudentDTO stuDOT = StuDataAccess.FindStudent(id);
            if (stuDOT == null)
            {
                return null;
            }

            return new clsStudent(stuDOT, eMode.update);
        }
        public static clsStudent FindByEmail(string Email)
        {
            StudentDTO stuDOT = StuDataAccess.FindStudentByEmail(Email);
            if (stuDOT == null)
            {
                return null;
            }

            return new clsStudent(stuDOT, eMode.update);
        }


    }
}
