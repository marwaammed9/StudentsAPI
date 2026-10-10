using Microsoft.Data.SqlClient;
using System.Data;

namespace Stu_DataAccess
{

    public class StudentDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }

        public int grade { get; set; }

        public string Email { get; set; }
        public string Role { get; set; }

        public string Password { get; set; }



        public StudentDTO(string name, int id, int age, int grade, string Email, string Role, string Password)
        {
            this.Name = name;
            this.grade = grade;
            this.Id = id;
            this.Age = age;
            this.Email = Email;
            this.Role = Role;
            this.Password = Password;

        }
        public StudentDTO()
        {
        }

    }

    public class StudentRefreshTokenDTO
    {
        public string Email { get; set; }

        public string RefreshTokenHash { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }

        public StudentRefreshTokenDTO(string email, string refreshTokenHash, DateTime? expiresAt, DateTime? revokedAt)
        {
            Email = email;
            RefreshTokenHash = refreshTokenHash;
            ExpiresAt = expiresAt;
            RevokedAt = revokedAt;
        }

    }
    public class StuDataAccess
    {
        private static string ConnectionString = "   Server=YOUR_SERVER;Database=StudentsDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;";
        public static List<StudentDTO> GetAllStudents()
        {
            var students = new List<StudentDTO>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetAllStudents", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new StudentDTO(
                                reader.GetString(reader.GetOrdinal("Name")),
                                reader.GetInt32(reader.GetOrdinal("Id")),
                                reader.GetInt32(reader.GetOrdinal("Age")),
                                reader.GetInt32(reader.GetOrdinal("Grade")),
                                reader.GetString(reader.GetOrdinal("Email")),
                                reader.GetString(reader.GetOrdinal("Role")),
                                reader.GetString(reader.GetOrdinal("HasdPassword"))

                                ));

                        }


                    }
                }
            }

            return students;


        }

        public static List<StudentDTO> GetPassedStudents()
        {
            var Passedstudents = new List<StudentDTO>();
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("SP_GetPassedStudents", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Passedstudents.Add(new StudentDTO(
                                reader.GetString(reader.GetOrdinal("Name")),
                                reader.GetInt32(reader.GetOrdinal("Id")),
                                reader.GetInt32(reader.GetOrdinal("Age")),
                                reader.GetInt32(reader.GetOrdinal("Grade")),
                                reader.GetString(reader.GetOrdinal("Email")),
                                reader.GetString(reader.GetOrdinal("Role")),
                                reader.GetString(reader.GetOrdinal("HasdPassword"))

                                ));

                        }


                    }
                }
            }

            return Passedstudents;
        }

        public static double GetAvarage()
        {
            double Avg = 0;

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_GetAverageGrade", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        conn.Open();
                        object reader = cmd.ExecuteScalar();
                        if (reader != DBNull.Value)
                        {
                            Avg = Convert.ToDouble(reader.ToString());
                        }
                        else
                        {
                            Avg = 0;
                        }

                    }


                }
            }
            catch (Exception ex)
            {
                return 0;
            }

            return Avg;
        }

        public static int AddStudent(StudentDTO student)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_AddStudent", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Name", student.Name);
                        cmd.Parameters.AddWithValue("@Age", student.Age);
                        cmd.Parameters.AddWithValue("@Grade", student.grade);
                        cmd.Parameters.AddWithValue("@Email", student.Email);
                        cmd.Parameters.AddWithValue("@Role", student.Role);
                        cmd.Parameters.AddWithValue("@Password", student.Password);


                        var output = new SqlParameter("@NewStudentId", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output,
                        };
                        cmd.Parameters.Add(output);

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        return (int)output.Value;

                    }
                }

            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
                return -1;

            }

        }

        public static bool UpdateStudent(StudentDTO stuDTO)
        {

            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_UpdateStudent", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@StudentId", stuDTO.Id);
                        cmd.Parameters.AddWithValue("@Name", stuDTO.Name);
                        cmd.Parameters.AddWithValue("@Grade", stuDTO.grade);
                        cmd.Parameters.AddWithValue("@Age", stuDTO.Age);
                        cmd.Parameters.AddWithValue("@Password", stuDTO.Password);
                        cmd.Parameters.AddWithValue("@Email", stuDTO.Email);
                        cmd.Parameters.AddWithValue("@Role", stuDTO.Role);


                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();
                        return rows > 0;

                    }
                }
            }

            catch (SqlException ex)
            {

                return false;

            }
        }
        public static StudentDTO FindStudent(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_GetStudentById", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Studentid", id);
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new StudentDTO(
                                  reader.GetString(reader.GetOrdinal("Name")),
                                   reader.GetInt32(reader.GetOrdinal("id")),
                                  reader.GetInt32(reader.GetOrdinal("Age")),
                                  reader.GetInt32(reader.GetOrdinal("Grade")),
                                  reader.GetString(reader.GetOrdinal("Email")),
                                   reader.GetString(reader.GetOrdinal("Role")),
                                 reader.GetString(reader.GetOrdinal("HasdPassword"))
                                   );
                            }
                            else
                            {
                                return null;
                            }


                        }

                    }
                }

            }
            catch (SqlException ex)
            {
                return null;
            }
        }


        public static StudentDTO FindStudentByEmail(string Email)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_GetStudentByEmail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Email", Email);
                        conn.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new StudentDTO(
                                  reader.GetString(reader.GetOrdinal("Name")),
                                   reader.GetInt32(reader.GetOrdinal("id")),
                                  reader.GetInt32(reader.GetOrdinal("Age")),
                                  reader.GetInt32(reader.GetOrdinal("Grade")),
                                  reader.GetString(reader.GetOrdinal("Email")),
                                   reader.GetString(reader.GetOrdinal("Role")),
                                 reader.GetString(reader.GetOrdinal("HasdPassword"))
                                   );
                            }
                            else
                            {
                                return null;
                            }


                        }

                    }
                }
            }
            catch (SqlException ex)
            {
                return null;
            }



        }


        public static bool SaveRefreshToken(string Email, string tokenHash, DateTime expiresAt)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SP_SaveRefreshToken", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", Email);
                cmd.Parameters.AddWithValue("@RefreshTokenHash", tokenHash);
                cmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public static StudentRefreshTokenDTO GetRefreshTokenByEmail(string email)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SP_GetRefreshTokenByEmail", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;

                    int iHash = reader.GetOrdinal("RefreshTokenHash");
                    int iExp = reader.GetOrdinal("RefreshTokenExpiresAt");
                    int iRev = reader.GetOrdinal("RefreshTokenRevokedAt");

                    return new StudentRefreshTokenDTO(
                        email,
                        reader.IsDBNull(iHash) ? null : reader.GetString(iHash),
                        reader.IsDBNull(iExp) ? (DateTime?)null : reader.GetDateTime(iExp),
                        reader.IsDBNull(iRev) ? (DateTime?)null : reader.GetDateTime(iRev));

                }
            }
        }


        public static bool RevokeRefreshToken(string email)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SP_RevokeRefreshToken", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Email", email);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
        public static bool DeleteStudent(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ConnectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("SP_DeleteStudent", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@StudentId", id);

                        conn.Open();
                        int rows = (int)cmd.ExecuteScalar();
                        return rows == 1;
                    }
                }

            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }


        }



    }
}

