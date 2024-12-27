using System.Data;
using Dapper;
using DotnetAPI.Data;
using DotnetAPI.Models;

namespace DotnetAPI.Helpers
{
    public class ReusableSql
    {
        private readonly DataContextDapper _dapper;
        public ReusableSql(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
        }

        public bool UpsertUser(UserComplete user)
        {
            string sql = @"EXEC TutorialAppSchema.spUser_Upsert
            @FirstName = @FirstNameParam, 
            @LastName = @LastNameParam,
            @Email = @EmailParam,
            @Gender = @GenderParam,
            @Active = @ActiveParam,
            @JobTitle = @JobTitleParam,
            @Department = @DepartmentParam,
            @Salary = @SalaryParam,
            @UserId = @UserIdParam";

            DynamicParameters sqlParams = new DynamicParameters();
            sqlParams.Add("@FirstNameParam", user.FirstName, DbType.String);
            sqlParams.Add("@LastNameParam", user.LastName, DbType.String);
            sqlParams.Add("@EmailParam", user.Email, DbType.String);
            sqlParams.Add("@GenderParam", user.Gender, DbType.Boolean);
            sqlParams.Add("@ActiveParam", user.Active, DbType.Boolean);
            sqlParams.Add("@JobTitleParam", user.JobTitle, DbType.String);
            sqlParams.Add("@DepartmentParam", user.Department, DbType.String);
            sqlParams.Add("@SalaryParam", user.Salary, DbType.Decimal);
            sqlParams.Add("@UserIdParam", user.UserId, DbType.Int32);

            return _dapper.ExecuteSqlWithParams(sql, sqlParams);
            
        }
    }
}