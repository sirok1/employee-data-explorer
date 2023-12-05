using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

class Program
{
    private const string XmlFilePath = "employees.xml";

    public static void Main()
    {
        while (true)
        {
            Console.WriteLine("1. Create XML file");
            Console.WriteLine("2. Add/Update employee data");
            Console.WriteLine("3. Search employees by surname");
            Console.WriteLine("4. Display salary payments");
            Console.WriteLine("5. Data analysis");
            Console.WriteLine("6. Export data to XML");
            Console.WriteLine("7. Exit");
            Console.Write("Enter your choice: ");

            var choice = int.Parse(Console.ReadLine()!);

            switch (choice)
            {
                case 1:
                    CreateXmlFile();
                    break;
                case 2:
                    AddOrUpdateEmployeeData();
                    break;
                case 3:
                    SearchEmployeesBySurname();
                    break;
                case 4:
                    DisplaySalaryPayments();
                    break;
                case 5:
                    PerformDataAnalysis();
                    break;
                case 6:
                    ExportDataToXml();
                    break;
                case 7:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    private static void CreateXmlFile()
    {
        var employees = new XElement("Сотрудники");
        employees.Save(XmlFilePath);
        Console.WriteLine("XML file created successfully.");
    }

    private static void AddOrUpdateEmployeeData()
    {
        var xdoc = XDocument.Load(XmlFilePath);

        Console.Write("Enter employee full name: ");
        var fullName = Console.ReadLine()!;

        // Check if the employee already exists
        var existingEmployee = xdoc.Descendants("Сотрудник")
            .FirstOrDefault(e => e.Element("ФИО").Value == fullName);

        if (existingEmployee != null)
        {
            // Employee exists, update data
            Console.Write("Enter new job title: ");
            var jobTitle = Console.ReadLine()!;
            // Update other details as needed...

            existingEmployee.Element("Работа")!.Value = jobTitle;

            xdoc.Save(XmlFilePath);
            Console.WriteLine("Employee data updated successfully.");
        }
        else
        {
            Console.Write("Напишите название занимаемой должности");
            var jobTitle = Console.ReadLine()!;
            Console.Write("Напишите дату вступления в долженость в (DD-MM-YYYY)");
            var startTime = Console.ReadLine()!;
            var startDate = DateTime.ParseExact(startTime, "DD-MM-YYYY", null);
            dynamic jobEnd = "-";
            var isChecked = "not";
            while (isChecked == "not")
            {
                Console.Write("Это его текущая должность ? Напечатайте y или n");
                var answer = Console.ReadLine();
                switch (answer)
                {
                    case "n":
                    {
                        Console.Write("Напишите дату оконччания работы в (DD-MM-YYYY)");
                        var endData = Console.ReadLine()!;
                        jobEnd = DateTime.ParseExact(endData, "DD-MM-YYYY", null);
                        isChecked = "yes";
                        break;
                    }
                    case "y":
                        Console.Write("Работник ещё в должности.");
                        isChecked = "yes";
                        break;
                    default:
                        Console.Write("Попробуйте ещё раз");
                        break;
                }
            }
            Console.Write("Напишите название отдела");
            var departmentName = Console.ReadLine();
            
            var newEmployee = new XElement("Сотрудник",
                new XElement("ФИО", fullName),
                new XElement("Работа",
                    new XElement("Название_должности", Console.ReadLine())));
            xdoc.Element("Сотрудники")!.Add(newEmployee);
            xdoc.Save(XmlFilePath);
            Console.WriteLine("Employee data added successfully.");
        }
    }

    static void SearchEmployeesBySurname()
    {
        XDocument xdoc = XDocument.Load(XmlFilePath);

        Console.Write("Enter employee surname to search: ");
        string surname = Console.ReadLine();

        var matchingEmployees = xdoc.Descendants("Employee")
            .Where(e => e.Element("FullName").Value.EndsWith(" " + surname))
            .ToList();

        if (matchingEmployees.Any())
        {
            foreach (var employee in matchingEmployees)
            {
                Console.WriteLine($"Employee: {employee.Element("FullName").Value}");
                // Display other details...
            }
        }
        else
        {
            Console.WriteLine("No matching employees found.");
        }
    }

    static void DisplaySalaryPayments()
    {
        XDocument xdoc = XDocument.Load(XmlFilePath);

        Console.Write("Enter start date (yyyy-MM): ");
        string startDateString = Console.ReadLine();
        DateTime startDate = DateTime.ParseExact(startDateString, "yyyy-MM", null);

        Console.Write("Enter end date (yyyy-MM): ");
        string endDateString = Console.ReadLine();
        DateTime endDate = DateTime.ParseExact(endDateString, "yyyy-MM", null);

        var salaryPayments = xdoc.Descendants("Salary")
            .Where(s => 
            {
                DateTime paymentDate = DateTime.ParseExact(s.Element("Year").Value + "-" + s.Element("Month").Value, "yyyy-MM", null);
                return paymentDate >= startDate && paymentDate <= endDate;
            })
            .ToList();

        if (salaryPayments.Any())
        {
            foreach (var payment in salaryPayments)
            {
                Console.WriteLine($"Year: {payment.Element("Year").Value}, Month: {payment.Element("Month").Value}, Total Salary: {payment.Element("TotalSalary").Value}");
            }
        }
        else
        {
            Console.WriteLine("No salary payments found for the specified period.");
        }
    }

    static void PerformDataAnalysis()
    {
       XDocument xdoc = XDocument.Load(XmlFilePath);

    // Number of currently active employees
    int activeEmployeesCount = xdoc.Descendants("Employee")
        .Count(e => e.Element("EndDate") == null || DateTime.Parse(e.Element("EndDate").Value) > DateTime.Now);

    Console.WriteLine($"Number of currently active employees: {activeEmployeesCount}");

    // List of unique job positions in each department
    var departments = xdoc.Descendants("Department")
        .Select(d => d.Value)
        .Distinct();

    foreach (var department in departments)
    {
        var uniqueJobPositions = xdoc.Descendants("Employee")
            .Where(e => e.Element("Department").Value == department)
            .Select(e => e.Element("JobTitle").Value)
            .Distinct();

        Console.WriteLine($"Unique job positions in {department} department: {string.Join(", ", uniqueJobPositions)}");
    }

    // Departments with no more than 3 employees
    var departmentsWithFewEmployees = xdoc.Descendants("Department")
        .Where(d => xdoc.Descendants("Employee")
            .Count(e => e.Element("Department").Value == d.Value) <= 3)
        .Select(d => d.Value);

    Console.WriteLine($"Departments with no more than 3 employees: {string.Join(", ", departmentsWithFewEmployees)}");

    // Employees currently working in more than one department
    var employeesInMultipleDepartments = xdoc.Descendants("Employee")
        .GroupBy(e => e.Element("FullName").Value)
        .Where(g => g.Count() > 1)
        .Select(g => g.Key);

    Console.WriteLine($"Employees working in more than one department: {string.Join(", ", employeesInMultipleDepartments)}");

    // Years when the highest and lowest number of employees were hired or fired
    var hiringYears = xdoc.Descendants("Employee")
        .Select(e => DateTime.Parse(e.Element("StartDate").Value).Year)
        .ToList();

    var firingYears = xdoc.Descendants("Employee")
        .Where(e => e.Element("EndDate") != null)
        .Select(e => DateTime.Parse(e.Element("EndDate").Value).Year)
        .ToList();

    int maxHiringYear = hiringYears.GroupBy(y => y).OrderByDescending(g => g.Count()).First().Key;
    int minHiringYear = hiringYears.GroupBy(y => y).OrderBy(g => g.Count()).First().Key;
    int maxFiringYear = firingYears.GroupBy(y => y).OrderByDescending(g => g.Count()).First().Key;
    int minFiringYear = firingYears.GroupBy(y => y).OrderBy(g => g.Count()).First().Key;

    Console.WriteLine($"Year with the highest number of hires: {maxHiringYear}");
    Console.WriteLine($"Year with the lowest number of hires: {minHiringYear}");
    Console.WriteLine($"Year with the highest number of fires: {maxFiringYear}");
    Console.WriteLine($"Year with the lowest number of fires: {minFiringYear}");

    // Identify employees having a jubilee (major anniversary) this year
    var jubileeEmployees = xdoc.Descendants("Employee")
        .Where(e => e.Element("StartDate") != null && DateTime.Now.Year - DateTime.Parse(e.Element("StartDate").Value).Year % 5 == 0);

    Console.WriteLine("Employees with a jubilee this year:");
    foreach (var jubileeEmployee in jubileeEmployees)
    {
        Console.WriteLine($"- {jubileeEmployee.Element("FullName").Value}");
    }
    }

    static void ExportDataToXml()
    {
        XDocument xdoc = XDocument.Load(XmlFilePath);

        // Implement logic to extract analyzed data
        // For the purpose of this example, I'm creating a simple XML structure as an example.
        XElement analyzedData = new XElement("AnalyzedData");

        // Add analyzed data to the XML structure
        analyzedData.Add(new XElement("ActiveEmployeesCount", xdoc.Descendants("Employee")
            .Count(e => e.Element("EndDate") == null || DateTime.Parse(e.Element("EndDate").Value) > DateTime.Now)));

        var departments = xdoc.Descendants("Department")
            .Select(d => d.Value)
            .Distinct();

        foreach (var department in departments)
        {
            var uniqueJobPositions = xdoc.Descendants("Employee")
                .Where(e => e.Element("Department").Value == department)
                .Select(e => e.Element("JobTitle").Value)
                .Distinct();

            analyzedData.Add(new XElement("UniqueJobPositions", new XAttribute("Department", department),
                uniqueJobPositions.Select(j => new XElement("JobPosition", j))));
        }

        var departmentsWithFewEmployees = xdoc.Descendants("Department")
            .Where(d => xdoc.Descendants("Employee")
                .Count(e => e.Element("Department").Value == d.Value) <= 3)
            .Select(d => d.Value);

        analyzedData.Add(new XElement("DepartmentsWithFewEmployees",
            departmentsWithFewEmployees.Select(d => new XElement("Department", d))));

        var employeesInMultipleDepartments = xdoc.Descendants("Employee")
            .GroupBy(e => e.Element("FullName").Value)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key);

        analyzedData.Add(new XElement("EmployeesInMultipleDepartments",
            employeesInMultipleDepartments.Select(e => new XElement("Employee", e))));

        // Add other analyzed data as needed...

        // Save the analyzed data to a new XML file
        analyzedData.Save("analyzed_data.xml");
        Console.WriteLine("Data exported to analyzed_data.xml");
    }

    static void FetchAndDisplayCurrencyRates()
    {
        // Implement logic to fetch and display currency rates
    }
}
