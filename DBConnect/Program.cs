using DBConnect.Models;
using DBConnect.Data;
using Microsoft.Extensions.Configuration;

namespace DBConnect
{
    internal class Program
    {
        static void Main(string[] args)
        {

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            DataContextDapper dapper = new DataContextDapper(configuration);
            DataContextEf entityFramework = new DataContextEf(configuration);

            string sqlCommand = "SELECT GETDATE()";
            DateTime abhi = dapper.LoadDataSingle<DateTime>(sqlCommand);
            Console.WriteLine(abhi);

            Computer myComputer = new Computer(){
                Motherboard = "With_ef",
                CPUCores = 200,
                HasWifi = true,
                HasLTE = true,
                ReleaseDate = DateTime.Now,
                Price = 1000.00M,
                VideoCard = "Nvidia"
            };

            entityFramework.Add(myComputer);
            entityFramework.SaveChanges();

            // sqlCommand = @"INSERT INTO TutorialAppSchema.Computer (Motherboard, CPUCores, HasWifi, HasLTE, ReleaseDate, Price, VideoCard)
            //                 VALUES ('"+myComputer.Motherboard+"', "+myComputer.CPUCores+", "+Convert.ToInt32(myComputer.HasWifi)+", "+Convert.ToInt32(myComputer.HasLTE)+", '"+myComputer.ReleaseDate+"', "+myComputer.Price+", '"+myComputer.VideoCard+"')";

            // Console.WriteLine(sqlCommand);
            // int result = dbConnection.Execute(sqlCommand);

            // Use parametrized queries for code readability and security
            sqlCommand = @"INSERT INTO TutorialAppSchema.Computer (Motherboard, CPUCores, HasWifi, HasLTE, ReleaseDate, Price, VideoCard)
                            VALUES (@Motherboard, @CPUCores, @HasWifi, @HasLTE, @ReleaseDate, @Price, @VideoCard)";
            
            int result=dapper.SaveDataWithRowCount<Computer>(sqlCommand, myComputer);
            Console.WriteLine($"Rows affected: {result}");

            sqlCommand = "SELECT * FROM TutorialAppSchema.Computer";
            IEnumerable<Computer> computers = dapper.LoadData<Computer>(sqlCommand);

             foreach (Computer computer in computers)
            {
                Console.WriteLine(computer.Motherboard);
                Console.WriteLine(computer.CPUCores);
                Console.WriteLine(computer.HasWifi);
                Console.WriteLine(computer.HasLTE);
                Console.WriteLine(computer.ReleaseDate);
                Console.WriteLine(computer.Price);
                Console.WriteLine(computer.VideoCard);
                Console.WriteLine("--------------------");
            }

            IEnumerable<Computer>? computersEf = entityFramework.Computer?.ToList();

            if(computersEf != null){
                foreach (Computer computer in computersEf)
                {
                    Console.WriteLine(computer.Motherboard);
                    Console.WriteLine(computer.CPUCores);
                    Console.WriteLine(computer.HasWifi);
                    Console.WriteLine(computer.HasLTE);
                    Console.WriteLine(computer.ReleaseDate);
                    Console.WriteLine(computer.Price);
                    Console.WriteLine(computer.VideoCard);
                    Console.WriteLine("--------------------");
                }
            }
        }
    }
}