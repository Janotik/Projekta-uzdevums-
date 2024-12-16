using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;

namespace TeslaRentalPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.Initialize();

            Console.WriteLine("Tesla Rental Platform Initialized!");

            
            Database.AddCar(new Car { Model = "Model 3", HourlyRate = 10, KilometerRate = 0.5 });
            Database.AddCar(new Car { Model = "Model Y", HourlyRate = 15, KilometerRate = 0.7 });

            Database.AddCustomer(new Customer { FullName = "John Doe", Email = "john.doe@example.com" });

            
            Rental rental = new Rental
            {
                CustomerId = 1,
                CarId = 1,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                KilometersDriven = 50
            };

            rental.CalculatePayment(Database.GetCar(rental.CarId));
            Database.AddRental(rental);

            Console.WriteLine("Rental completed! Total payment: " + rental.PaymentAmount + " EUR");
        }
    }

    
    public class Car
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public double HourlyRate { get; set; }
        public double KilometerRate { get; set; }
    }

    
    public class Customer
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
    }

    
    public class Rental
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public int CarId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double KilometersDriven { get; set; }
        public double PaymentAmount { get; set; }

        public void CalculatePayment(Car car)
        {
            double hours = (EndTime - StartTime).TotalHours;
            PaymentAmount = (hours * car.HourlyRate) + (KilometersDriven * car.KilometerRate);
        }

        
    public static class Database
    {
        private const string ConnectionString = "Data Source=tesla_rental.db;Version=3;";

        
        public static void Initialize()
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string createCarsTable = @"CREATE TABLE IF NOT EXISTS Cars (
                                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                            Model TEXT,
                                            HourlyRate REAL,
                                            KilometerRate REAL)";

                string createCustomersTable = @"CREATE TABLE IF NOT EXISTS Customers (
                                                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                                FullName TEXT,
                                                Email TEXT)";

                string createRentalsTable = @"CREATE TABLE IF NOT EXISTS Rentals (
                                              Id INTEGER PRIMARY KEY AUTOINCREMENT,
                                              CustomerId INTEGER,
                                              CarId INTEGER,
                                              StartTime TEXT,
                                              EndTime TEXT,
                                              KilometersDriven REAL,
                                              PaymentAmount REAL,
                                              FOREIGN KEY(CustomerId) REFERENCES Customers(Id),
                                              FOREIGN KEY(CarId) REFERENCES Cars(Id))";

                ExecuteCommand(connection, createCarsTable);
                ExecuteCommand(connection, createCustomersTable);
                ExecuteCommand(connection, createRentalsTable);
            }
        }

        
        public static void AddCar(Car car)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO Cars (Model, HourlyRate, KilometerRate) VALUES (@Model, @HourlyRate, @KilometerRate)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Model", car.Model);
                    command.Parameters.AddWithValue("@HourlyRate", car.HourlyRate);
                    command.Parameters.AddWithValue("@KilometerRate", car.KilometerRate);
                    command.ExecuteNonQuery();
                }
            }
        }

        
        public static Car GetCar(int carId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT * FROM Cars WHERE Id = @Id";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", carId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Car
                            {
                                Id = reader.GetInt32(0),
                                Model = reader.GetString(1),
                                HourlyRate = reader.GetDouble(2),
                                KilometerRate = reader.GetDouble(3)
                            };
                        }
                    }
                }
            }
            return null;
        }

        
        public static void AddCustomer(Customer customer)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = "INSERT INTO Customers (FullName, Email) VALUES (@FullName, @Email)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FullName", customer.FullName);
                    command.Parameters.AddWithValue("@Email", customer.Email);
                    command.ExecuteNonQuery();
                }
            }
        }

        
        public static void AddRental(Rental rental)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                string query = @"INSERT INTO Rentals (CustomerId, CarId, StartTime, EndTime, KilometersDriven, PaymentAmount) 
                                 VALUES (@CustomerId, @CarId, @StartTime, @EndTime, @KilometersDriven, @PaymentAmount)";
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", rental.CustomerId);
                    command.Parameters.AddWithValue("@CarId", rental.CarId);
                    command.Parameters.AddWithValue("@StartTime", rental.StartTime.ToString("o", CultureInfo.InvariantCulture));
                    command.Parameters.AddWithValue("@EndTime", rental.EndTime.ToString("o", CultureInfo.InvariantCulture));
                    command.Parameters.AddWithValue("@KilometersDriven", rental.KilometersDriven);
                    command.Parameters.AddWithValue("@PaymentAmount", rental.PaymentAmount);
                    command.ExecuteNonQuery();
                }
            }
        }

        
        private static void ExecuteCommand(SQLiteConnection connection, string commandText)
        {
            using (var command = new SQLiteCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}
