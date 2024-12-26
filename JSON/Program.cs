using System;
using System.Text.Json;
using JSON.Data;
using JSON.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace JSON
{
    class Program
    {
        static void Main()
        {
            IConfiguration config = new ConfigurationBuilder().
                AddJsonFile("appsettings.json").
                Build();

            DataContextDapper dapper = new DataContextDapper(config);    

            string computersJson = File.ReadAllText("Computers.json");
            Console.WriteLine(computersJson);          

            JsonSerializerOptions options = new JsonSerializerOptions{
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };

            IEnumerable<Computer>? computersSystemText = System.Text.Json.JsonSerializer.Deserialize<IEnumerable<Computer>>(computersJson,options);

            if(computersSystemText != null){
                foreach(Computer computer in computersSystemText){
                    Console.WriteLine(computer.Motherboard);
                }
            }

            string computersCopySystemText = System.Text.Json.JsonSerializer.Serialize(computersSystemText,options);
            File.WriteAllText("ComputersCopySystemText.json", computersCopySystemText);

            Console.WriteLine("---Newtonsoft---");

            IEnumerable<Computer>? computersNewtonsoft= JsonConvert.DeserializeObject<IEnumerable<Computer>>(computersJson);

            if(computersNewtonsoft != null){
                foreach(Computer computer in computersNewtonsoft){
                    string sql = "INSERT INTO TutorialAppSchema.Computer (Motherboard, CPUCores, HasWifi, HasLTE, ReleaseDate, Price, VideoCard) VALUES (@Motherboard, @CPUCores, @HasWifi, @HasLTE, @ReleaseDate, @Price, @VideoCard)";
                    dapper.SaveData(sql, computer);
                }
            }

            JsonSerializerSettings settings = new JsonSerializerSettings{
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
            };

            string computersCopyNewtonsoft = JsonConvert.SerializeObject(computersNewtonsoft,settings);
            File.WriteAllText("ComputersCopyNewtonsoft.json", computersCopyNewtonsoft);
                    
        }
    }
}